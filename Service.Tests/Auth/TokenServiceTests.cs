using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Core.Entities;
using Core.Enums;
using Core.Settings;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Service.Services.Auth;

namespace Service.Tests.Auth;

public class TokenServiceTests
{
    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        var settings = Options.Create(new JwtSettings
        {
            Secret = "unit-test-secret-key-at-least-32-chars!",
            Issuer = "RentCar.Test",
            Audience = "RentCar.Test.Client",
            ExpirationInMinutes = 60
        });

        _sut = new TokenService(settings);
    }

    [Fact]
    public void CreateToken_ReturnsJwtWithExpectedClaims()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "customer@test.com",
            FirstName = "Ali",
            LastName = "Yilmaz",
            Role = UserRole.Customer,
            LicenseNumber = "L123",
            PasswordHash = "hash"
        };

        var token = _sut.CreateToken(user, out var expiresAt);

        token.Should().NotBeNullOrWhiteSpace();
        expiresAt.Should().BeAfter(DateTime.UtcNow);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Issuer.Should().Be("RentCar.Test");
        jwt.Audiences.Should().Contain("RentCar.Test.Client");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == nameof(UserRole.Customer));
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
    }
}
