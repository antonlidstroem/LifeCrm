using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Infrastructure.Services
{
    public class OrganizationReader : IOrganizationReader
    {
        private readonly AppDbContext _db;
        public OrganizationReader(AppDbContext db) { _db = db; }

        public async Task<Organization?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _db.Organizations.FirstOrDefaultAsync(o => o.Id == id, ct);

        public async Task SaveDocumentAsync(Document document, CancellationToken ct = default)
        {
            _db.Documents.Add(document);
            await _db.SaveChangesAsync(ct);
        }
    }
}
