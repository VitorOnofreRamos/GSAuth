using GSAuth.Infrastructure;
using GSAuth.Models;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace GSAuth.Repositories;

public class _Repository<T> : _IRepository<T> where T : _BaseEntity
{
    protected readonly ApplicationDbContext _context;
    private readonly DbSet<T> _entities;

    public _Repository(ApplicationDbContext context)
    {
        _context = context;
        _entities = context.Set<T>();
    }

    public async Task<T> GetById(long id)
        => await _entities.FindAsync(id) ?? throw new KeyNotFoundException($"Entity with id {id} not found.");

    public async Task<IEnumerable<T>> GetAll()
        => await _entities.ToListAsync();

    public async Task Insert(T entity)
    {
        if (entity is User user)
        {
            var sql = @"BEGIN GS_MANAGEMENT_PKG.INSERT_USER(
                :p_email,
                :p_phone,
                :p_name,
                :p_password_hash,
                :p_role,
                :p_is_active,
                :p_organization_id
            ); END;";
            await _context.Database.ExecuteSqlRawAsync(sql,
                new OracleParameter("p_email", user.Email),
                new OracleParameter("p_phone", user.Phone),
                new OracleParameter("p_name", user.Name),
                new OracleParameter("p_password_hash", user.PasswordHash),
                new OracleParameter("p_role", user.Role),
                new OracleParameter("p_is_active", user.IsActive),
                new OracleParameter("organization_id", user.OrganizationId));
        }
        else
        {
            throw new NotSupportedException("Entity type not supported for Insert.");
        }
    }

    public async Task Update(T entity)
    {
        if (entity is User user)
        {
            var sql = @"BEGIN GS_MANAGEMENT_PKG.UPDATE_USER(
                :p_id,
                :p_email,
                :p_phone,
                :p_name,
                :p_password_hash,
                :p_role,
                :p_is_active,
                :p_organization_id
            ); END;";
            await _context.Database.ExecuteSqlRawAsync(sql,
                new OracleParameter("p_id", user.Id),
                new OracleParameter("p_email", (object)user.Email ?? DBNull.Value),
                new OracleParameter("p_phone", (object)user.Phone ?? DBNull.Value),
                new OracleParameter("p_name", (object)user.Name ?? DBNull.Value),
                new OracleParameter("p_password_hash", (object)user.PasswordHash ?? DBNull.Value),
                new OracleParameter("p_role", (object)user.Role ?? DBNull.Value),
                new OracleParameter("p_is_active", (object)user.IsActive ?? DBNull.Value),
                new OracleParameter("p_organization_id", (object)user.OrganizationId ?? DBNull.Value));
        }
        else
        {
            throw new NotSupportedException("Entity type not supported for Update.");
        }
    }

    public async Task Delete(long id)
    {
        if (typeof(T) == typeof(User))
        {
            var sql = @"BEGIN GS_MANAGEMENT_PKG.DELETE_USER(:p_id); END;";
            await _context.Database.ExecuteSqlRawAsync(sql,
                new OracleParameter("p_id_Paciente", id));
        }
        else
        {
            throw new NotSupportedException("Entity type not supported for Delete.");
        }
    }
}
