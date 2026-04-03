using LifeCrm.Application.Common.Behaviours;
using LifeCrm.Core.Entities;
using LifeCrm.Infrastructure.Persistence;

namespace LifeCrm.Infrastructure.Services
{
    /// <summary>
    /// Writes audit log entries to the database.
    ///
    /// DESIGN NOTE: AuditBehaviour runs AFTER the handler's SaveChangesAsync() has already
    /// committed the entity. This writer therefore calls its own SaveChangesAsync() to persist
    /// the audit entry. Because AppDbContext is registered as Scoped (one instance per HTTP
    /// request), this writer shares the exact same DbContext instance as UnitOfWork. The second
    /// SaveChangesAsync() is an additional, intentional round-trip — audit logs are best-effort
    /// and should not roll back the primary operation if they fail. Failures are caught and
    /// logged by AuditBehaviour.
    /// </summary>
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
