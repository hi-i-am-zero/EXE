namespace AutoWork.Application.DTOs.Users;

public class CreateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
}
