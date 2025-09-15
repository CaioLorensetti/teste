namespace Antecipacao.Domain.ValueObjects
{
    public class ValorMonetario
    {
        public decimal Valor { get; private set; }
        public decimal Taxa { get; private set; }
        public decimal ValorLiquido { get; private set; }
        
        public ValorMonetario(decimal valor, decimal taxa = 0.05m)
        {
            if (valor <= 100)
                throw new ArgumentException("Valor deve ser maior que R$ 100,00");
                
            Valor = valor;
            Taxa = taxa;
            ValorLiquido = valor - (valor * taxa);
        }
    }
}
