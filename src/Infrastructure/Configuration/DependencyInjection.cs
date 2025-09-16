using Antecipacao.Domain.Interfaces;
using Antecipacao.Domain.ValueObjects;
using Antecipacao.Infrastructure.Data;
using Antecipacao.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Antecipacao.Infrastructure.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<AntecipacaoDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

            // JWT Settings
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
            services.AddSingleton(jwtSettings);

            // Repositories
            services.AddScoped<ISolicitacaoRepository, SolicitacaoRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
