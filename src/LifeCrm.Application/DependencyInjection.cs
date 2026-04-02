using FluentValidation;
using LifeCrm.Application.Campaigns.Commands;
using LifeCrm.Application.Common.Behaviours;
using LifeCrm.Application.Contacts.Commands;
using LifeCrm.Application.Common.Mappings;
using LifeCrm.Application.Donations.Commands;
using LifeCrm.Application.Interactions.Commands;
using LifeCrm.Application.Projects.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LifeCrm.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            MapsterConfig.RegisterMappings();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehaviour<,>));
            services.AddValidatorsFromAssemblyContaining<CreateContactValidator>();
            return services;
        }
    }
}
