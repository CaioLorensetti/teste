using System.ComponentModel.DataAnnotations.Schema;

namespace Antecipacao.Domain.DTOs
{
    public class CriarSolicitacaoRequest
    {
        public decimal ValorSolicitado { get; set; }
        public DateTime DataSolicitacao { get; set; }
    }
}
