using Antecipacao.Domain.DTOs;
using Antecipacao.Domain.Entities;
using Antecipacao.Domain.Enums;

namespace Antecipacao.Tests.TestUtilities
{
    public static class TestDataBuilder
    {
        public static CriarSolicitacaoRequest CriarSolicitacaoRequestValida(decimal valor = 1000m)
        {
            return new CriarSolicitacaoRequest
            {
                ValorSolicitado = valor,
                DataSolicitacao = DateTime.UtcNow
            };
        }

        public static CriarSolicitacaoRequest CriarSolicitacaoRequestInvalida(decimal valor = 50m)
        {
            return new CriarSolicitacaoRequest
            {
                ValorSolicitado = valor,
                DataSolicitacao = DateTime.UtcNow
            };
        }

        public static SolicitacaoAntecipacao CriarSolicitacaoAntecipacao(
            long creatorId = 1L, 
            decimal valor = 1000m, 
            StatusSolicitacao status = StatusSolicitacao.Pendente)
        {
            var solicitacao = new SolicitacaoAntecipacao(creatorId, valor, DateTime.UtcNow);
            
            if (status == StatusSolicitacao.Aprovada)
                solicitacao.Aprovar();
            else if (status == StatusSolicitacao.Recusada)
                solicitacao.Recusar();

            return solicitacao;
        }

        public static MinhaSolicitacaoResponseDto CriarMinhaSolicitacaoResponseDto(
            Guid? guidId = null, 
            decimal valor = 1000m, 
            string status = "Pendente")
        {
            return new MinhaSolicitacaoResponseDto
            {
                GuidId = guidId ?? Guid.NewGuid(),
                ValorSolicitado = valor,
                TaxaAplicada = 0.05m,
                ValorLiquido = valor - (valor * 0.05m),
                DataSolicitacao = DateTime.UtcNow,
                Status = status,
                DataAprovacao = status == "Aprovada" ? DateTime.UtcNow : null,
                DataRecusa = status == "Recusada" ? DateTime.UtcNow : null
            };
        }

        public static SimulacaoDto CriarSimulacaoDto(decimal valor = 1000m)
        {
            return new SimulacaoDto
            {
                ValorSolicitado = valor,
                TaxaAplicada = 0.05m,
                ValorLiquido = valor - (valor * 0.05m)
            };
        }

        public static LoginRequest CriarLoginRequestValido(string email = "usuario@exemplo.com")
        {
            return new LoginRequest
            {
                Username = email,
                Password = "MinhaSenh@123"
            };
        }

        public static LoginRequest CriarLoginRequestInvalido(string email = "", string password = "")
        {
            return new LoginRequest
            {
                Username = email,
                Password = password
            };
        }

        public static RegisterRequest CriarRegisterRequestValido()
        {
            return new RegisterRequest
            {
                Fullname = "Usuário Teste",
                Username = "novo@exemplo.com",
                Password = "NovaSenh@123"
            };
        }

        public static RegisterRequest CriarRegisterRequestInvalido()
        {
            return new RegisterRequest
            {
                Fullname = "",
                Username = "email-invalido",
                Password = "123"
            };
        }

        public static List<SolicitacaoAntecipacao> CriarListaSolicitacoes(long creatorId = 1L)
        {
            return new List<SolicitacaoAntecipacao>
            {
                CriarSolicitacaoAntecipacao(creatorId, 1000m, StatusSolicitacao.Pendente),
                CriarSolicitacaoAntecipacao(creatorId, 2000m, StatusSolicitacao.Aprovada),
                CriarSolicitacaoAntecipacao(creatorId, 500m, StatusSolicitacao.Recusada)
            };
        }

        public static List<MinhaSolicitacaoResponseDto> CriarListaMinhaSolicitacaoResponseDto(long creatorId = 1L)
        {
            return new List<MinhaSolicitacaoResponseDto>
            {
                CriarMinhaSolicitacaoResponseDto(Guid.NewGuid(), 1000m, "Pendente"),
                CriarMinhaSolicitacaoResponseDto(Guid.NewGuid(), 2000m, "Aprovada"),
                CriarMinhaSolicitacaoResponseDto(Guid.NewGuid(), 500m, "Recusada")
            };
        }
    }
}
