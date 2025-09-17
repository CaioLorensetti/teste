using Antecipacao.Application.Validators;
using Antecipacao.Domain.DTOs;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Antecipacao.Tests.Application.Validators
{
    public class CriarSolicitacaoValidatorTests
    {
        private readonly CriarSolicitacaoValidator _validator;

        public CriarSolicitacaoValidatorTests()
        {
            _validator = new CriarSolicitacaoValidator();
        }

        [Fact]
        public void Validar_ComValorValido_DeveSerValido()
        {
            // Arrange
            var dto = new CriarSolicitacaoDto
            {
                ValorSolicitado = 1000m,
                DataSolicitacao = DateTime.UtcNow
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(100.00)]
        [InlineData(50.00)]
        [InlineData(0.01)]
        [InlineData(0.00)]
        [InlineData(-100.00)]
        public void Validar_ComValorMenorOuIgualA100_DeveSerInvalido(decimal valor)
        {
            // Arrange
            var dto = new CriarSolicitacaoDto
            {
                ValorSolicitado = valor,
                DataSolicitacao = DateTime.UtcNow
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ValorSolicitado)
                .WithErrorMessage("Valor deve ser maior que R$ 100,00");
        }

        [Fact]
        public void Validar_ComDataFutura_DeveSerInvalido()
        {
            // Arrange
            var dto = new CriarSolicitacaoDto
            {
                ValorSolicitado = 1000m,
                DataSolicitacao = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DataSolicitacao)
                .WithErrorMessage("Data de solicitação não pode ser futura");
        }

        [Fact]
        public void Validar_ComDataAtual_DeveSerValido()
        {
            // Arrange
            var dto = new CriarSolicitacaoDto
            {
                ValorSolicitado = 1000m,
                DataSolicitacao = DateTime.UtcNow
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.DataSolicitacao);
        }

        [Fact]
        public void Validar_ComDataPassada_DeveSerValido()
        {
            // Arrange
            var dto = new CriarSolicitacaoDto
            {
                ValorSolicitado = 1000m,
                DataSolicitacao = DateTime.UtcNow.AddDays(-1)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.DataSolicitacao);
        }

        [Theory]
        [InlineData(100.01)]
        [InlineData(500.50)]
        [InlineData(1000.00)]
        [InlineData(10000.99)]
        public void Validar_ComValoresValidos_DeveSerValido(decimal valor)
        {
            // Arrange
            var dto = new CriarSolicitacaoDto
            {
                ValorSolicitado = valor,
                DataSolicitacao = DateTime.UtcNow
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validar_ComValorValidoEDataFutura_DeveTerDoisErros()
        {
            // Arrange
            var dto = new CriarSolicitacaoDto
            {
                ValorSolicitado = 50m, // Valor inválido
                DataSolicitacao = DateTime.UtcNow.AddDays(1) // Data futura
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.Errors.Should().HaveCount(2);
            result.ShouldHaveValidationErrorFor(x => x.ValorSolicitado);
            result.ShouldHaveValidationErrorFor(x => x.DataSolicitacao);
        }
    }
}
