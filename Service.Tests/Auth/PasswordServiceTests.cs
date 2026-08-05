using Core.Services;
using FluentAssertions;
using Service.Services.Auth;

namespace Service.Tests.Auth;

public class PasswordServiceTests
{
    private readonly PasswordService _sut = new();

    [Fact]
    public void HashPassword_ReturnsNonEmptyHashDifferentFromPlaintext()
    {
        var hash = _sut.HashPassword("Secret123");

        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe("Secret123");
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var hash = _sut.HashPassword("Secret123");

        _sut.VerifyPassword("Secret123", hash).Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var hash = _sut.HashPassword("Secret123");

        _sut.VerifyPassword("WrongPassword", hash).Should().BeFalse();
    }
}
