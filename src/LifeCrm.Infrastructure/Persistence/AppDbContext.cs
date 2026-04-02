using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        private readonly ICurrentUserService? _currentUser;

        public AppDbContext(DbContextOptions<AppDbContext> options,
            ICurrentUserService? currentUser = null) : base(options)
        {
            _currentUser = currentUser;
        }

        public DbSet<Organization>      Organizations { get; set; } = null!;
        public DbSet<ApplicationUser>   Users         { get; set; } = null!;
        public DbSet<Contact>           Contacts      { get; set; } = null!;
        public DbSet<Donation>          Donations     { get; set; } = null!;
        public DbSet<Campaign>          Campaigns     { get; set; } = null!;
        public DbSet<Project>           Projects      { get; set; } = null!;
        public DbSet<Interaction>       Interactions  { get; set; } = null!;
        public DbSet<Document>          Documents     { get; set; } = null!;
        public DbSet<AuditLog>          AuditLogs     { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // Global soft-delete + tenant filters on TenantEntity types
            var orgId = _currentUser?.OrganizationId ?? Guid.Empty;

            mb.Entity<Contact>()
                .HasQueryFilter(e => !e.IsDeleted && e.OrganizationId == orgId);
            mb.Entity<Donation>()
                .HasQueryFilter(e => !e.IsDeleted && e.OrganizationId == orgId);
            mb.Entity<Campaign>()
                .HasQueryFilter(e => !e.IsDeleted && e.OrganizationId == orgId);
            mb.Entity<Project>()
                .HasQueryFilter(e => !e.IsDeleted && e.OrganizationId == orgId);
            mb.Entity<Interaction>()
                .HasQueryFilter(e => !e.IsDeleted && e.OrganizationId == orgId);
            mb.Entity<Document>()
                .HasQueryFilter(e => !e.IsDeleted && e.OrganizationId == orgId);
            mb.Entity<ApplicationUser>()
                .HasQueryFilter(e => !e.IsDeleted && e.OrganizationId == orgId);

            // Decimal precision
            mb.Entity<Donation>().Property(d => d.Amount).HasPrecision(18, 2);
            mb.Entity<Campaign>().Property(c => c.BudgetGoal).HasPrecision(18, 2);
            mb.Entity<Project>().Property(p => p.BudgetGoal).HasPrecision(18, 2);

            // Document binary
            mb.Entity<Document>().Property(d => d.PdfBytes).HasColumnType("varbinary(max)");

            // Indexes
            mb.Entity<ApplicationUser>().HasIndex(u => u.Email).IsUnique();
            mb.Entity<Contact>().HasIndex(c => new { c.OrganizationId, c.Email });
            mb.Entity<AuditLog>().HasIndex(a => new { a.OrganizationId, a.ChangedAt });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var now    = DateTimeOffset.UtcNow;
            var userId = _currentUser?.UserId?.ToString() ?? "system";

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                }
                if (entry.State is EntityState.Added or EntityState.Modified)
                {
                    entry.Entity.LastModifiedAt = now;
                    entry.Entity.LastModifiedBy = userId;
                }
            }
            return await base.SaveChangesAsync(ct);
        }
    }
}
