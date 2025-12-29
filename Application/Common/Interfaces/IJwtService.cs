namespace Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(Guid userId, string email, IEnumerable<string> roles);
}

