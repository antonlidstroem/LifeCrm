using FluentValidation;
using LifeCrm.Application.Interactions.DTOs;

namespace LifeCrm.Application.Interactions.Commands
{
    public sealed class CreateInteractionValidator : AbstractValidator<CreateInteractionRequest>
    {
        public CreateInteractionValidator()
        {
            RuleFor(x => x.Body).NotEmpty().MaximumLength(8000);
            RuleFor(x => x).Must(x => x.ContactId.HasValue || x.ProjectId.HasValue)
                .WithMessage("An interaction must be linked to a contact or a project.").WithName("ContactId");
            RuleFor(x => x.Subject).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Subject));
        }
    }

    public sealed class UpdateInteractionValidator : AbstractValidator<UpdateInteractionRequest>
    {
        public UpdateInteractionValidator() { RuleFor(x => x.Id).NotEmpty(); Include(new CreateInteractionValidator()); }
    }
}
