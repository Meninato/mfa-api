using MfaApi.Entities;
using Microsoft.EntityFrameworkCore;
using BCryptNet = BCrypt.Net.BCrypt;

namespace MfaApi.Database;

public class DbSeeder
{
    private readonly ModelBuilder _modelBuilder;

    public DbSeeder(ModelBuilder modelBuilder)
    {
        _modelBuilder = modelBuilder;
    }

    public void Seed()
    {
        SeedAccounts();
    }

    private void SeedAccounts()
    {
        //Seed Account table
        _modelBuilder.Entity<Account>().HasData(
            new Account
            {
                Id = 1,
                Title = "Mr",
                FirstName = "Bob",
                LastName = "Blue",
                Email = "bob@blue.com",
                AcceptTerms = true,
                PasswordHash = BCryptNet.HashPassword("12345678"),
                Verified = DateTime.Now,
                Created = DateTime.Now,
                Role = Role.Admin
            }
        );
    }
}
