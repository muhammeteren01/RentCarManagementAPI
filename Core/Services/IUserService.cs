using Core.DTOs.Users;
using Core.Entities;

namespace Core.Services;

public interface IUserService : IGenericService<User>
{
    Task<UserProfileResponse> GetProfileAsync(Guid userId);
    Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}
