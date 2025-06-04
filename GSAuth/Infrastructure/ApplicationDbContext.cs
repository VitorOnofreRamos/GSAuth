using Microsoft.EntityFrameworkCore;
using GSAuth.Models;

namespace GSAuth.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>("SEQ_USERS")
            .StartsAt(1)
            .IncrementsBy(1);

        modelBuilder.Entity<User>()
            .Property(c => c.Id)
            .HasDefaultValueSql("SEQ_USERS.NEXTVAL");
    }
}
