using Application.Common.Interfaces;
using Application.DTOs.UserProfile;
using Application.DTOs.Video;
using Application.Features.UserProfiles.Queries.GetUserProfileByUserId;
using Application.Mappings;
using Domain.Exceptions;
using Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.UserProfiles.Commands.CreateOrUpdateUserProfile;

public class CreateOrUpdateUserProfileCommandHandler : IRequestHandler<CreateOrUpdateUserProfileCommand, UserProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public CreateOrUpdateUserProfileCommandHandler(
        IApplicationDbContext context,
        IMediator mediator,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public async Task<UserProfileDto> Handle(CreateOrUpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw AuthenticationRequiredException.ForOperation("update profile");

        var userId = _currentUserService.UserId.Value;
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile == null)
        {
            profile = new UserProfile(userId);
            await _context.UserProfiles.AddAsync(profile, cancellationToken);
        }

        profile.Update(
            request.DisplayName,
            request.Bio,
            request.AvatarUrl,
            request.SteamUrl,
            request.FaceitUrl);

        var profileDto = await _mediator.Send(
            new GetUserProfileByUserIdQuery(userId, userId),
            cancellationToken);

        return profileDto!;
    }
}

