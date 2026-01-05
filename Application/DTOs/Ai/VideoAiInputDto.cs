using Domain.Videos;

namespace Application.DTOs.Ai;
public sealed record VideoAiInputDto(
    string Title,
    string? UserDescription,
    string? Map,
    string? Mode,
    string? Weapon,
    string? ExtraContext,
    HighlightType? HighlightType = null
);