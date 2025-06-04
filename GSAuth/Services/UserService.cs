using GSAuth.Models;
using GSAuth.Repositories;

namespace GSAuth.Services;

public class UserService : _Service, IUserService
{
    private readonly _IRepository<User> _userRepository;

    public UserService(_IRepository<User> userRepository) : base(userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> GetByIdAsync(long id)
    {
        try
        {
            return await _userRepository.GetById(id);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var users = await _userRepository.GetAll();
        return users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<User> CreateAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        await _userRepository.Insert(user);
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        user.UpdatedAt = DateTime.Now;
        await _userRepository.Update(user);
    }

    public async Task DeleteAsync(long id)
    {
        await _userRepository.Delete(id);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _userRepository.GetAll();
    }
}
