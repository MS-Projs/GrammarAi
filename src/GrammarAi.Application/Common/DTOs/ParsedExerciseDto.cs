namespace GrammarAi.Application.Common.DTOs;

public record ParsedExerciseDto(
    string Title,
    string ExerciseType,
    string? Difficulty,
    List<ParsedQuestionDto> Questions
);

public record ParsedQuestionDto(
    int Order,
    string Body,
    string? Explanation,
    List<ParsedAnswerDto> Answers
);

public record ParsedAnswerDto(string Text, bool IsCorrect);
