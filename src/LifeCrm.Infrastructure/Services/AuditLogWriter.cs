using LifeCrm.Application.Common.Behaviours;
using LifeCrm.Core.Entities;
using LifeCrm.Infrastructure.Persistence;

namespace LifeCrm.Infrastructure.Services
{
    public class AuditLogWriter : IAuditLogWriter
    {
        private readonly AppDbContext _db;
        public AuditLogWriter(AppDbContext db) { _db = db; }

        public async Task WriteAsync(AuditLog entry, CancellationToken ct = default)
        {
            _db.AuditLogs.Add(entry);
            await _db.SaveChangesAsync(ct);
        }
    }
}
