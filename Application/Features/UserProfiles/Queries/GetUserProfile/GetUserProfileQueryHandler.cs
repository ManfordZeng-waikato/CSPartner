using Application.Common.Interfaces;
using Application.DTOs.UserProfile;
using Application.DTOs.Video;
using Application.Features.Videos.Queries.GetVideosByUserId;
using Application.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.UserProfiles.Queries.GetUserProfile;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;

    public GetUserProfileQueryHandler(IApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<UserProfileDto?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.Id == request.UserId, cancellationToken);

        if (profile == null)
            return null;

        var videoDtos = await _mediator.Send(
            new GetVideosByUserIdQuery(profile.UserId, request.CurrentUserId),
            cancellationToken);

        return profile.ToDto(videoDtos.ToList());
    }
}

