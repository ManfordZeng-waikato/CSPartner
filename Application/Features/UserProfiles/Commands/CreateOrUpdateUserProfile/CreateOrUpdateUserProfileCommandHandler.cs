using Application.Common.Interfaces;
using Application.DTOs.UserProfile;
using Application.DTOs.Video;
using Application.Features.UserProfiles.Queries.GetUserProfileByUserId;
using Application.Mappings;
using Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.UserProfiles.Commands.CreateOrUpdateUserProfile;

public class CreateOrUpdateUserProfileCommandHandler : IRequestHandler<CreateOrUpdateUserProfileCommand, UserProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;

    public CreateOrUpdateUserProfileCommandHandler(IApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<UserProfileDto> Handle(CreateOrUpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (profile == null)
        {
            profile = new UserProfile(request.UserId);
            await _context.UserProfiles.AddAsync(profile, cancellationToken);
        }

        profile.Update(
            request.DisplayName,
            request.Bio,
            request.AvatarUrl,
            request.SteamUrl,
            request.FaceitUrl);

        await _context.SaveChangesAsync(cancellationToken);

        var profileDto = await _mediator.Send(
            new GetUserProfileByUserIdQuery(request.UserId, request.CurrentUserId ?? request.UserId),
            cancellationToken);

        return profileDto!;
    }
}

