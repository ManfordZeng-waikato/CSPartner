using Application.Common.Interfaces;
using Application.DTOs.UserProfile;
using Application.DTOs.Video;
using Application.Features.Videos.Queries.GetVideosByUserId;
using Application.Mappings;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.UserProfiles.Queries.GetUserProfileByUserId;

public class GetUserProfileByUserIdQueryHandler : IRequestHandler<GetUserProfileByUserIdQuery, UserProfileDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;

    public GetUserProfileByUserIdQueryHandler(IApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<UserProfileDto?> Handle(GetUserProfileByUserIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (profile == null)
            throw new UserProfileNotFoundException(request.UserId);

        var videoDtos = await _mediator.Send(
            new GetVideosByUserIdQuery(request.UserId, request.CurrentUserId),
            cancellationToken);

        return profile.ToDto(videoDtos.ToList());
    }
}

