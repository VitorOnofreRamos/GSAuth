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

    // Constructor that accepts all repositories (for services that need them all)
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

    // Constructor that accepts only user repository (for simpler services)
    protected _Service(_IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    // Parameterless constructor for dependency injection scenarios
    protected _Service(){}
}
