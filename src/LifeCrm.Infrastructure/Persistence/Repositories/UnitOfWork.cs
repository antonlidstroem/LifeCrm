using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;

namespace LifeCrm.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _db;
        private bool _disposed;

        public IRepository<Contact>     Contacts     { get; }
        public IRepository<Donation>    Donations    { get; }
        public ICampaignRepository      Campaigns    { get; }
        public IRepository<Project>     Projects     { get; }
        public IRepository<Interaction> Interactions { get; }
        public IInteractionRepository   InteractionRepo { get; }

        public UnitOfWork(AppDbContext db)
        {
            _db             = db;
            Contacts        = new GenericRepository<Contact>(db);
            Donations       = new GenericRepository<Donation>(db);
            Campaigns       = new CampaignRepository(db);
            Projects        = new GenericRepository<Project>(db);
            var ir          = new InteractionRepository(db);
            Interactions    = ir;
            InteractionRepo = ir;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);

        public void Dispose()
        {
            if (!_disposed) { _db.Dispose(); _disposed = true; }
        }
    }
}
