namespace GrammarAi.Application.Common.DTOs;

public record ExerciseDto(
    Guid Id,
    string? Title,
    string? Description,
    string ExerciseType,
    string? Difficulty,
    string Status,
    bool IsPublic,
    List<string> Tags,
    DateTimeOffset CreatedAt
);

public record ExerciseDetailDto(
    Guid Id,
    string? Title,
    string? Description,
    string ExerciseType,
    string? Difficulty,
    string Status,
    bool IsPublic,
    List<string> Tags,
    List<QuestionDto> Questions,
    DateTimeOffset CreatedAt
);

public record QuestionDto(
    Guid Id,
    int OrderIndex,
    string Body,
    string? Explanation,
    string ExerciseType,
    int MaxScore,
    List<AnswerDto> Answers
);

public record AnswerDto(
    Guid Id,
    int OrderIndex,
    string Text,
    bool IsCorrect,
    string? Explanation
);

public record ImageUploadResultDto(Guid ImageId, string FileUrl, int PageNumber);

public record ExerciseStatusDto(Guid ExerciseId, string Status, string? Error);
