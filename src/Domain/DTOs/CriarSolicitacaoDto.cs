using System.ComponentModel.DataAnnotations.Schema;

namespace Antecipacao.Domain.DTOs
{
    public class CriarSolicitacaoDto
    {
        [NotMapped]
        public long IdCriador { get; set; }
        public decimal ValorSolicitado { get; set; }
        public DateTime DataSolicitacao { get; set; }
    }
}
