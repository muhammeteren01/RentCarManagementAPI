using AutoMapper;
using Core.DTOs.Auth;
using Core.Entities;
using Core.Exceptions;
using Core.Repositories;
using Core.Services;
using Core.UnitOfWork;
using FluentValidation;
using ValidationException = Core.Validations.ValidationException;

namespace Service.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        ITokenService tokenService,
        IMapper mapper,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _mapper = mapper;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var validationResult = await _registerValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        if (await _userRepository.AnyAsync(u => u.Email == request.Email))
        {
            throw new ConflictException("Email is already registered.");
        }

        var user = _mapper.Map<User>(request);
        user.Id = Guid.NewGuid();
        user.PasswordHash = _passwordService.HashPassword(request.Password);
        user.Role = request.Role;
        user.CreatedAt = DateTime.UtcNow;

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user is null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        return CreateAuthResponse(user);
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        var token = _tokenService.CreateToken(user, out var expiresAt);

        return new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role
        };
    }
}
