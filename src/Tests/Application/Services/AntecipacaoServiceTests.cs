using Antecipacao.Application.Services;
using Antecipacao.Domain.DTOs;
using Antecipacao.Domain.Entities;
using Antecipacao.Domain.Enums;
using Antecipacao.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Antecipacao.Tests.Application.Services
{
    public class AntecipacaoServiceTests
    {
        private readonly Mock<ISolicitacaoRepository> _mockRepository;
        private readonly AntecipacaoService _service;

        public AntecipacaoServiceTests()
        {
            _mockRepository = new Mock<ISolicitacaoRepository>();
            _service = new AntecipacaoService(_mockRepository.Object);
        }

        [Fact]
        public async Task CriarSolicitacaoAsync_ComDadosValidos_DeveCriarSolicitacao()
        {
            // Arrange
            var creatorId = 1L;
            var request = new CriarSolicitacaoRequest
            {
                ValorSolicitado = 1000m,
                DataSolicitacao = DateTime.UtcNow
            };

            _mockRepository.Setup(x => x.ExisteSolicitacaoPendenteAsync(creatorId))
                .ReturnsAsync(false);
            _mockRepository.Setup(x => x.AdicionarAsync(It.IsAny<SolicitacaoAntecipacao>()))
                .ReturnsAsync(It.IsAny<SolicitacaoAntecipacao>());
            _mockRepository.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CriarSolicitacaoAsync(creatorId, request);

            // Assert
            result.Should().NotBeNull();
            result.ValorSolicitado.Should().Be(1000m);
            result.Status.Should().Be(StatusSolicitacao.Pendente.ToString());
            result.GuidId.Should().NotBeEmpty();

            _mockRepository.Verify(x => x.ExisteSolicitacaoPendenteAsync(creatorId), Times.Once);
            _mockRepository.Verify(x => x.AdicionarAsync(It.IsAny<SolicitacaoAntecipacao>()), Times.Once);
            _mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CriarSolicitacaoAsync_ComValorMenorQue100_DeveLancarExcecao()
        {
            // Arrange
            var creatorId = 1L;
            var request = new CriarSolicitacaoRequest
            {
                ValorSolicitado = 50m,
                DataSolicitacao = DateTime.UtcNow
            };

            // Act & Assert
            var action = async () => await _service.CriarSolicitacaoAsync(creatorId, request);
            await action.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Valor deve ser maior que R$ 100,00");

            _mockRepository.Verify(x => x.ExisteSolicitacaoPendenteAsync(It.IsAny<long>()), Times.Never);
        }

        [Fact]
        public async Task CriarSolicitacaoAsync_ComSolicitacaoPendente_DeveLancarExcecao()
        {
            // Arrange
            var creatorId = 1L;
            var request = new CriarSolicitacaoRequest
            {
                ValorSolicitado = 1000m,
                DataSolicitacao = DateTime.UtcNow
            };

            _mockRepository.Setup(x => x.ExisteSolicitacaoPendenteAsync(creatorId))
                .ReturnsAsync(true);

            // Act & Assert
            var action = async () => await _service.CriarSolicitacaoAsync(creatorId, request);
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Usuário já possui uma solicitação pendente");

            _mockRepository.Verify(x => x.ExisteSolicitacaoPendenteAsync(creatorId), Times.Once);
        }

        [Fact]
        public async Task ListarPorCreatorAsync_ComCreatorValido_DeveRetornarSolicitacoes()
        {
            // Arrange
            var creatorId = 1L;
            var solicitacoes = new List<SolicitacaoAntecipacao>
            {
                CriarSolicitacao(1L, 1000m),
                CriarSolicitacao(1L, 2000m)
            };

            _mockRepository.Setup(x => x.ListarPorCreatorAsync(creatorId))
                .ReturnsAsync(solicitacoes);

            // Act
            var result = await _service.ListarPorCreatorAsync(creatorId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(s => s.Should().NotBeNull());

            _mockRepository.Verify(x => x.ListarPorCreatorAsync(creatorId), Times.Once);
        }

        [Fact]
        public async Task AprovarAsync_ComGuidValido_DeveAprovarSolicitacao()
        {
            // Arrange
            var guidId = Guid.NewGuid();
            var solicitacao = CriarSolicitacao(1L, 1000m);

            _mockRepository.Setup(x => x.ObterPorGuidIdAsync(guidId))
                .ReturnsAsync(solicitacao);
            _mockRepository.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.AprovarAsync(guidId);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(StatusSolicitacao.Aprovada.ToString());
            solicitacao.Status.Should().Be(StatusSolicitacao.Aprovada);
            solicitacao.DataAprovacao.Should().NotBeNull();

            _mockRepository.Verify(x => x.ObterPorGuidIdAsync(guidId), Times.Once);
            _mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AprovarAsync_ComGuidInexistente_DeveLancarExcecao()
        {
            // Arrange
            var guidId = Guid.NewGuid();

            _mockRepository.Setup(x => x.ObterPorGuidIdAsync(guidId))
                .ReturnsAsync((SolicitacaoAntecipacao?)null);

            // Act & Assert
            var action = async () => await _service.AprovarAsync(guidId);
            await action.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Solicitação não encontrada");

            _mockRepository.Verify(x => x.ObterPorGuidIdAsync(guidId), Times.Once);
        }

        [Fact]
        public async Task RecusarAsync_ComGuidValido_DeveRecusarSolicitacao()
        {
            // Arrange
            var guidId = Guid.NewGuid();
            var solicitacao = CriarSolicitacao(1L, 1000m);

            _mockRepository.Setup(x => x.ObterPorGuidIdAsync(guidId))
                .ReturnsAsync(solicitacao);
            _mockRepository.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.RecusarAsync(guidId);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(StatusSolicitacao.Recusada.ToString());
            solicitacao.Status.Should().Be(StatusSolicitacao.Recusada);
            solicitacao.DataRecusa.Should().NotBeNull();

            _mockRepository.Verify(x => x.ObterPorGuidIdAsync(guidId), Times.Once);
            _mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RecusarAsync_ComGuidInexistente_DeveLancarExcecao()
        {
            // Arrange
            var guidId = Guid.NewGuid();

            _mockRepository.Setup(x => x.ObterPorGuidIdAsync(guidId))
                .ReturnsAsync((SolicitacaoAntecipacao?)null);

            // Act & Assert
            var action = async () => await _service.RecusarAsync(guidId);
            await action.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Solicitação não encontrada");

            _mockRepository.Verify(x => x.ObterPorGuidIdAsync(guidId), Times.Once);
        }

        [Fact]
        public async Task SimularAntecipacaoAsync_ComValorValido_DeveRetornarSimulacao()
        {
            // Arrange
            var valor = 1000m;

            // Act
            var result = await _service.SimularAntecipacaoAsync(valor);

            // Assert
            result.Should().NotBeNull();
            result.ValorSolicitado.Should().Be(1000m);
            result.TaxaAplicada.Should().Be(0.05m);
            result.ValorLiquido.Should().Be(950m);
        }

        [Theory]
        [InlineData(100.00)]
        [InlineData(50.00)]
        [InlineData(0.01)]
        [InlineData(0.00)]
        [InlineData(-100.00)]
        public async Task SimularAntecipacaoAsync_ComValorInvalido_DeveLancarExcecao(decimal valor)
        {
            // Act & Assert
            var action = async () => await _service.SimularAntecipacaoAsync(valor);
            await action.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Valor deve ser maior que R$ 100,00");
        }

        [Theory]
        [InlineData(100.01, 950.005)]
        [InlineData(500.00, 475.00)]
        [InlineData(1000.00, 950.00)]
        [InlineData(2000.00, 1900.00)]
        public async Task SimularAntecipacaoAsync_ComValoresValidos_DeveCalcularCorretamente(
            decimal valor, 
            decimal valorLiquidoEsperado)
        {
            // Act
            var result = await _service.SimularAntecipacaoAsync(valor);

            // Assert
            result.ValorSolicitado.Should().Be(valor);
            result.ValorLiquido.Should().Be(valorLiquidoEsperado);
            result.TaxaAplicada.Should().Be(0.05m);
        }

        private static SolicitacaoAntecipacao CriarSolicitacao(long creatorId, decimal valor)
        {
            return new SolicitacaoAntecipacao(creatorId, valor, DateTime.UtcNow);
        }
    }
}
