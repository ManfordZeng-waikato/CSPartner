using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Videos.Commands.IncreaseViewCount;

public class IncreaseViewCountCommandHandler : IRequestHandler<IncreaseViewCountCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public IncreaseViewCountCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(IncreaseViewCountCommand request, CancellationToken cancellationToken)
    {
        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == request.VideoId && !v.IsDeleted, cancellationToken);

        if (video != null)
        {
            video.IncreaseView();
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}

