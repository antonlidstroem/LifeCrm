using LifeCrm.Application.Common.Behaviours;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Contacts.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Contacts.Commands
{
    public sealed record ContactCreatedNotification(Guid ContactId, string ContactName) : INotification;

    public sealed class CreateContactCommand : IRequest<Guid>, IAuditableRequest
    {
        private readonly Guid _entityId = Guid.NewGuid();
        public CreateContactRequest Request { get; }
        public string AuditEntityName => "Contact";
        public Guid   AuditEntityId   => _entityId;
        public string AuditAction     => "Created";
        public CreateContactCommand(CreateContactRequest request) { Request = request; }
    }

    public sealed class CreateContactHandler : IRequestHandler<CreateContactCommand, Guid>
    {
        private readonly IUnitOfWork         _uow;
        private readonly ICurrentUserService _currentUser;
        private readonly IMediator           _mediator;

        public CreateContactHandler(IUnitOfWork uow, ICurrentUserService currentUser, IMediator mediator)
        { _uow = uow; _currentUser = currentUser; _mediator = mediator; }

        public async Task<Guid> Handle(CreateContactCommand command, CancellationToken cancellationToken)
        {
            var req   = command.Request;
            var orgId = _currentUser.OrganizationId ?? throw new ForbiddenException("No organization context.");
            var contact = new Contact
            {
                Id                 = command.AuditEntityId,
                OrganizationId     = orgId,
                Name               = req.Name.Trim(),
                Type               = req.Type,
                Email              = req.Email?.Trim().ToLowerInvariant(),
                Phone              = req.Phone?.Trim(),
                AddressLine1       = req.AddressLine1?.Trim(),
                AddressLine2       = req.AddressLine2?.Trim(),
                City               = req.City?.Trim(),
                StateProvince      = req.StateProvince?.Trim(),
                PostalCode         = req.PostalCode?.Trim(),
                Country            = req.Country?.Trim(),
                Tags               = req.Tags?.Trim(),
                Notes              = req.Notes?.Trim(),
                PrimaryContactName = req.PrimaryContactName?.Trim(),
                EmailOptOut        = req.EmailOptOut
            };
            await _uow.Contacts.AddAsync(contact, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            await _mediator.Publish(new ContactCreatedNotification(contact.Id, contact.Name), cancellationToken);
            return contact.Id;
        }
    }

    public sealed class UpdateContactCommand : IRequest<Unit>, IAuditableRequest
    {
        public UpdateContactRequest Request { get; }
        public string AuditEntityName => "Contact";
        public Guid   AuditEntityId   => Request.Id;
        public string AuditAction     => "Updated";
        public UpdateContactCommand(UpdateContactRequest request) { Request = request; }
    }

    public sealed class UpdateContactHandler : IRequestHandler<UpdateContactCommand, Unit>
    {
        private readonly IUnitOfWork _uow;
        public UpdateContactHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<Unit> Handle(UpdateContactCommand command, CancellationToken cancellationToken)
        {
            var req     = command.Request;
            var contact = await _uow.Contacts.GetByIdAsync(req.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(Contact), req.Id);
            contact.Name               = req.Name.Trim();
            contact.Type               = req.Type;
            contact.Email              = req.Email?.Trim().ToLowerInvariant();
            contact.Phone              = req.Phone?.Trim();
            contact.AddressLine1       = req.AddressLine1?.Trim();
            contact.AddressLine2       = req.AddressLine2?.Trim();
            contact.City               = req.City?.Trim();
            contact.StateProvince      = req.StateProvince?.Trim();
            contact.PostalCode         = req.PostalCode?.Trim();
            contact.Country            = req.Country?.Trim();
            contact.Tags               = req.Tags?.Trim();
            contact.Notes              = req.Notes?.Trim();
            contact.PrimaryContactName = req.PrimaryContactName?.Trim();
            contact.EmailOptOut        = req.EmailOptOut;
            _uow.Contacts.Update(contact);
            await _uow.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }

    public sealed class DeleteContactCommand : IRequest<Unit>, IAuditableRequest
    {
        public Guid   ContactId       { get; }
        public string AuditEntityName => "Contact";
        public Guid   AuditEntityId   => ContactId;
        public string AuditAction     => "Deleted";
        public DeleteContactCommand(Guid contactId) { ContactId = contactId; }
    }

    public sealed class DeleteContactHandler : IRequestHandler<DeleteContactCommand, Unit>
    {
        private readonly IUnitOfWork _uow;
        public DeleteContactHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<Unit> Handle(DeleteContactCommand command, CancellationToken cancellationToken)
        {
            var contact = await _uow.Contacts.GetByIdAsync(command.ContactId, cancellationToken)
                ?? throw new NotFoundException(nameof(Contact), command.ContactId);
            _uow.Contacts.Delete(contact);
            await _uow.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }

    public sealed class ImportContactsCommand : IRequest<ImportContactsResult>
    {
        public byte[] CsvBytes { get; }
        public ImportContactsCommand(byte[] csvBytes) { CsvBytes = csvBytes; }
    }

    public record ImportContactsResult
    {
        public int SuccessCount { get; init; }
        public int ErrorCount   { get; init; }
        public IReadOnlyList<string> ValidationErrors { get; init; } = Array.Empty<string>();
    }

    public record ImportRowError(int RowNumber, string Reason, string RawRow);

    public sealed class ImportContactsHandler : IRequestHandler<ImportContactsCommand, ImportContactsResult>
    {
        private readonly ICsvService         _csv;
        private readonly IUnitOfWork         _uow;
        private readonly ICurrentUserService _currentUser;

        public ImportContactsHandler(ICsvService csv, IUnitOfWork uow, ICurrentUserService currentUser)
        { _csv = csv; _uow = uow; _currentUser = currentUser; }

        public async Task<ImportContactsResult> Handle(
            ImportContactsCommand command, CancellationToken cancellationToken)
        {
            var orgId = _currentUser.OrganizationId
                ?? throw new ForbiddenException("No organization context.");

            var (rows, parseErrors) = await _csv.ImportAsync<ContactCsvRow>(command.CsvBytes);

            int successCount = 0;
            var importErrors = parseErrors
                .Select(e => new ImportRowError(e.RowNumber, e.Reason, e.RawRow))
                .ToList();

            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.Name))
                {
                    importErrors.Add(new ImportRowError(0, "Name is required.", string.Empty));
                    continue;
                }

                // FIXED: Parse the Type string from CSV; fall back to Individual if absent/invalid.
                var contactType = ContactType.Individual;
                if (!string.IsNullOrWhiteSpace(row.Type) &&
                    !Enum.TryParse<ContactType>(row.Type.Trim(), ignoreCase: true, out contactType))
                {
                    contactType = ContactType.Individual; // graceful fallback
                }

                var contact = new Contact
                {
                    Id             = Guid.NewGuid(),
                    OrganizationId = orgId,
                    Name           = row.Name.Trim(),
                    Type           = contactType,             // FIXED
                    Email          = row.Email?.Trim().ToLowerInvariant(),
                    Phone          = row.Phone?.Trim(),
                    Tags           = row.Tags?.Trim(),
                    AddressLine1   = row.AddressLine1?.Trim(),
                    City           = row.City?.Trim(),
                    StateProvince  = row.StateProvince?.Trim(),
                    PostalCode     = row.PostalCode?.Trim(),
                    Country        = row.Country?.Trim(),
                    Notes          = row.Notes?.Trim()
                };
                await _uow.Contacts.AddAsync(contact, cancellationToken);
                successCount++;
            }

            await _uow.SaveChangesAsync(cancellationToken);

            return new ImportContactsResult
            {
                SuccessCount      = successCount,
                ErrorCount        = importErrors.Count,
                ValidationErrors  = importErrors.Select(e => e.Reason).ToList()
            };
        }
    }
}
