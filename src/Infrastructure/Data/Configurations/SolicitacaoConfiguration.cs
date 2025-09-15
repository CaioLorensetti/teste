using Antecipacao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Antecipacao.Infrastructure.Data.Configurations
{
    public class SolicitacaoConfiguration : IEntityTypeConfiguration<SolicitacaoAntecipacao>
    {
        public void Configure(EntityTypeBuilder<SolicitacaoAntecipacao> builder)
        {
            builder.HasKey(s => s.Id);
            
            // Configurar ID como Identity (auto-incremento)
            builder.Property(s => s.Id)
                   .ValueGeneratedOnAdd();
            
            // Configurar GuidId como único (para uso futuro)
            builder.Property(s => s.GuidId)
                   .IsRequired();
            
            builder.HasIndex(s => s.GuidId)
                   .IsUnique();
            
            builder.Property(s => s.CreatorId)
                   .IsRequired();
                   
            builder.Property(s => s.ValorSolicitado)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();
                   
            builder.Property(s => s.TaxaAplicada)
                   .HasColumnType("decimal(5,4)")
                   .IsRequired();
                   
            builder.Property(s => s.ValorLiquido)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();
                   
            builder.Property(s => s.DataSolicitacao)
                   .IsRequired();
                   
            builder.Property(s => s.Status)
                   .HasConversion<string>()
                   .IsRequired();
                   
            builder.Property(s => s.DataAprovacao)
                   .IsRequired(false);
                   
            builder.Property(s => s.DataRecusa)
                   .IsRequired(false);
        }
    }
}
