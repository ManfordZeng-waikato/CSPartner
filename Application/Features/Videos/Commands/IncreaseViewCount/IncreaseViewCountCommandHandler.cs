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
        await _context.Videos
       .Where(v => v.Id == request.VideoId && !v.IsDeleted)
       .ExecuteUpdateAsync(
           s => s.SetProperty(
               v => v.ViewCount,
               v => v.ViewCount + 1),
           cancellationToken);

        return Unit.Value;
    }
}

