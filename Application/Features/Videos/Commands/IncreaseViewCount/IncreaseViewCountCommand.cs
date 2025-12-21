using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Videos.Commands.IncreaseViewCount;

public record IncreaseViewCountCommand(Guid VideoId) : ICommand<Unit>;

