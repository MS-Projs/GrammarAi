namespace GrammarAi.Application.Common.DTOs;

public record SolveResultDto(
    decimal Score,
    decimal MaxScore,
    decimal AccuracyPercent,
    List<AnswerBreakdownDto> Breakdown
);

public record AnswerBreakdownDto(
    Guid QuestionId,
    bool? IsCorrect,
    decimal Score,
    List<Guid> CorrectAnswerIds,
    string? Explanation
);
