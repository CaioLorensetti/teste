using Antecipacao.Domain.Entities;
using Antecipacao.Domain.Enums;
using FluentAssertions;

namespace Antecipacao.Tests.Domain.Entities
{
    public class SolicitacaoAntecipacaoTests
    {
        [Fact]
        public void Construtor_ComParametrosValidos_DeveCriarSolicitacaoComStatusPendente()
        {
            // Arrange
            var creatorId = 1L;
            var valorSolicitado = 1000m;
            var dataSolicitacao = DateTime.UtcNow;

            // Act
            var solicitacao = new SolicitacaoAntecipacao(creatorId, valorSolicitado, dataSolicitacao);

            // Assert
            solicitacao.CreatorId.Should().Be(creatorId);
            solicitacao.ValorSolicitado.Should().Be(valorSolicitado);
            solicitacao.DataSolicitacao.Should().Be(dataSolicitacao);
            solicitacao.Status.Should().Be(StatusSolicitacao.Pendente);
            solicitacao.TaxaAplicada.Should().Be(0.05m);
            solicitacao.ValorLiquido.Should().Be(950m); // 1000 - (1000 * 0.05)
            solicitacao.GuidId.Should().NotBeEmpty();
            solicitacao.DataAprovacao.Should().BeNull();
            solicitacao.DataRecusa.Should().BeNull();
        }

        [Fact]
        public void Construtor_ComValorDiferente_DeveCalcularTaxaECorretamente()
        {
            // Arrange
            var creatorId = 1L;
            var valorSolicitado = 2000m;
            var dataSolicitacao = DateTime.UtcNow;

            // Act
            var solicitacao = new SolicitacaoAntecipacao(creatorId, valorSolicitado, dataSolicitacao);

            // Assert
            solicitacao.ValorSolicitado.Should().Be(2000m);
            solicitacao.TaxaAplicada.Should().Be(0.05m);
            solicitacao.ValorLiquido.Should().Be(1900m); // 2000 - (2000 * 0.05)
        }

        [Fact]
        public void Aprovar_ComStatusPendente_DeveAlterarStatusParaAprovada()
        {
            // Arrange
            var solicitacao = CriarSolicitacaoValida();
            var dataAntes = DateTime.UtcNow;

            // Act
            solicitacao.Aprovar();

            // Assert
            solicitacao.Status.Should().Be(StatusSolicitacao.Aprovada);
            solicitacao.DataAprovacao.Should().NotBeNull();
            solicitacao.DataAprovacao.Should().BeOnOrAfter(dataAntes);
            solicitacao.DataRecusa.Should().BeNull();
        }

        [Fact]
        public void Aprovar_ComStatusAprovada_DeveLancarExcecao()
        {
            // Arrange
            var solicitacao = CriarSolicitacaoValida();
            solicitacao.Aprovar(); // Primeira aprovação

            // Act & Assert
            var action = () => solicitacao.Aprovar();
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Apenas solicitações pendentes podem ser aprovadas");
        }

        [Fact]
        public void Aprovar_ComStatusRecusada_DeveLancarExcecao()
        {
            // Arrange
            var solicitacao = CriarSolicitacaoValida();
            solicitacao.Recusar(); // Recusar primeiro

            // Act & Assert
            var action = () => solicitacao.Aprovar();
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Apenas solicitações pendentes podem ser aprovadas");
        }

        [Fact]
        public void Recusar_ComStatusPendente_DeveAlterarStatusParaRecusada()
        {
            // Arrange
            var solicitacao = CriarSolicitacaoValida();
            var dataAntes = DateTime.UtcNow;

            // Act
            solicitacao.Recusar();

            // Assert
            solicitacao.Status.Should().Be(StatusSolicitacao.Recusada);
            solicitacao.DataRecusa.Should().NotBeNull();
            solicitacao.DataRecusa.Should().BeOnOrAfter(dataAntes);
            solicitacao.DataAprovacao.Should().BeNull();
        }

        [Fact]
        public void Recusar_ComStatusAprovada_DeveLancarExcecao()
        {
            // Arrange
            var solicitacao = CriarSolicitacaoValida();
            solicitacao.Aprovar(); // Aprovar primeiro

            // Act & Assert
            var action = () => solicitacao.Recusar();
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Apenas solicitações pendentes podem ser recusadas");
        }

        [Fact]
        public void Recusar_ComStatusRecusada_DeveLancarExcecao()
        {
            // Arrange
            var solicitacao = CriarSolicitacaoValida();
            solicitacao.Recusar(); // Primeira recusa

            // Act & Assert
            var action = () => solicitacao.Recusar();
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Apenas solicitações pendentes podem ser recusadas");
        }

        [Theory]
        [InlineData(100.01)]
        [InlineData(500.50)]
        [InlineData(1000.00)]
        [InlineData(10000.99)]
        public void Construtor_ComValoresValidos_DeveCalcularTaxaCorretamente(decimal valor)
        {
            // Arrange
            var creatorId = 1L;
            var dataSolicitacao = DateTime.UtcNow;
            var valorEsperado = valor - (valor * 0.05m);

            // Act
            var solicitacao = new SolicitacaoAntecipacao(creatorId, valor, dataSolicitacao);

            // Assert
            solicitacao.ValorLiquido.Should().Be(valorEsperado);
        }

        private static SolicitacaoAntecipacao CriarSolicitacaoValida()
        {
            return new SolicitacaoAntecipacao(1L, 1000m, DateTime.UtcNow);
        }
    }
}
