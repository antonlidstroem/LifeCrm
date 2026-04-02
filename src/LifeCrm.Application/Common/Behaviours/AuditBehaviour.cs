using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Application.Common.Behaviours
{
    public interface IAuditableRequest
    {
        string AuditEntityName { get; }
        Guid AuditEntityId { get; }
        string AuditAction { get; }
    }

    public interface IAuditLogWriter
    {
        Task WriteAsync(AuditLog entry, CancellationToken cancellationToken = default);
    }

    public sealed class AuditBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditLogWriter _auditLogWriter;
        private readonly ILogger<AuditBehaviour<TRequest, TResponse>> _logger;

        public AuditBehaviour(ICurrentUserService currentUser, IAuditLogWriter auditLogWriter,
            ILogger<AuditBehaviour<TRequest, TResponse>> logger)
        { _currentUser = currentUser; _auditLogWriter = auditLogWriter; _logger = logger; }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var response = await next();
            if (request is IAuditableRequest auditable)
            {
                try
                {
                    var entry = new AuditLog
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = _currentUser.OrganizationId ?? Guid.Empty,
                        EntityName = auditable.AuditEntityName,
                        EntityId = auditable.AuditEntityId,
                        Action = auditable.AuditAction,
                        ChangedBy = _currentUser.UserId?.ToString() ?? "system",
                        ChangedAt = DateTimeOffset.UtcNow
                    };
                    await _auditLogWriter.WriteAsync(entry, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Audit log write failed for {Action} on {Entity} {Id}",
                        auditable.AuditAction, auditable.AuditEntityName, auditable.AuditEntityId);
                }
            }
            return response;
        }
    }
}
