using Application.Common.Interfaces;
using Application.DTOs.Ai;
using MediatR;

namespace Application.Features.Videos.Commands.GenerateVideoAiMeta;
public record GenerateVideoAiMetaCommand(Guid VideoId) : ICommand<VideoAiResultDto>;