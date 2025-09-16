namespace Antecipacao.Domain.DTOs
{
    public class CriarSolicitacaoDto
    {
        public long IdCriador { get; set; }
        public decimal ValorSolicitado { get; set; }
        public DateTime DataSolicitacao { get; set; }
    }
}
