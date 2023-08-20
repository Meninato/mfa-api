using MfaApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MfaApi.Database;

public class DataContext : DbContext
{
    private readonly IConfiguration Configuration;

    public DbSet<Account> Accounts { get; set; }

    public DataContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // connect to sqlite database
        options.UseSqlite(Configuration.GetConnectionString("ApiDatabase"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Owned<RefreshToken>();

        modelBuilder.Entity<Account>(e =>
        {
            e.OwnsMany(acc => acc.RefreshTokens).WithOwner(rt => rt.Account);

            e.HasKey(e => e.Id);
            e.HasIndex(e => e.Email).IsUnique();

            e.Property(e => e.FirstName).IsRequired();
            e.Property(e => e.LastName).IsRequired();
            e.Property(e => e.Role).IsRequired();
            e.Property(e => e.Email).IsRequired();
            e.Property(e => e.PasswordHash).IsRequired();
        });

        new DbSeeder(modelBuilder).Seed();
    }
}
