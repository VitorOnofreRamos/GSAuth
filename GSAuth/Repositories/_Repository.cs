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
            // Ajustar os parâmetros para corresponder exatamente à procedure INSERT_USER
            var sql = @"BEGIN GS_MANAGEMENT_PKG.INSERT_USER(
                :p_email,
                :p_phone,
                :p_name,
                :p_password_hash,
                :p_role,
                :p_is_active,
                :p_organization_id
            ); END;";

            var parameters = new List<OracleParameter>
            {
                new OracleParameter("p_email", OracleDbType.Varchar2) { Value = user.Email ?? (object)DBNull.Value },
                new OracleParameter("p_phone", OracleDbType.Varchar2) { Value = user.Phone ?? (object)DBNull.Value },
                new OracleParameter("p_name", OracleDbType.Varchar2) { Value = user.Name ?? (object)DBNull.Value },
                new OracleParameter("p_password_hash", OracleDbType.Varchar2) { Value = user.PasswordHash ?? (object)DBNull.Value },
                new OracleParameter("p_role", OracleDbType.Varchar2) { Value = user.Role ?? (object)DBNull.Value },
                new OracleParameter("p_is_active", OracleDbType.Char) { Value = user.IsActive ?? "Y" },
                new OracleParameter("p_organization_id", OracleDbType.Int64) { Value = user.OrganizationId ?? (object)DBNull.Value }
            };

            await _context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());
        }
        else if (entity is Organization organization)
        {
            var sql = @"BEGIN GS_MANAGEMENT_PKG.INSERT_ORGANIZATION(
                :p_name,
                :p_description,
                :p_location,
                :p_contact_email,
                :p_contact_phone,
                :p_type
            ); END;";

            var parameters = new List<OracleParameter>
            {
                new OracleParameter("p_name", OracleDbType.Varchar2) { Value = organization.Name ?? (object)DBNull.Value },
                new OracleParameter("p_description", OracleDbType.Clob) { Value = organization.Description ?? (object)DBNull.Value },
                new OracleParameter("p_location", OracleDbType.Varchar2) { Value = organization.Location ?? (object)DBNull.Value },
                new OracleParameter("p_contact_email", OracleDbType.Varchar2) { Value = organization.ContactEmail ?? (object)DBNull.Value },
                new OracleParameter("p_contact_phone", OracleDbType.Varchar2) { Value = organization.ContactPhone ?? (object)DBNull.Value },
                new OracleParameter("p_type", OracleDbType.Varchar2) { Value = organization.Type ?? (object)DBNull.Value },
            };

            await _context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());
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
            // Ajustar os parâmetros para corresponder exatamente à procedure UPDATE_USER
            var sql = @"BEGIN GS_MANAGEMENT_PKG.UPDATE_USER(
                :p_id,
                :p_email,
                :p_phone,
                :p_name,
                :p_password_hash,
                :p_role,
                :p_is_active,
                :p_last_login,
                :p_organization_id
            ); END;";

            var parameters = new List<OracleParameter>
            {
                new OracleParameter("p_id", OracleDbType.Int64) { Value = user.Id },
                new OracleParameter("p_email", OracleDbType.Varchar2) { Value = user.Email ?? (object)DBNull.Value },
                new OracleParameter("p_phone", OracleDbType.Varchar2) { Value = user.Phone ?? (object)DBNull.Value },
                new OracleParameter("p_name", OracleDbType.Varchar2) { Value = user.Name ?? (object)DBNull.Value },
                new OracleParameter("p_password_hash", OracleDbType.Varchar2) { Value = user.PasswordHash ?? (object)DBNull.Value },
                new OracleParameter("p_role", OracleDbType.Varchar2) { Value = user.Role ?? (object)DBNull.Value },
                new OracleParameter("p_is_active", OracleDbType.Char) { Value = user.IsActive ?? (object)DBNull.Value },
                new OracleParameter("p_last_login", OracleDbType.TimeStamp) { Value = user.LastLogin ?? (object)DBNull.Value },
                new OracleParameter("p_organization_id", OracleDbType.Int64) { Value = user.OrganizationId ?? (object)DBNull.Value }
            };

            await _context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());
        }
        else if (entity is Organization organization)
        {
            var sql = @"BEGIN GS_MANAGEMENT_PKG.UPDATE_ORGANIZATION(
                :p_id,
                :p_name,
                :p_description,
                :p_location,
                :p_contact_email,
                :p_contact_phone,
                :p_type
            ); END;";

            var parameters = new List<OracleParameter>
            {
                new OracleParameter("p_id", OracleDbType.Int64) { Value = organization.Id },
                new OracleParameter("p_name", OracleDbType.Varchar2) { Value = organization.Name ?? (object)DBNull.Value },
                new OracleParameter("p_description", OracleDbType.Clob) { Value = organization.Description ?? (object)DBNull.Value },
                new OracleParameter("p_location", OracleDbType.Varchar2) { Value = organization.Location ?? (object)DBNull.Value },
                new OracleParameter("p_contact_email", OracleDbType.Varchar2) { Value = organization.ContactEmail ?? (object)DBNull.Value },
                new OracleParameter("p_contact_phone", OracleDbType.Varchar2) { Value = organization.ContactPhone ?? (object)DBNull.Value },
                new OracleParameter("p_type", OracleDbType.Varchar2) { Value = organization.Type ?? (object)DBNull.Value },
            };

            await _context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());
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

            var parameter = new OracleParameter("p_id", OracleDbType.Int64) { Value = id };

            await _context.Database.ExecuteSqlRawAsync(sql, parameter);
        }
        else if (typeof(T) == typeof(Organization))
        {
            var sql = "@BEGIN GS_MANAGEMENT_PKG.DELETE_ORGANIZATION; END;";

            var parameter = new OracleParameter("p_id", OracleDbType.Int64) { Value = id };

            await _context.Database.ExecuteSqlRawAsync(sql, parameter);
        }
        else
        {
            throw new NotSupportedException("Entity type not supported for Delete.");
        }
    }
}