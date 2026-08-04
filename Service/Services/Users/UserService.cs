using AutoMapper;
using Core.DTOs.Users;
using Core.Entities;
using Core.Exceptions;
using Core.Repositories;
using Core.Services;
using Core.UnitOfWork;
using Service.Services.Common;
using FluentValidation;
using ValidationException = Core.Validations.ValidationException;

namespace Service.Services.Users;

public class UserService : GenericService<User>, IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordService _passwordService;
    private readonly IValidator<UpdateUserProfileRequest> _updateProfileValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;

    public UserService(
        IUserRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPasswordService passwordService,
        IValidator<UpdateUserProfileRequest> updateProfileValidator,
        IValidator<ChangePasswordRequest> changePasswordValidator)
        : base(repository, unitOfWork)
    {
        _userRepository = repository;
        _mapper = mapper;
        _passwordService = passwordService;
        _updateProfileValidator = updateProfileValidator;
        _changePasswordValidator = changePasswordValidator;
    }

    public async Task<UserProfileResponse> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User profile was not found.");

        return _mapper.Map<UserProfileResponse>(user);
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request)
    {
        var validationResult = await _updateProfileValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User profile was not found.");

        _mapper.Map(request, user);
        _userRepository.Update(user);
        await UnitOfWork.SaveChangesAsync();

        return _mapper.Map<UserProfileResponse>(user);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var validationResult = await _changePasswordValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User profile was not found.");

        if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedException("Current password is incorrect.");
        }

        user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
        _userRepository.Update(user);
        await UnitOfWork.SaveChangesAsync();
    }
}
