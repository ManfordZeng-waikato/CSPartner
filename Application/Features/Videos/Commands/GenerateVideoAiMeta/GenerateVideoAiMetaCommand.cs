using Application.Common.Interfaces;
using Application.DTOs.Ai;
using Domain.Videos;
using MediatR;

namespace Application.Features.Videos.Commands.GenerateVideoAiMeta;
public record GenerateVideoAiMetaCommand(Guid VideoId, string? Map = null, string? Weapon = null, HighlightType? HighlightType = null) : ICommand<VideoAiResultDto>;