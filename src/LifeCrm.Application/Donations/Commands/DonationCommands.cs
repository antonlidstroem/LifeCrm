using LifeCrm.Application.Common.Behaviours;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Donations.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Donations.Commands
{
    public record DonationCreatedNotification(Guid DonationId, string? ContactEmail, bool EmailOptOut) : INotification;

    public class CreateDonationCommand : IRequest<Guid>, IAuditableRequest
    {
        private readonly Guid _entityId = Guid.NewGuid();
        public CreateDonationRequest Request { get; }
        public string AuditEntityName => "Donation";
        public Guid   AuditEntityId   => _entityId;
        public string AuditAction     => "Created";
        public CreateDonationCommand(CreateDonationRequest r) { Request = r; }
    }

    public sealed class CreateDonationHandler : IRequestHandler<CreateDonationCommand, Guid>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _cu;
        private readonly IMediator _mediator;
        public CreateDonationHandler(IUnitOfWork uow, ICurrentUserService cu, IMediator m) { _uow = uow; _cu = cu; _mediator = m; }

        public async Task<Guid> Handle(CreateDonationCommand cmd, CancellationToken ct)
        {
            var orgId = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");
            var r = cmd.Request;
            var contact = await _uow.Contacts.GetByIdAsync(r.ContactId, ct)
                ?? throw new NotFoundException(nameof(Contact), r.ContactId);
            if (r.CampaignId.HasValue) _ = await _uow.Campaigns.GetByIdAsync(r.CampaignId.Value, ct)
                ?? throw new NotFoundException(nameof(Campaign), r.CampaignId.Value);
            if (r.ProjectId.HasValue)  _ = await _uow.Projects.GetByIdAsync(r.ProjectId.Value, ct)
                ?? throw new NotFoundException(nameof(Project), r.ProjectId.Value);
            var donation = new Donation
            {
                Id = cmd.AuditEntityId, OrganizationId = orgId, ContactId = r.ContactId,
                Amount = r.Amount, Date = r.Date, Status = r.Status,
                CampaignId = r.CampaignId, ProjectId = r.ProjectId,
                PaymentMethod = r.PaymentMethod?.Trim(), ReferenceNumber = r.ReferenceNumber?.Trim(), Notes = r.Notes?.Trim()
            };
            await _uow.Donations.AddAsync(donation, ct);
            await _uow.SaveChangesAsync(ct);
            await _mediator.Publish(new DonationCreatedNotification(donation.Id, contact.Email, contact.EmailOptOut), ct);
            return donation.Id;
        }
    }

    public class UpdateDonationCommand : IRequest<Unit>, IAuditableRequest
    {
        public UpdateDonationRequest Request { get; }
        public string AuditEntityName => "Donation";
        public Guid   AuditEntityId   => Request.Id;
        public string AuditAction     => "Updated";
        public UpdateDonationCommand(UpdateDonationRequest r) { Request = r; }
    }

    public sealed class UpdateDonationHandler : IRequestHandler<UpdateDonationCommand, Unit>
    {
        private readonly IUnitOfWork _uow;
        public UpdateDonationHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<Unit> Handle(UpdateDonationCommand cmd, CancellationToken ct)
        {
            var r = cmd.Request;
            var d = await _uow.Donations.GetByIdAsync(r.Id, ct)
                ?? throw new NotFoundException(nameof(Donation), r.Id);
            d.Amount = r.Amount; d.Date = r.Date; d.Status = r.Status;
            d.CampaignId = r.CampaignId; d.ProjectId = r.ProjectId;
            d.PaymentMethod = r.PaymentMethod?.Trim(); d.ReferenceNumber = r.ReferenceNumber?.Trim(); d.Notes = r.Notes?.Trim();
            _uow.Donations.Update(d);
            await _uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }

    public class DeleteDonationCommand : IRequest<Unit>, IAuditableRequest
    {
        public Guid DonationId { get; }
        public string AuditEntityName => "Donation";
        public Guid   AuditEntityId   => DonationId;
        public string AuditAction     => "Deleted";
        public DeleteDonationCommand(Guid id) { DonationId = id; }
    }

    public sealed class DeleteDonationHandler : IRequestHandler<DeleteDonationCommand, Unit>
    {
        private readonly IUnitOfWork _uow;
        public DeleteDonationHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<Unit> Handle(DeleteDonationCommand cmd, CancellationToken ct)
        {
            var d = await _uow.Donations.GetByIdAsync(cmd.DonationId, ct)
                ?? throw new NotFoundException(nameof(Donation), cmd.DonationId);
            _uow.Donations.Delete(d);
            await _uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
