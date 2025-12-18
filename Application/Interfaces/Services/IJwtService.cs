namespace Application.Interfaces.Services;

public interface IJwtService
{
    string GenerateToken(Guid userId, string email, IEnumerable<string> roles);
}

