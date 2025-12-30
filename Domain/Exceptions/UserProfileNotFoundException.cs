namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when a user profile is not found
/// </summary>
public class UserProfileNotFoundException : DomainException
{
    public UserProfileNotFoundException(Guid userId) 
        : base($"User profile for user {userId} does not exist")
    {
    }
}

