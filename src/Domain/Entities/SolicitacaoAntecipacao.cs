using Antecipacao.Domain.Enums;

namespace Antecipacao.Domain.Entities
{
    public class SolicitacaoAntecipacao
    {
        public long Id { get; private set; }                    // ID sequencial único
        public Guid GuidId { get; private set; }                // GUID para uso futuro (não utilizado por enquanto)
        public long CreatorId { get; private set; }             // ID sequencial do creator
        public decimal ValorSolicitado { get; private set; }
        public decimal TaxaAplicada { get; private set; }
        public decimal ValorLiquido { get; private set; }
        public DateTime DataSolicitacao { get; private set; }
        public StatusSolicitacao Status { get; private set; }
        public DateTime? DataAprovacao { get; private set; }
        public DateTime? DataRecusa { get; private set; }

        private SolicitacaoAntecipacao() { }

        public SolicitacaoAntecipacao(long creatorId, decimal valorSolicitado, DateTime dataSolicitacao)
        {
            // Id será definido pelo banco de dados (Identity)
            GuidId = Guid.NewGuid(); // Gerar GUID para uso futuro
            CreatorId = creatorId;
            ValorSolicitado = valorSolicitado;
            TaxaAplicada = 0.05m;
            ValorLiquido = valorSolicitado - (valorSolicitado * 0.05m);
            DataSolicitacao = dataSolicitacao;
            Status = StatusSolicitacao.Pendente;
        }

        public void Aprovar()
        {
            if (Status != StatusSolicitacao.Pendente)
                throw new InvalidOperationException("Apenas solicitações pendentes podem ser aprovadas");
                
            Status = StatusSolicitacao.Aprovada;
            DataAprovacao = DateTime.UtcNow;
        }

        public void Recusar()
        {
            if (Status != StatusSolicitacao.Pendente)
                throw new InvalidOperationException("Apenas solicitações pendentes podem ser recusadas");
                
            Status = StatusSolicitacao.Recusada;
            DataRecusa = DateTime.UtcNow;
        }
    }
}
