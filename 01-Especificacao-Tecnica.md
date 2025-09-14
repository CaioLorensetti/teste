# Especificação Técnica - API de Antecipação de Valores

## 📋 Visão Geral

**Sistema**: API REST para gestão de solicitações de antecipação de valores  
**Stack**: C# + .NET Core 8.0 + SQLite (In-Memory)  
**Arquitetura**: Clean Architecture + DDD  
**Code**: Clean Code [Código Limpo](./00-Resumo-Executivo.md#codigo-limpo-clean-code)
**Versionamento**: API v1  

## 🏗️ Arquitetura do Sistema

### Estrutura de Camadas (Clean Architecture)

```
├── AntecipacaoAPI/
│   ├── Domain/                 # Entidades e regras de negócio
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Enums/
│   │   └── Interfaces/
│   ├── Application/            # Casos de uso e serviços
│   │   ├── Services/
│   │   ├── DTOs/
│   │   ├── Interfaces/
│   │   └── Validators/
│   ├── Infrastructure/         # Implementações externas
│   │   ├── Data/
│   │   ├── Repositories/
│   │   └── Configuration/
│   └── Presentation/           # Controllers e configuração
│       ├── Controllers/
│       ├── Middleware/
│       ├── Program.cs
│       └── appsettings.json    # Parametrizações globais
```
## 🔧 Regras de Negócio Detalhadas

### Validações de Entrada
1. **Valor Mínimo**: R$ 100,00
   - Deve ficar na parametrização global (appsettings.json)
   - Assim se houver uma mudança futura, a regra estará em um lugar para mudança
2. **Creator ID**: Obrigatório e válido (long > 0)
3. **Data Solicitação**: Não pode ser futura
4. **Solicitação Pendente**: Um creator só pode ter uma solicitação pendente

### Cálculos
- **Taxa Fixa**: 5% sobre o valor bruto
  - Deve ficar na parametrização global (appsettings.json)
  - Assim se houver uma mudança futura, a regra estará em um lugar para mudança
- **Valor Líquido**: Valor solicitado - (Valor solicitado × 0.05)

### Estados da Solicitação
- **Pendente**: Estado inicial
- **Aprovada**: Aprovada por administrador
- **Recusada**: Recusada por administrador

## 🏗️ RESTFull

### Códigos HTTP Padrão
- **200**: Sucesso
- **201**: Criado com sucesso
- **400**: Dados inválidos
- **404**: Recurso não encontrado
- **409**: Conflito (solicitação pendente existente)
- **500**: Erro interno

## 🔒 Segurança e Validações

### Validações de Entrada
- Validação de tipos de dados
- Validação de ranges (valor mínimo)
- Validação de formato (GUIDs, datas)
- Sanitização de inputs

### Middleware
- Logging de requisições
- Tratamento global de exceções
- Validação de modelo automática
- CORS configurado

## 📈 Performance e Escalabilidade

### Otimizações
- Uso de async/await
- Paginação nas listagens
- Cache de configurações
- Logs estruturados

### Monitoramento
- Health checks
- Métricas de performance
- Logs de auditoria

## 📊 Modelos de Dados

### Entidade: SolicitaçãoAntecipacao

```csharp
public class SolicitacaoAntecipacao
{
    public long Id { get; set; }                    // ID sequencial único
    public Guid GuidId { get; set; }                // GUID para uso futuro (não utilizado por enquanto)
    public long CreatorId { get; set; }             // ID sequencial do creator
    public decimal ValorSolicitado { get; set; }
    public decimal TaxaAplicada { get; set; } = 0.05m; // 5%
    public decimal ValorLiquido { get; set; }
    public DateTime DataSolicitacao { get; set; }
    public StatusSolicitacao Status { get; set; } = StatusSolicitacao.Pendente;
    public DateTime? DataAprovacao { get; set; }
    public DateTime? DataRecusa { get; set; }
}
```

### Enum: StatusSolicitacao

```csharp
public enum StatusSolicitacao
{
    Pendente = 0,
    Aprovada = 1,
    Recusada = 2
}
```

### Value Object: ValorMonetario

```csharp
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
```

## 🌐 Endpoints da API

### Base URL: `/api/v1/antecipacao`

#### 1. Criar Solicitação
- **POST** `/api/v1/antecipacao`
- **Body**:
```json
{
  "creatorId": 12345,
  "valorSolicitado": 1000.00,
  "dataSolicitacao": "2024-01-15T10:30:00Z"
}
```
- **Response 201**:
```json
{
  "id": 1,
  "guidId": "550e8400-e29b-41d4-a716-446655440000",
  "creatorId": 12345,
  "valorSolicitado": 1000.00,
  "taxaAplicada": 0.05,
  "valorLiquido": 950.00,
  "dataSolicitacao": "2024-01-15T10:30:00Z",
  "status": "Pendente"
}
```

#### 2. Listar Solicitações por Creator
- **GET** `/api/v1/antecipacao/creator/{creatorId}`
- **Response 200**:
```json
[
  {
    "id": 1,
    "guidId": "550e8400-e29b-41d4-a716-446655440000",
    "creatorId": 12345,
    "valorSolicitado": 1000.00,
    "taxaAplicada": 0.05,
    "valorLiquido": 950.00,
    "dataSolicitacao": "2024-01-15T10:30:00Z",
    "status": "Pendente"
  }
]
```

#### 3. Aprovar Solicitação
- **PUT** `/api/v1/antecipacao/{id}/aprovar`
- **Response 200**:
```json
{
  "id": 1,
  "status": "Aprovada",
  "dataAprovacao": "2024-01-15T11:00:00Z"
}
```

#### 4. Recusar Solicitação
- **PUT** `/api/v1/antecipacao/{id}/recusar`
- **Response 200**:
```json
{
  "id": 1,
  "status": "Recusada",
  "dataRecusa": "2024-01-15T11:00:00Z"
}
```

#### 5. Simular Antecipação (Opcional)
- **GET** `/api/v1/antecipacao/simular?valor=1000.00`
- **Response 200**:
```json
{
  "valorSolicitado": 1000.00,
  "taxaAplicada": 0.05,
  "valorLiquido": 950.00
}
```



### Estrutura de Erro
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Valor deve ser maior que R$ 100,00",
    "details": {
      "field": "valorSolicitado",
      "value": 50.00
    }
  }
}
```