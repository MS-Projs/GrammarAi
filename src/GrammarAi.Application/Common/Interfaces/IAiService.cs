using GrammarAi.Application.Common.DTOs;

namespace GrammarAi.Application.Common.Interfaces;

public interface IAiService
{
    Task<string> ExtractTextFromImageAsync(byte[] imageBytes, CancellationToken ct = default);
    Task<ParsedExerciseDto> ParseExerciseAsync(string ocrText, CancellationToken ct = default);
    Task<bool> EvaluateEssayAnswerAsync(string questionBody, string userAnswer, CancellationToken ct = default);
}
