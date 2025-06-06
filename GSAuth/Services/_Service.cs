using GSAuth.Models;
using GSAuth.Repositories;

namespace GSAuth.Services;

public class _Service : _IService
{
    private readonly _IRepository<Organization> _organizationRepository;
    private readonly _IRepository<User> _userRepository;
    private readonly _IRepository<Need> _needRepository;
    private readonly _IRepository<Donation> _donationRepository;
    private readonly _IRepository<Match> _matchRepository;

    public _Service(
        _IRepository<User> userRepository, 
        _IRepository<Organization> organizationRepository, 
        _IRepository<Need> needRepository,
        _IRepository<Donation> donationRepository, 
        _IRepository<Match> matchRepository)
    {
        _userRepository = userRepository;
        _organizationRepository = organizationRepository;
        _needRepository = needRepository;
        _donationRepository = donationRepository;
        _matchRepository = matchRepository;
    }
}
