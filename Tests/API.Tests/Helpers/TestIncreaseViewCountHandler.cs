using Application.Common.Interfaces;
using Application.Features.Videos.Commands.IncreaseViewCount;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace API.Tests.Helpers;

public class TestIncreaseViewCountHandler : IRequestHandler<IncreaseViewCountCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public TestIncreaseViewCountHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(IncreaseViewCountCommand request, CancellationToken cancellationToken)
    {
        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == request.VideoId && !v.IsDeleted, cancellationToken);
        video?.IncreaseView();
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
