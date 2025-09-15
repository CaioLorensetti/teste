using Antecipacao.Domain.Entities;
using Antecipacao.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Antecipacao.Infrastructure.Data
{
    public class AntecipacaoDbContext : DbContext
    {
        public AntecipacaoDbContext(DbContextOptions<AntecipacaoDbContext> options) : base(options)
        {
        }

        public DbSet<SolicitacaoAntecipacao> Solicitacoes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new SolicitacaoConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}
