using FluentValidation;
using LifeCrm.Application.Donations.DTOs;

namespace LifeCrm.Application.Donations.Commands
{
    public sealed class CreateDonationValidator : AbstractValidator<CreateDonationRequest>
    {
        public CreateDonationValidator()
        {
            RuleFor(x => x.ContactId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThan(0).LessThanOrEqualTo(10_000_000);
            RuleFor(x => x.Date).NotEmpty().LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today));
            RuleFor(x => x.PaymentMethod).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.PaymentMethod));
        }
    }

    public sealed class UpdateDonationValidator : AbstractValidator<UpdateDonationRequest>
    {
        public UpdateDonationValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.ContactId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThan(0).LessThanOrEqualTo(10_000_000);
            RuleFor(x => x.Date).NotEmpty();
        }
    }
}
