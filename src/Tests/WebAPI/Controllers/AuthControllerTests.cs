using Antecipacao.Domain.DTOs;
using Antecipacao.Domain.Entities;
using Antecipacao.Domain.Interfaces;
using Antecipacao.WebAPI.Controllers;
using Antecipacao.Tests.TestUtilities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Antecipacao.Tests.WebAPI.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthenticationService> _mockService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockService = new Mock<IAuthenticationService>();
            _controller = new AuthController(_mockService.Object);
            SetupControllerContext();
        }

        [Fact]
        public async Task Login_ComCredenciaisValidas_DeveRetornarOk()
        {
            // Arrange
            var request = TestDataBuilder.CriarLoginRequestValido();
            var response = new AuthenticationResponse
            {
                Username = request.Username,
                Role = UserRole.User,
                AccessToken = "access-token",
                RefreshToken = "refresh-token"
            };

            _mockService.Setup(x => x.AuthenticateAsync(request, It.IsAny<string>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Login(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();

            _mockService.Verify(x => x.AuthenticateAsync(request, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Login_ComCredenciaisInvalidas_DeveRetornarBadRequest()
        {
            // Arrange
            var request = TestDataBuilder.CriarLoginRequestValido();

            _mockService.Setup(x => x.AuthenticateAsync(request, It.IsAny<string>()))
                .ReturnsAsync((AuthenticationResponse?)null);

            // Act
            var result = await _controller.Login(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(new { erro = "Usuário ou senha incorretos" });

            _mockService.Verify(x => x.AuthenticateAsync(request, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Login_ComExcecao_DeveRetornarInternalServerError()
        {
            // Arrange
            var request = TestDataBuilder.CriarLoginRequestValido();
            var mensagemErro = "Erro interno do servidor";

            _mockService.Setup(x => x.AuthenticateAsync(request, It.IsAny<string>()))
                .ThrowsAsync(new Exception(mensagemErro));

            // Act
            var result = await _controller.Login(request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeEquivalentTo(new { erro = mensagemErro });

            _mockService.Verify(x => x.AuthenticateAsync(request, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Register_ComDadosValidos_DeveRetornarOk()
        {
            // Arrange
            var request = TestDataBuilder.CriarRegisterRequestValido();

            _mockService.Setup(x => x.RegisterAsync(request))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Register(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(new { mensagem = "Registro realizado com sucesso" });

            _mockService.Verify(x => x.RegisterAsync(request), Times.Once);
        }

        [Fact]
        public async Task Register_ComDadosInvalidos_DeveRetornarBadRequest()
        {
            // Arrange
            var request = TestDataBuilder.CriarRegisterRequestInvalido();
            var mensagemErro = "Dados inválidos";

            _mockService.Setup(x => x.RegisterAsync(request))
                .ThrowsAsync(new ApplicationException(mensagemErro));

            // Act
            var result = await _controller.Register(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(new { erro = mensagemErro });

            _mockService.Verify(x => x.RegisterAsync(request), Times.Once);
        }

        [Fact]
        public async Task Register_ComExcecaoGenerica_DeveRetornarInternalServerError()
        {
            // Arrange
            var request = TestDataBuilder.CriarRegisterRequestValido();
            var mensagemErro = "Erro interno do servidor";

            _mockService.Setup(x => x.RegisterAsync(request))
                .ThrowsAsync(new Exception(mensagemErro));

            // Act
            var result = await _controller.Register(request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeEquivalentTo(new { erro = mensagemErro });

            _mockService.Verify(x => x.RegisterAsync(request), Times.Once);
        }

        [Fact]
        public async Task RefreshToken_ComTokenValido_DeveRetornarOk()
        {
            // Arrange
            var refreshToken = "valid-refresh-token";
            var response = new AuthenticationResponse
            {
                Username = "usuario@exemplo.com",
                Role = UserRole.User,
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh-token"
            };

            SetupRefreshTokenCookie(refreshToken);
            _mockService.Setup(x => x.RefreshTokenAsync(refreshToken, It.IsAny<string>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.RefreshToken();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();

            _mockService.Verify(x => x.RefreshTokenAsync(refreshToken, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RefreshToken_SemToken_DeveRetornarBadRequest()
        {
            // Arrange
            SetupRefreshTokenCookie(null);

            // Act
            var result = await _controller.RefreshToken();

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(new { erro = "Token de refresh é obrigatório" });

            _mockService.Verify(x => x.RefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RefreshToken_ComTokenInvalido_DeveRetornarUnauthorized()
        {
            // Arrange
            var refreshToken = "invalid-refresh-token";
            var mensagemErro = "Token inválido";

            SetupRefreshTokenCookie(refreshToken);
            _mockService.Setup(x => x.RefreshTokenAsync(refreshToken, It.IsAny<string>()))
                .ThrowsAsync(new System.Security.SecurityException(mensagemErro));

            // Act
            var result = await _controller.RefreshToken();

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = result as UnauthorizedObjectResult;
            unauthorizedResult!.Value.Should().BeEquivalentTo(new { erro = mensagemErro });

            _mockService.Verify(x => x.RefreshTokenAsync(refreshToken, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Logout_ComToken_DeveRetornarOk()
        {
            // Arrange
            var refreshToken = "valid-refresh-token";
            SetupRefreshTokenCookie(refreshToken);

            _mockService.Setup(x => x.RevokeTokenAsync(refreshToken, It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Logout();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(new { mensagem = "Logout realizado com sucesso" });

            _mockService.Verify(x => x.RevokeTokenAsync(refreshToken, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Logout_SemToken_DeveRetornarOk()
        {
            // Arrange
            SetupRefreshTokenCookie(null);

            // Act
            var result = await _controller.Logout();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(new { mensagem = "Logout realizado com sucesso" });

            _mockService.Verify(x => x.RevokeTokenAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        private void SetupControllerContext()
        {
            var context = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
        }

        private void SetupRefreshTokenCookie(string? token)
        {
            var cookies = new Mock<IRequestCookieCollection>();
            cookies.Setup(x => x["refreshToken"]).Returns(token ?? string.Empty);
            
            _controller.Request.Cookies = cookies.Object;
        }
    }
}
