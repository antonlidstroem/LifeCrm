using FluentValidation;
using LifeCrm.Application.Projects.DTOs;

namespace LifeCrm.Application.Projects.Commands
{
    public sealed class CreateProjectValidator : AbstractValidator<CreateProjectRequest>
    {
        public CreateProjectValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(200); }
    }
    public sealed class UpdateProjectValidator : AbstractValidator<UpdateProjectRequest>
    {
        public UpdateProjectValidator() { RuleFor(x => x.Id).NotEmpty(); Include(new CreateProjectValidator()); }
    }
}
