using Antecipacao.Domain.ValueObjects;
using FluentAssertions;

namespace Antecipacao.Tests.Domain.ValueObjects
{
    public class ValorMonetarioTests
    {
        [Fact]
        public void Construtor_ComValorValido_DeveCriarValorMonetario()
        {
            // Arrange
            var valor = 1000m;
            var taxa = 0.05m;

            // Act
            var valorMonetario = new ValorMonetario(valor, taxa);

            // Assert
            valorMonetario.Valor.Should().Be(valor);
            valorMonetario.Taxa.Should().Be(taxa);
            valorMonetario.ValorLiquido.Should().Be(950m); // 1000 - (1000 * 0.05)
        }

        [Fact]
        public void Construtor_ComTaxaPadrao_DeveUsarTaxaDe5Porcento()
        {
            // Arrange
            var valor = 1000m;

            // Act
            var valorMonetario = new ValorMonetario(valor);

            // Assert
            valorMonetario.Valor.Should().Be(valor);
            valorMonetario.Taxa.Should().Be(0.05m);
            valorMonetario.ValorLiquido.Should().Be(950m);
        }

        [Theory]
        [InlineData(100.00)]
        [InlineData(50.00)]
        [InlineData(0.01)]
        [InlineData(0.00)]
        [InlineData(-100.00)]
        public void Construtor_ComValorMenorOuIgualA100_DeveLancarExcecao(decimal valor)
        {
            // Act & Assert
            var action = () => new ValorMonetario(valor);
            action.Should().Throw<ArgumentException>()
                .WithMessage("Valor deve ser maior que R$ 100,00");
        }

        [Theory]
        [InlineData(100.01, 0.05, 95.0095)]
        [InlineData(500.00, 0.10, 450.00)]
        [InlineData(1000.00, 0.03, 970.00)]
        [InlineData(2000.00, 0.15, 1700.00)]
        public void Construtor_ComValoresValidos_DeveCalcularValorLiquidoCorretamente(
            decimal valor, 
            decimal taxa, 
            decimal valorLiquidoEsperado)
        {
            // Act
            var valorMonetario = new ValorMonetario(valor, taxa);

            // Assert
            valorMonetario.Valor.Should().Be(valor);
            valorMonetario.Taxa.Should().Be(taxa);
            valorMonetario.ValorLiquido.Should().Be(valorLiquidoEsperado);
        }

        [Fact]
        public void Construtor_ComTaxaZero_DeveCalcularValorLiquidoIgualAoValor()
        {
            // Arrange
            var valor = 1000m;
            var taxa = 0m;

            // Act
            var valorMonetario = new ValorMonetario(valor, taxa);

            // Assert
            valorMonetario.Valor.Should().Be(valor);
            valorMonetario.Taxa.Should().Be(taxa);
            valorMonetario.ValorLiquido.Should().Be(valor);
        }

        [Fact]
        public void Construtor_ComTaxa100Porcento_DeveCalcularValorLiquidoZero()
        {
            // Arrange
            var valor = 1000m;
            var taxa = 1m;

            // Act
            var valorMonetario = new ValorMonetario(valor, taxa);

            // Assert
            valorMonetario.Valor.Should().Be(valor);
            valorMonetario.Taxa.Should().Be(taxa);
            valorMonetario.ValorLiquido.Should().Be(0m);
        }
    }
}
