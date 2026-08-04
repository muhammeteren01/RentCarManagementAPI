using Core.Enums;

namespace Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; }
    public string LicenseNumber { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}
