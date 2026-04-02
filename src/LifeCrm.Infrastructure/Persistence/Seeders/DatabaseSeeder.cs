using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Infrastructure.Persistence.Seeders
{
    public class DatabaseSeeder
    {
        private readonly AppDbContext _db;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(AppDbContext db, ILogger<DatabaseSeeder> logger)
        { _db = db; _logger = logger; }

        public async Task SeedAsync()
        {
            try
            {
                await _db.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Migration failed — database may already be up to date.");
            }

            if (await _db.Organizations.AnyAsync()) return;

            var org = new Organization
            {
                Id     = Guid.NewGuid(),
                Name   = "Demo Organisation",
                Slug   = "demo",
                IsActive = true
            };
            _db.Organizations.Add(org);

            var admin = new ApplicationUser
            {
                Id             = Guid.NewGuid(),
                OrganizationId = org.Id,
                FullName       = "Admin User",
                Email          = "admin@lifecrm.dev",
                PasswordHash   = BCrypt.Net.BCrypt.HashPassword("Admin123!@#"),
                Role           = UserRole.Admin,
                IsActive       = true,
                CreatedBy      = "seed"
            };
            _db.Users.Add(admin);

            await _db.SaveChangesAsync();
            _logger.LogInformation("Database seeded. Login: admin@lifecrm.dev / Admin123!@#");
        }
    }
}
