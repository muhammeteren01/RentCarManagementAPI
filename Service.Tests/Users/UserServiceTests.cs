using AutoMapper;
using Core.DTOs.Users;
using Core.Entities;
using Core.Enums;
using Core.Exceptions;
using Core.Repositories;
using Core.Services;
using Core.UnitOfWork;
using Core.Validations.Users;
using FluentAssertions;
using Moq;
using Service.Mapping;
using Service.Services.Users;

namespace Service.Tests.Users;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IPasswordService> _password = new();
    private readonly IMapper _mapper;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        _uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _sut = new UserService(
            _userRepo.Object,
            _uow.Object,
            _mapper,
            _password.Object,
            new UpdateUserProfileRequestValidator(),
            new ChangePasswordRequestValidator());
    }

    [Fact]
    public async Task GetProfileAsync_ExistingUser_ReturnsProfile()
    {
        var userId = Guid.NewGuid();
        _userRepo.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User
        {
            Id = userId,
            FirstName = "Ali",
            LastName = "Yilmaz",
            Email = "ali@test.com",
            LicenseNumber = "L123",
            Role = UserRole.Customer,
            PasswordHash = "hash"
        });

        var result = await _sut.GetProfileAsync(userId);

        result.FirstName.Should().Be("Ali");
        result.Email.Should().Be("ali@test.com");
        result.LicenseNumber.Should().Be("L123");
    }

    [Fact]
    public async Task GetProfileAsync_MissingUser_ThrowsNotFound()
    {
        var userId = Guid.NewGuid();
        _userRepo.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        var act = async () => await _sut.GetProfileAsync(userId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateProfileAsync_UpdatesAllowedFieldsOnly()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            FirstName = "Ali",
            LastName = "Yilmaz",
            Email = "ali@test.com",
            LicenseNumber = "OLD",
            Role = UserRole.Customer,
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        _userRepo.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        var result = await _sut.UpdateProfileAsync(userId, new UpdateUserProfileRequest
        {
            FirstName = "Veli",
            LastName = "Demir",
            LicenseNumber = "NEW123"
        });

        result.FirstName.Should().Be("Veli");
        result.LastName.Should().Be("Demir");
        result.LicenseNumber.Should().Be("NEW123");
        user.Email.Should().Be("ali@test.com");
        user.Role.Should().Be(UserRole.Customer);
        user.PasswordHash.Should().Be("hash");
        _userRepo.Verify(x => x.Update(user), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongCurrentPassword_ThrowsUnauthorized()
    {
        var userId = Guid.NewGuid();
        _userRepo.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User
        {
            Id = userId,
            FirstName = "Ali",
            LastName = "Yilmaz",
            Email = "ali@test.com",
            LicenseNumber = "L123",
            Role = UserRole.Customer,
            PasswordHash = "old-hash"
        });
        _password.Setup(x => x.VerifyPassword("wrong", "old-hash")).Returns(false);

        var act = async () => await _sut.ChangePasswordAsync(userId, new ChangePasswordRequest
        {
            CurrentPassword = "wrong",
            NewPassword = "NewSecret1",
            ConfirmPassword = "NewSecret1"
        });

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*Current password is incorrect*");
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidRequest_UpdatesHash()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            FirstName = "Ali",
            LastName = "Yilmaz",
            Email = "ali@test.com",
            LicenseNumber = "L123",
            Role = UserRole.Customer,
            PasswordHash = "old-hash"
        };

        _userRepo.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _password.Setup(x => x.VerifyPassword("OldSecret", "old-hash")).Returns(true);
        _password.Setup(x => x.HashPassword("NewSecret1")).Returns("new-hash");

        await _sut.ChangePasswordAsync(userId, new ChangePasswordRequest
        {
            CurrentPassword = "OldSecret",
            NewPassword = "NewSecret1",
            ConfirmPassword = "NewSecret1"
        });

        user.PasswordHash.Should().Be("new-hash");
        _userRepo.Verify(x => x.Update(user), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
