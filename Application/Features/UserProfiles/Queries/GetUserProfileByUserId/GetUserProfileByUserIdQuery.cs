using Application.Common.Interfaces;
using Application.DTOs.UserProfile;

namespace Application.Features.UserProfiles.Queries.GetUserProfileByUserId;

public record GetUserProfileByUserIdQuery(Guid UserId, Guid? CurrentUserId = null) : IQuery<UserProfileDto?>;

