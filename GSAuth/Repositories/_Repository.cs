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
        else if (entity is Need need)
        {
            var sql = @"BEGIN GS_MANAGEMENT_PKG.INSERT_NEED(
                :p_title,
                :p_description,
                :p_location,
                :p_category,
                :p_priority,
                :p_status,
                :p_quantity,
                :p_unit,
                :p_deadline_date,
                :p_creator_id,
                :p_organization_id
            ); END;";

            var parameters = new List<OracleParameter>
            {
                new OracleParameter("p_title", OracleDbType.Varchar2) { Value = need.Title ?? (object)DBNull.Value },
                new OracleParameter("p_description", OracleDbType.Clob) { Value = need.Description ?? (object)DBNull.Value },
                new OracleParameter("p_location", OracleDbType.Varchar2) { Value = need.Location ?? (object)DBNull.Value },
                new OracleParameter("p_category", OracleDbType.Varchar2) { Value = need.Category ?? (object)DBNull.Value },
                new OracleParameter("p_priority", OracleDbType.Varchar2) { Value = need.Priority ?? (object)DBNull.Value },
                new OracleParameter("p_status", OracleDbType.Varchar2) { Value = need.Status ?? "ACTIVE" },
                new OracleParameter("p_quantity", OracleDbType.Int32) { Value = need.Quantity },
                new OracleParameter("p_unit", OracleDbType.Varchar2) { Value = need.Unit ?? (object)DBNull.Value },
                new OracleParameter("p_deadline_date", OracleDbType.TimeStamp) { Value = need.DeadlineDate ?? (object)DBNull.Value },
                new OracleParameter("p_creator_id", OracleDbType.Int64) { Value = need.CreatorId },
                new OracleParameter("p_organization_id", OracleDbType.Int64) { Value = need.OrganizationId ?? (object)DBNull.Value },
            };

            await _context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());
        }
        else if (entity is Donation donation)
        {
            var sql = @"BEGIN GS_MANAGEMENT_PKG.INSERT_DONATION(
                :p_title,
                :p_description,
                :p_location,
                :p_category,
                :p_status,
                :p_quantity,
                :p_unit,
                :p_expiry_date,
                :p_donor_id
            ); END;";

            var parameters = new List<OracleParameter>
            {
                new OracleParameter("p_title", OracleDbType.Varchar2) { Value = donation.Title ?? (object)DBNull.Value },
                new OracleParameter("p_description", OracleDbType.Clob) { Value = donation.Description ?? (object)DBNull.Value },
                new OracleParameter("p_location", OracleDbType.Varchar2) { Value = donation.Location ?? (object)DBNull.Value },
                new OracleParameter("p_category", OracleDbType.Varchar2) { Value = donation.Category ?? (object)DBNull.Value },
                new OracleParameter("p_status", OracleDbType.Varchar2) { Value = donation.Status ?? "AVAILABLE" },
                new OracleParameter("p_quantity", OracleDbType.Int32) { Value = donation.Quantity },
                new OracleParameter("p_unit", OracleDbType.Varchar2) { Value = donation.Unit ?? (object)DBNull.Value },
                new OracleParameter("p_expiry_date", OracleDbType.TimeStamp) { Value = donation.ExpiryDate ?? (object)DBNull.Value },
                new OracleParameter("p_donor_id", OracleDbType.Int64) { Value = donation.DonorId },
            };

            await _context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());
        }
        else if (entity is Match match)
        {
            var sql = @"BEGIN GS_MANAGEMENT_PKG.INSERT_MATCH(
                :p_need_id,
                :p_donation_id,
                :p_status,
                :p_matched_quantity,
                :p_compatibility_score,
                :p_notes
            ); END;";

            var parameters = new List<OracleParameter>
            {
                new OracleParameter("p_need_id", OracleDbType.Int64) { Value = match.NeedId },
                new OracleParameter("p_donation_id", OracleDbType.Int64) { Value = match.DonationId },
                new OracleParameter("p_status", OracleDbType.Varchar2) { Value = match.Status ?? (object)DBNull.Value },
                new OracleParameter("p_matched_quantity", OracleDbType.Int32) { Value = match.MatchedQuantity ?? (object)DBNull.Value },
                new OracleParameter("p_compatibility_score", OracleDbType.Int32) { Value = match.CompatibilityScore ?? (object)DBNull.Value },
                new OracleParameter("p_notes", OracleDbType.Clob) { Value = match.Notes ?? (object)DBNull.Value }
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
        else if (entity is Need need)
        {
            var sql = @"BEGIN GS_MANAGEMENT_PKG.UPDATE_NEED(
                :p_id,
                :p_title,
                :p_description,
                :p_location,
                :p_category,
                :p_priority,
                :p_status,
                :p_quantity,
                :p_unit,
                :p_deadline_date
            ); END;";

            var parameters = new List<OracleParameter>
            {
                new OracleParameter("p_id", OracleDbType.Int64) { Value = need.Id },
                new OracleParameter("p_title", OracleDbType.Varchar2) { Value = need.Title ?? (object)DBNull.Value },
                new OracleParameter("p_description", OracleDbType.Clob) { Value = need.Description ?? (object)DBNull.Value },
                new OracleParameter("p_location", OracleDbType.Varchar2) { Value = need.Location ?? (object)DBNull.Value },
                new OracleParameter("p_category", OracleDbType.Varchar2) { Value = need.Category ?? (object)DBNull.Value },
                new OracleParameter("p_priority", OracleDbType.Varchar2) { Value = need.Priority ?? (object)DBNull.Value },
                new OracleParameter("p_status", OracleDbType.Varchar2) { Value = need.Status ?? (object)DBNull.Value },
                new OracleParameter("p_quantity", OracleDbType.Int32) { Value = need.Quantity },
                new OracleParameter("p_unit", OracleDbType.Varchar2) { Value = need.Unit ?? (object)DBNull.Value },
                new OracleParameter("p_deadline_date", OracleDbType.TimeStamp) { Value = need.DeadlineDate ?? (object)DBNull.Value },
            };

            await _context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());
        }

        else if (entity is Donation donation)
        {
            var sql = @"BEGIN GS_MANAGEMENT_PKG.UPDATE_DONATION(
                :p_id,
                :p_title,
                :p_description,
                :p_location,
                :p_category,
                :p_status,
                :p_quantity,
                :p_unit,
                :p_expiry_date
            ); END;";

            var parameters = new List<OracleParameter>
            {
                new OracleParameter("p_id", OracleDbType.Int64) { Value = donation.Id },
                new OracleParameter("p_title", OracleDbType.Varchar2) { Value = donation.Title ?? (object)DBNull.Value },
                new OracleParameter("p_description", OracleDbType.Clob) { Value = donation.Description ?? (object)DBNull.Value },
                new OracleParameter("p_location", OracleDbType.Varchar2) { Value = donation.Location ?? (object)DBNull.Value },
                new OracleParameter("p_category", OracleDbType.Varchar2) { Value = donation.Category ?? (object)DBNull.Value },
                new OracleParameter("p_status", OracleDbType.Varchar2) { Value = donation.Status ?? (object)DBNull.Value },
                new OracleParameter("p_quantity", OracleDbType.Int32) { Value = donation.Quantity },
                new OracleParameter("p_unit", OracleDbType.Varchar2) { Value = donation.Unit ?? (object)DBNull.Value },
                new OracleParameter("p_expiry_date", OracleDbType.TimeStamp) { Value = donation.ExpiryDate ?? (object)DBNull.Value }
            };

            await _context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());
        }
        else if (entity is Match match)
        {
            var sql = @"BEGIN GS_MANAGEMENT_PKG.UPDATE_MATCH(
                :p_id,
                :p_status,
                :p_matched_quantity,
                :p_compatibility_score,
                :p_confirmed_at,
                :p_notes
            ); END;";

            var parameters = new List<OracleParameter>
            {
                new OracleParameter("p_id", OracleDbType.Int64) { Value = match.Id },
                new OracleParameter("p_status", OracleDbType.Varchar2) { Value = match.Status ?? (object)DBNull.Value },
                new OracleParameter("p_matched_quantity", OracleDbType.Int32) { Value = match.MatchedQuantity ?? (object)DBNull.Value },
                new OracleParameter("p_compatibility_score", OracleDbType.Int32) { Value = match.CompatibilityScore ?? (object)DBNull.Value },
                new OracleParameter("p_confirmed_at", OracleDbType.TimeStamp) { Value = match.ConfirmedAt ?? (object)DBNull.Value },
                new OracleParameter("p_notes", OracleDbType.Clob) { Value = match.Notes ?? (object)DBNull.Value }
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
        else if (typeof(T) == typeof(Need))
        {
            var sql = @"BEGIN GS_MANAGEMENT_PKG.DELETE_NEED(:p_id); END;";

            var parameter = new OracleParameter("p_id", OracleDbType.Int64) { Value = id };

            await _context.Database.ExecuteSqlRawAsync(sql, parameter);
        }
        else if (typeof(T) == typeof(Donation))
        {
            var sql = @"BEGIN GS_MANAGEMENT_PKG.DELETE_DONATION(:p_id); END;";

            var parameter = new OracleParameter("p_id", OracleDbType.Int64) { Value = id };

            await _context.Database.ExecuteSqlRawAsync(sql, parameter);
        }
        else if (typeof(T) == typeof(Match))
        {
            var sql = @"BEGIN GS_MANAGEMENT_PKG.DELETE_MATCH(:p_id); END;";

            var parameter = new OracleParameter("p_id", OracleDbType.Int64) { Value = id };

            await _context.Database.ExecuteSqlRawAsync(sql, parameter);
        }
        else
        {
            throw new NotSupportedException("Entity type not supported for Delete.");
        }
    }
}