using Application.Common.Interfaces;

namespace Application.Features.Videos.Commands.DeleteVideo;

public record DeleteVideoCommand(Guid VideoId) : ICommand<bool>;

