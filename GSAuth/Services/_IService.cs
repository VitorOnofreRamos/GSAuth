using GSAuth.DTOs;
using GSAuth.Models;

namespace GSAuth.Services;

public interface _IService {}

public interface IUserService : _IService
{
    Task<User> GetByIdAsync(long id);
    Task<User> GetByEmailAsync(string name);
    Task<User> CreateAsync(User user);
    Task UpdateAsync (User user);
    Task DeleteAsync (long id);
    Task<IEnumerable<User>> GetAllAsync();
}

public interface IAuthService : _IService
{
    Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDto);
    Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto);
    Task<bool> ChangePasswordAsync(long userId, ChangePasswordDTO changePasswordDto);
    Task<bool> ValidateTokenAsync(string token);
    Task<UserDTO> GetCurrentUserAsync(long userId);
}
