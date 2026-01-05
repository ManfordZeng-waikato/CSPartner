using Domain.Videos;

namespace Application.DTOs.Ai;

public sealed record VideoAiResultDto(
    string Description,
    HighlightType HighlightType
);
