using FluentValidation;
using LifeCrm.Application.Contacts.DTOs;

namespace LifeCrm.Application.Contacts.Commands
{
    public sealed class CreateContactValidator : AbstractValidator<CreateContactRequest>
    {
        public CreateContactValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Email).EmailAddress().MaximumLength(320).When(x => !string.IsNullOrWhiteSpace(x.Email));
            RuleFor(x => x.Phone).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Phone));
            RuleFor(x => x.Tags).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Tags));
            RuleFor(x => x.Notes).MaximumLength(4000).When(x => !string.IsNullOrWhiteSpace(x.Notes));
        }
    }

    public sealed class UpdateContactValidator : AbstractValidator<UpdateContactRequest>
    {
        public UpdateContactValidator() { RuleFor(x => x.Id).NotEmpty(); Include(new CreateContactValidator()); }
    }
}
