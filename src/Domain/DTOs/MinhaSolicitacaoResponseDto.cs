namespace Antecipacao.Domain.DTOs
{
    public class MinhaSolicitacaoResponseDto
    {
        public Guid GuidId { get; set; }
        public decimal ValorSolicitado { get; set; }
        public decimal TaxaAplicada { get; set; }
        public decimal ValorLiquido { get; set; }
        public DateTime DataSolicitacao { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? DataAprovacao { get; set; }
        public DateTime? DataRecusa { get; set; }
    }
}
