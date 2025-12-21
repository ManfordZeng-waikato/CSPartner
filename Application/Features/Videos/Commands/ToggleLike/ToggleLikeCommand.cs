using Application.Common.Interfaces;

namespace Application.Features.Videos.Commands.ToggleLike;

public record ToggleLikeCommand(Guid VideoId) : ICommand<bool>;

