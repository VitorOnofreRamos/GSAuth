using GSAuth.Models;
using GSAuth.Repositories;

namespace GSAuth.Infrastructure;

public interface IUnitOfWork : IDisposable
{
    _IRepository<_BaseEntity> _IRepository { get; }

    Task<int> CompleteAsync();
}
