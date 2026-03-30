using System.Text.Json;
using System.Text.Json.Serialization;
using GrammarAi.Application.Common.DTOs;
using GrammarAi.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace GrammarAi.Infrastructure.Services;

public class OpenAiService(IConfiguration config, ILogger<OpenAiService> logger) : IAiService
{
    private readonly string _model = config["OpenAI:Model"] ?? "gpt-4o";

    private ChatClient CreateClient() =>
        new(_model, config["OpenAI:ApiKey"]!);

    private const string ParseExerciseSystemPrompt = """
        You are an English exercise parser. Given OCR text from a worksheet, return a JSON object.

        Rules:
        - Detect exercise type from cues: A/B/C options = multiple_choice, blanks (___ or gaps) = fill_blank,
          numbered sequences = reorder, Yes/No statements = true_false, open-ended = essay
        - If an answer key is present in the text, set is_correct=true for correct options
        - If no answer key, set is_correct=false for all options
        - Estimate CEFR difficulty (A1/A2/B1/B2/C1/C2) from vocabulary and grammar complexity
        - Clean any OCR artifacts from question text
        - question body must include any relevant context sentence

        Return ONLY this JSON structure (no markdown):
        {
          "title": "string",
          "exercise_type": "multiple_choice|fill_blank|reorder|true_false|essay",
          "difficulty": "A1|A2|B1|B2|C1|C2",
          "questions": [
            {
              "order": 1,
              "body": "string",
              "explanation": "string or null",
              "answers": [{ "text": "string", "is_correct": true }]
            }
          ]
        }
        """;

    public async Task<string> ExtractTextFromImageAsync(byte[] imageBytes, CancellationToken ct = default)
    {
        var client = CreateClient();
        var base64 = Convert.ToBase64String(imageBytes);

        var messages = new List<ChatMessage>
        {
            new UserChatMessage(
                ChatMessageContentPart.CreateTextPart("Extract ALL text from this image exactly as written. Preserve line breaks and numbering. Return raw text only."),
                ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(imageBytes), "image/png"))
        };

        var response = await client.CompleteChatAsync(messages, cancellationToken: ct);
        return response.Value.Content[0].Text ?? string.Empty;
    }

    public async Task<ParsedExerciseDto> ParseExerciseAsync(string ocrText, CancellationToken ct = default)
    {
        var client = CreateClient();

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(ParseExerciseSystemPrompt),
            new UserChatMessage(ocrText)
        };

        var options = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
        };

        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                var response = await client.CompleteChatAsync(messages, options, ct);
                var json = response.Value.Content[0].Text;
                var raw = JsonSerializer.Deserialize<RawParsedExercise>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? throw new InvalidOperationException("Null response from AI.");

                return new ParsedExerciseDto(
                    raw.Title ?? "Untitled Exercise",
                    raw.ExerciseType ?? "multiple_choice",
                    raw.Difficulty,
                    raw.Questions?.Select(q => new ParsedQuestionDto(
                        q.Order,
                        q.Body ?? string.Empty,
                        q.Explanation,
                        q.Answers?.Select(a => new ParsedAnswerDto(a.Text ?? string.Empty, a.IsCorrect)).ToList() ?? []
                    )).ToList() ?? []);
            }
            catch (Exception ex) when (attempt < 2)
            {
                logger.LogWarning(ex, "AI parse attempt {Attempt} failed, retrying...", attempt + 1);
                await Task.Delay(1000 * (attempt + 1), ct);
            }
        }

        throw new InvalidOperationException("Failed to parse exercise after 3 attempts.");
    }

    public async Task<bool> EvaluateEssayAnswerAsync(string questionBody, string userAnswer, CancellationToken ct = default)
    {
        var client = CreateClient();
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are an English teacher. Evaluate if the student's answer is correct or acceptable. Reply with exactly one word: 'correct' or 'incorrect'."),
            new UserChatMessage($"Question: {questionBody}\nStudent answer: {userAnswer}")
        };

        var response = await client.CompleteChatAsync(messages, cancellationToken: ct);
        var result = response.Value.Content[0].Text?.Trim().ToLower() ?? "incorrect";
        return result.StartsWith("correct");
    }

    // Internal deserialization types
    private record RawParsedExercise(
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("exercise_type")] string? ExerciseType,
        [property: JsonPropertyName("difficulty")] string? Difficulty,
        [property: JsonPropertyName("questions")] List<RawQuestion>? Questions);

    private record RawQuestion(
        [property: JsonPropertyName("order")] int Order,
        [property: JsonPropertyName("body")] string? Body,
        [property: JsonPropertyName("explanation")] string? Explanation,
        [property: JsonPropertyName("answers")] List<RawAnswer>? Answers);

    private record RawAnswer(
        [property: JsonPropertyName("text")] string? Text,
        [property: JsonPropertyName("is_correct")] bool IsCorrect);
}
