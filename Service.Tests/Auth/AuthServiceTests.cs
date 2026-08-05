using System.Linq.Expressions;
using AutoMapper;
using Core.DTOs.Auth;
using Core.Entities;
using Core.Enums;
using Core.Exceptions;
using Core.Repositories;
using Core.Services;
using Core.UnitOfWork;
using Core.Validations.Auth;
using FluentAssertions;
using Moq;
using Service.Mapping;
using Service.Services.Auth;

namespace Service.Tests.Auth;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IPasswordService> _password = new();
    private readonly Mock<ITokenService> _token = new();
    private readonly IMapper _mapper;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        _uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _password.Setup(x => x.HashPassword(It.IsAny<string>())).Returns("hashed-password");
        _token.Setup(x => x.CreateToken(It.IsAny<User>(), out It.Ref<DateTime>.IsAny))
            .Returns((User _, out DateTime expiresAt) =>
            {
                expiresAt = DateTime.UtcNow.AddHours(1);
                return "test-token";
            });

        _sut = new AuthService(
            _userRepo.Object,
            _uow.Object,
            _password.Object,
            _token.Object,
            _mapper,
            new RegisterRequestValidator(),
            new LoginRequestValidator());
    }

    [Fact]
    public async Task RegisterAsync_NewEmail_CreatesUserAndReturnsToken()
    {
        _userRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(false);

        var request = new RegisterRequest
        {
            FirstName = "Ali",
            LastName = "Yilmaz",
            Email = "ali@test.com",
            Password = "Secret123",
            LicenseNumber = "L12345",
            Role = UserRole.Customer
        };

        var result = await _sut.RegisterAsync(request);

        result.Token.Should().Be("test-token");
        result.Email.Should().Be("ali@test.com");
        result.Role.Should().Be(UserRole.Customer);
        _userRepo.Verify(x => x.AddAsync(It.Is<User>(u =>
            u.Email == "ali@test.com" &&
            u.PasswordHash == "hashed-password")), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ExistingEmail_ThrowsConflict()
    {
        _userRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(true);

        var request = new RegisterRequest
        {
            FirstName = "Ali",
            LastName = "Yilmaz",
            Email = "ali@test.com",
            Password = "Secret123",
            LicenseNumber = "L12345"
        };

        var act = async () => await _sut.RegisterAsync(request);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already registered*");
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "ali@test.com",
            FirstName = "Ali",
            LastName = "Yilmaz",
            PasswordHash = "hashed-password",
            Role = UserRole.Customer,
            LicenseNumber = "L12345"
        };

        _userRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(user);
        _password.Setup(x => x.VerifyPassword("Secret123", "hashed-password")).Returns(true);

        var result = await _sut.LoginAsync(new LoginRequest
        {
            Email = "ali@test.com",
            Password = "Secret123"
        });

        result.Token.Should().Be("test-token");
        result.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorized()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "ali@test.com",
            FirstName = "Ali",
            LastName = "Yilmaz",
            PasswordHash = "hashed-password",
            Role = UserRole.Customer,
            LicenseNumber = "L12345"
        };

        _userRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(user);
        _password.Setup(x => x.VerifyPassword("Wrong", "hashed-password")).Returns(false);

        var act = async () => await _sut.LoginAsync(new LoginRequest
        {
            Email = "ali@test.com",
            Password = "Wrong"
        });

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*Invalid email or password*");
    }
}
