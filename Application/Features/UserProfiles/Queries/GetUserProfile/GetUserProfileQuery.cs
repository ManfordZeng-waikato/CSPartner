using Application.Common.Interfaces;
using Application.DTOs.UserProfile;

namespace Application.Features.UserProfiles.Queries.GetUserProfile;

public record GetUserProfileQuery(Guid UserId, Guid? CurrentUserId = null) : IQuery<UserProfileDto?>;

