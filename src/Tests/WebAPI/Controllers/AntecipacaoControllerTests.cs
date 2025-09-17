using Antecipacao.Domain.DTOs;
using Antecipacao.Domain.Interfaces;
using Antecipacao.WebAPI.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace Antecipacao.Tests.WebAPI.Controllers
{
    public class AntecipacaoControllerTests
    {
        private readonly Mock<IAntecipacaoService> _mockService;
        private readonly AntecipacaoController _controller;

        public AntecipacaoControllerTests()
        {
            _mockService = new Mock<IAntecipacaoService>();
            _controller = new AntecipacaoController(_mockService.Object);
        }

        [Fact]
        public async Task SimularAntecipacao_ComValorValido_DeveRetornarOk()
        {
            // Arrange
            var valor = 1000m;
            var simulacao = new SimulacaoDto
            {
                ValorSolicitado = 1000m,
                TaxaAplicada = 0.05m,
                ValorLiquido = 950m
            };

            _mockService.Setup(x => x.SimularAntecipacaoAsync(valor))
                .ReturnsAsync(simulacao);

            // Act
            var result = await _controller.SimularAntecipacao(valor);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(simulacao);

            _mockService.Verify(x => x.SimularAntecipacaoAsync(valor), Times.Once);
        }

        [Fact]
        public async Task SimularAntecipacao_ComValorInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var valor = 50m;
            var mensagemErro = "Valor deve ser maior que R$ 100,00";

            _mockService.Setup(x => x.SimularAntecipacaoAsync(valor))
                .ThrowsAsync(new ArgumentException(mensagemErro));

            // Act
            var result = await _controller.SimularAntecipacao(valor);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(new { erro = mensagemErro });

            _mockService.Verify(x => x.SimularAntecipacaoAsync(valor), Times.Once);
        }

        [Fact]
        public async Task SimularAntecipacaoV2_ComValorValido_DeveRetornarOkComInformacoesAdicionais()
        {
            // Arrange
            var valor = 1000m;
            var simulacao = new SimulacaoDto
            {
                ValorSolicitado = 1000m,
                TaxaAplicada = 0.05m,
                ValorLiquido = 950m
            };

            _mockService.Setup(x => x.SimularAntecipacaoAsync(valor))
                .ReturnsAsync(simulacao);

            // Act
            var result = await _controller.SimularAntecipacaoV2(valor);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value;

            response.Should().NotBeNull();
            // Verifica se o objeto tem as propriedades esperadas
            var responseObj = response as dynamic;
            responseObj.Should().NotBeNull();

            _mockService.Verify(x => x.SimularAntecipacaoAsync(valor), Times.Once);
        }

        [Fact]
        public async Task CriarSolicitacao_ComUsuarioAutenticado_DeveRetornarCreated()
        {
            // Arrange
            var userId = 1L;
            var request = new CriarSolicitacaoRequest
            {
                ValorSolicitado = 1000m,
                DataSolicitacao = DateTime.UtcNow
            };

            var solicitacaoCriada = new MinhaSolicitacaoResponseDto
            {
                GuidId = Guid.NewGuid(),
                ValorSolicitado = 1000m,
                Status = "Pendente"
            };

            SetupUserContext(userId);
            _mockService.Setup(x => x.CriarSolicitacaoAsync(userId, request))
                .ReturnsAsync(solicitacaoCriada);

            // Act
            var result = await _controller.CriarSolicitacao(request);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult!.Value.Should().BeEquivalentTo(solicitacaoCriada);

            _mockService.Verify(x => x.CriarSolicitacaoAsync(userId, request), Times.Once);
        }

        [Fact]
        public async Task CriarSolicitacao_ComUsuarioNaoAutenticado_DeveRetornarUnauthorized()
        {
            // Arrange
            var request = new CriarSolicitacaoRequest
            {
                ValorSolicitado = 1000m,
                DataSolicitacao = DateTime.UtcNow
            };

            SetupUserContext(null); // Usuário não autenticado

            // Act
            var result = await _controller.CriarSolicitacao(request);

            // Assert
            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = result.Result as UnauthorizedObjectResult;
            unauthorizedResult!.Value.Should().BeEquivalentTo(new { erro = "Token inválido ou usuário não identificado." });

            _mockService.Verify(x => x.CriarSolicitacaoAsync(It.IsAny<long>(), It.IsAny<CriarSolicitacaoRequest>()), Times.Never);
        }

        [Fact]
        public async Task CriarSolicitacao_ComValorInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var userId = 1L;
            var request = new CriarSolicitacaoRequest
            {
                ValorSolicitado = 50m,
                DataSolicitacao = DateTime.UtcNow
            };

            var mensagemErro = "Valor deve ser maior que R$ 100,00";

            SetupUserContext(userId);
            _mockService.Setup(x => x.CriarSolicitacaoAsync(userId, request))
                .ThrowsAsync(new ArgumentException(mensagemErro));

            // Act
            var result = await _controller.CriarSolicitacao(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(new { erro = mensagemErro });

            _mockService.Verify(x => x.CriarSolicitacaoAsync(userId, request), Times.Once);
        }

        [Fact]
        public async Task ListarMinhasSolicitacoes_ComUsuarioAutenticado_DeveRetornarOk()
        {
            // Arrange
            var userId = 1L;
            var solicitacoes = new List<MinhaSolicitacaoResponseDto>
            {
                new() { GuidId = Guid.NewGuid(), ValorSolicitado = 1000m, Status = "Pendente" },
                new() { GuidId = Guid.NewGuid(), ValorSolicitado = 2000m, Status = "Aprovada" }
            };

            SetupUserContext(userId);
            _mockService.Setup(x => x.ListarPorCreatorAsync(userId))
                .ReturnsAsync(solicitacoes);

            // Act
            var result = await _controller.ListarMinhasSolicitacoes();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(solicitacoes);

            _mockService.Verify(x => x.ListarPorCreatorAsync(userId), Times.Once);
        }

        [Fact]
        public async Task AprovarSolicitacao_ComGuidValido_DeveRetornarOk()
        {
            // Arrange
            var guidId = Guid.NewGuid();
            var solicitacaoAprovada = new MinhaSolicitacaoResponseDto
            {
                GuidId = guidId,
                Status = "Aprovada"
            };

            _mockService.Setup(x => x.AprovarAsync(guidId))
                .ReturnsAsync(solicitacaoAprovada);

            // Act
            var result = await _controller.AprovarSolicitacao(guidId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(solicitacaoAprovada);

            _mockService.Verify(x => x.AprovarAsync(guidId), Times.Once);
        }

        [Fact]
        public async Task AprovarSolicitacao_ComGuidInexistente_DeveRetornarNotFound()
        {
            // Arrange
            var guidId = Guid.NewGuid();
            var mensagemErro = "Solicitação não encontrada";

            _mockService.Setup(x => x.AprovarAsync(guidId))
                .ThrowsAsync(new ArgumentException(mensagemErro));

            // Act
            var result = await _controller.AprovarSolicitacao(guidId);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult!.Value.Should().BeEquivalentTo(new { erro = mensagemErro });

            _mockService.Verify(x => x.AprovarAsync(guidId), Times.Once);
        }

        [Fact]
        public async Task RecusarSolicitacao_ComGuidValido_DeveRetornarOk()
        {
            // Arrange
            var guidId = Guid.NewGuid();
            var solicitacaoRecusada = new MinhaSolicitacaoResponseDto
            {
                GuidId = guidId,
                Status = "Recusada"
            };

            _mockService.Setup(x => x.RecusarAsync(guidId))
                .ReturnsAsync(solicitacaoRecusada);

            // Act
            var result = await _controller.RecusarSolicitacao(guidId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(solicitacaoRecusada);

            _mockService.Verify(x => x.RecusarAsync(guidId), Times.Once);
        }

        [Fact]
        public async Task RecusarSolicitacao_ComGuidInexistente_DeveRetornarNotFound()
        {
            // Arrange
            var guidId = Guid.NewGuid();
            var mensagemErro = "Solicitação não encontrada";

            _mockService.Setup(x => x.RecusarAsync(guidId))
                .ThrowsAsync(new ArgumentException(mensagemErro));

            // Act
            var result = await _controller.RecusarSolicitacao(guidId);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult!.Value.Should().BeEquivalentTo(new { erro = mensagemErro });

            _mockService.Verify(x => x.RecusarAsync(guidId), Times.Once);
        }

        private void SetupUserContext(long? userId)
        {
            var claims = new List<Claim>();
            if (userId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var context = new DefaultHttpContext { User = principal };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
        }
    }
}
