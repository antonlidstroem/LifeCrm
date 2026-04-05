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

        public DbSet<Organization>           Organizations           { get; set; } = null!;
        public DbSet<ApplicationUser>        Users                   { get; set; } = null!;
        public DbSet<Contact>                Contacts                { get; set; } = null!;
        public DbSet<Donation>               Donations               { get; set; } = null!;
        public DbSet<Campaign>               Campaigns               { get; set; } = null!;
        public DbSet<Project>                Projects                { get; set; } = null!;
        public DbSet<Interaction>            Interactions            { get; set; } = null!;
        public DbSet<Document>               Documents               { get; set; } = null!;
        public DbSet<AuditLog>               AuditLogs               { get; set; } = null!;
        public DbSet<Newsletter>             Newsletters             { get; set; } = null!;
        public DbSet<NewsletterAttachment>   NewsletterAttachments   { get; set; } = null!;  // Phase 2

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // Tenant + soft-delete filter on every TenantEntity
            static void Filter<T>(ModelBuilder b, ICurrentUserService? cu)
                where T : TenantEntity
                => b.Entity<T>().HasQueryFilter(e =>
                    !e.IsDeleted &&
                    (cu == null || !cu.IsAuthenticated || e.OrganizationId == cu.OrganizationId));

            Filter<Contact>(mb, _currentUser);
            Filter<Donation>(mb, _currentUser);
            Filter<Campaign>(mb, _currentUser);
            Filter<Project>(mb, _currentUser);
            Filter<Interaction>(mb, _currentUser);
            Filter<Document>(mb, _currentUser);
            Filter<ApplicationUser>(mb, _currentUser);
            Filter<Newsletter>(mb, _currentUser);
            Filter<NewsletterAttachment>(mb, _currentUser);   // Phase 2

            // Decimal precision
            mb.Entity<Donation>().Property(d => d.Amount).HasPrecision(18, 2);
            mb.Entity<Campaign>().Property(c => c.BudgetGoal).HasPrecision(18, 2);
            mb.Entity<Project>().Property(p => p.BudgetGoal).HasPrecision(18, 2);

            // Large text / binary columns
            mb.Entity<Newsletter>().Property(n => n.HtmlBody).HasColumnType("nvarchar(max)");
            mb.Entity<Document>().Property(d => d.PdfBytes).HasColumnType("varbinary(max)");
            mb.Entity<NewsletterAttachment>().Property(a => a.FileBytes).HasColumnType("varbinary(max)");

            // FK: NewsletterAttachment → Newsletter
            mb.Entity<NewsletterAttachment>()
                .HasOne(a => a.Newsletter)
                .WithMany(n => n.Attachments)
                .HasForeignKey(a => a.NewsletterId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            mb.Entity<ApplicationUser>().HasIndex(u => u.Email).IsUnique();
            mb.Entity<Contact>().HasIndex(c => new { c.OrganizationId, c.Email });
            mb.Entity<AuditLog>().HasIndex(a => new { a.OrganizationId, a.ChangedAt });
            mb.Entity<Newsletter>().HasIndex(n => new { n.OrganizationId, n.Status, n.CreatedAt });
            mb.Entity<NewsletterAttachment>().HasIndex(a => a.NewsletterId);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var now    = DateTimeOffset.UtcNow;
            var userId = _currentUser?.UserId?.ToString() ?? "system";

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                    entry.Entity.CreatedAt = now;

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
