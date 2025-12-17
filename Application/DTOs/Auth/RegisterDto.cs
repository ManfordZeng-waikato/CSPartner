using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class RegisterDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password), ErrorMessage = "两次密码输入需一致")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [StringLength(50)]
    public string? DisplayName { get; set; }
}

