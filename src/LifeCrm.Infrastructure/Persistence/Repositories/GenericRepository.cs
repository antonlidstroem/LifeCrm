using System.Linq.Expressions;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly AppDbContext _db;
        protected readonly DbSet<T> _set;

        public GenericRepository(AppDbContext db) { _db = db; _set = db.Set<T>(); }

        public IQueryable<T> Query() => _set.AsQueryable();

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _set.FirstOrDefaultAsync(e => e.Id == id, ct);

        public async Task AddAsync(T entity, CancellationToken ct = default)
            => await _set.AddAsync(entity, ct);

        public void Update(T entity)
            => _db.Entry(entity).State = EntityState.Modified;

        public void Delete(T entity)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTimeOffset.UtcNow;
            _db.Entry(entity).State = EntityState.Modified;
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
            => await _set.CountAsync(predicate, ct);
    }

    public class CampaignRepository : GenericRepository<Campaign>, ICampaignRepository
    {
        public CampaignRepository(AppDbContext db) : base(db) { }

        public async Task<IReadOnlyList<Campaign>> GetActiveAsync(CancellationToken ct = default)
            => await _set.Where(c => c.Status == Core.Enums.CampaignStatus.Active)
                         .OrderBy(c => c.Name)
                         .ToListAsync(ct);
    }

    public class InteractionRepository : GenericRepository<Interaction>, IInteractionRepository
    {
        public InteractionRepository(AppDbContext db) : base(db) { }

        public async Task<IReadOnlyList<Interaction>> GetByContactAsync(Guid contactId, CancellationToken ct = default)
            => await _set.Include(i => i.Contact).Include(i => i.Project)
                         .Where(i => i.ContactId == contactId)
                         .OrderByDescending(i => i.OccurredAt)
                         .ToListAsync(ct);

        public async Task<IReadOnlyList<Interaction>> GetByProjectAsync(Guid projectId, CancellationToken ct = default)
            => await _set.Include(i => i.Contact).Include(i => i.Project)
                         .Where(i => i.ProjectId == projectId)
                         .OrderByDescending(i => i.OccurredAt)
                         .ToListAsync(ct);
    }
}
