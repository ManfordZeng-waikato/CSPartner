using Domain.Videos;

namespace Application.DTOs.Ai;

public sealed record VideoAiResultDto(
    string Description,
    IReadOnlyList<string> Tags,
    HighlightType HighlightType
);
