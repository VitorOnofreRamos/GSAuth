using GSAuth.Models;
using GSAuth.Repositories;

namespace GSAuth.Services;

public class _Service : _IService
{
    private readonly _IRepository<User> _userRepository;

    public _Service(
        _IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }
}
