using Antecipacao.Application.Validators;
using Antecipacao.Domain.DTOs;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Antecipacao.Tests.Application.Validators
{
    public class LoginRequestValidatorTests
    {
        private readonly LoginRequestValidator _validator;

        public LoginRequestValidatorTests()
        {
            _validator = new LoginRequestValidator();
        }

        [Fact]
        public void Validar_ComDadosValidos_DeveSerValido()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "usuario@exemplo.com",
                Password = "MinhaSenh@123"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Validar_ComUsernameVazio_DeveSerInvalido(string username)
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = username,
                Password = "MinhaSenh@123"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Username)
                .WithErrorMessage("Username é obrigatório");
        }

        [Theory]
        [InlineData("usuario")]
        [InlineData("usuario@")]
        [InlineData("@exemplo.com")]
        [InlineData("usuario.exemplo.com")]
        [InlineData("usuario@exemplo")]
        public void Validar_ComEmailInvalido_DeveSerInvalido(string email)
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = email,
                Password = "MinhaSenh@123"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Username)
                .WithErrorMessage("Username deve ser um email válido");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Validar_ComPasswordVazio_DeveSerInvalido(string password)
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "usuario@exemplo.com",
                Password = password
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage("Senha é obrigatória");
        }

        [Theory]
        [InlineData("usuario@exemplo.com")]
        [InlineData("admin@empresa.com.br")]
        [InlineData("teste@dominio.org")]
        [InlineData("user123@teste.net")]
        public void Validar_ComEmailsValidos_DeveSerValido(string email)
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = email,
                Password = "MinhaSenh@123"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Username);
        }

        [Fact]
        public void Validar_ComUsernameEPasswordVazios_DeveTerDoisErros()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "",
                Password = ""
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.Errors.Should().HaveCount(2);
            result.ShouldHaveValidationErrorFor(x => x.Username);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Validar_ComEmailInvalidoEPasswordVazio_DeveTerDoisErros()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "email-invalido",
                Password = ""
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.Errors.Should().HaveCount(2);
            result.ShouldHaveValidationErrorFor(x => x.Username);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
    }
}
