using LifeCrm.Application.Common.Behaviours;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;
using LifeCrm.Infrastructure.Persistence.Repositories;
using LifeCrm.Infrastructure.Persistence.Seeders;
using LifeCrm.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LifeCrm.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseSqlServer(config.GetConnectionString("DefaultConnection")));

            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IOrganizationReader, OrganizationReader>();
            services.AddScoped<IAuditLogWriter, AuditLogWriter>();
            services.AddScoped<ICsvService, CsvService>();
            services.AddScoped<IPdfService, PdfService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<DatabaseSeeder>();

            return services;
        }
    }
}
