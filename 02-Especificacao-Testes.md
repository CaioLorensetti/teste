# Especificação de Testes - API de Antecipação de Valores

## 🧪 Estratégia de Testes

### Tipos de Testes
- **Testes Unitários**: Lógica de negócio, validações, cálculos
- **Testes de Integração**: Endpoints, repositórios, serviços
- **Testes de Contrato**: Validação de DTOs e responses

### Framework de Testes
- **xUnit**: Framework principal
- **FluentAssertions**: Assertions mais legíveis
- **Moq**: Mocking de dependências
- **Microsoft.AspNetCore.Mvc.Testing**: Testes de integração

## 📋 Cenários de Teste

### 1. Testes Unitários - Domain Layer

#### ValorMonetario Value Object
```csharp
[Test]
public void ValorMonetario_ValorValido_DeveCalcularCorretamente()
{
    // Arrange
    var valor = 1000m;
    var taxa = 0.05m;
    
    // Act
    var valorMonetario = new ValorMonetario(valor, taxa);
    
    // Assert
    valorMonetario.Valor.Should().Be(1000m);
    valorMonetario.Taxa.Should().Be(0.05m);
    valorMonetario.ValorLiquido.Should().Be(950m);
}

[Test]
public void ValorMonetario_ValorMenorQue100_DeveLancarExcecao()
{
    // Arrange & Act & Assert
    var action = () => new ValorMonetario(50m);
    action.Should().Throw<ArgumentException>()
          .WithMessage("Valor deve ser maior que R$ 100,00");
}
```

#### SolicitacaoAntecipacao Entity
```csharp
[Test]
public void SolicitacaoAntecipacao_Criar_DeveInicializarComStatusPendente()
{
    // Arrange
    var creatorId = 12345L;
    var valorSolicitado = 1000m;
    var dataSolicitacao = DateTime.UtcNow;
    
    // Act
    var solicitacao = new SolicitacaoAntecipacao(creatorId, valorSolicitado, dataSolicitacao);
    
    // Assert
    solicitacao.Status.Should().Be(StatusSolicitacao.Pendente);
    solicitacao.ValorLiquido.Should().Be(950m);
    solicitacao.CreatorId.Should().Be(12345L);
    solicitacao.GuidId.Should().NotBeEmpty();
}

[Test]
public void SolicitacaoAntecipacao_Aprovar_DeveAlterarStatusEAtualizarData()
{
    // Arrange
    var solicitacao = new SolicitacaoAntecipacao(12345L, 1000m, DateTime.UtcNow);
    
    // Act
    solicitacao.Aprovar();
    
    // Assert
    solicitacao.Status.Should().Be(StatusSolicitacao.Aprovada);
    solicitacao.DataAprovacao.Should().NotBeNull();
}
```

### 2. Testes Unitários - Application Layer

#### AntecipacaoService
```csharp
[Test]
public async Task CriarSolicitacao_ValorValido_DeveRetornarSolicitacaoCriada()
{
    // Arrange
    var mockRepo = new Mock<ISolicitacaoRepository>();
    var service = new AntecipacaoService(mockRepo.Object);
    var dto = new CriarSolicitacaoDto
    {
        CreatorId = 12345L,
        ValorSolicitado = 1000m,
        DataSolicitacao = DateTime.UtcNow
    };
    
    // Act
    var result = await service.CriarSolicitacaoAsync(dto);
    
    // Assert
    result.Should().NotBeNull();
    result.ValorLiquido.Should().Be(950m);
    result.Status.Should().Be(StatusSolicitacao.Pendente);
    result.CreatorId.Should().Be(12345L);
    result.GuidId.Should().NotBeEmpty();
}

[Test]
public async Task CriarSolicitacao_ValorMenorQue100_DeveLancarExcecao()
{
    // Arrange
    var mockRepo = new Mock<ISolicitacaoRepository>();
    var service = new AntecipacaoService(mockRepo.Object);
    var dto = new CriarSolicitacaoDto
    {
        CreatorId = 12345L,
        ValorSolicitado = 50m,
        DataSolicitacao = DateTime.UtcNow
    };
    
    // Act & Assert
    var action = async () => await service.CriarSolicitacaoAsync(dto);
    await action.Should().ThrowAsync<ArgumentException>();
}

[Test]
public async Task CriarSolicitacao_CreatorComSolicitacaoPendente_DeveLancarExcecao()
{
    // Arrange
    var creatorId = 12345L;
    var mockRepo = new Mock<ISolicitacaoRepository>();
    mockRepo.Setup(x => x.ExisteSolicitacaoPendenteAsync(creatorId))
            .ReturnsAsync(true);
    var service = new AntecipacaoService(mockRepo.Object);
    var dto = new CriarSolicitacaoDto
    {
        CreatorId = creatorId,
        ValorSolicitado = 1000m,
        DataSolicitacao = DateTime.UtcNow
    };
    
    // Act & Assert
    var action = async () => await service.CriarSolicitacaoAsync(dto);
    await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Creator já possui uma solicitação pendente");
}
```

### 3. Testes de Integração - Controllers

#### AntecipacaoController
```csharp
[Test]
public async Task Post_CriarSolicitacao_DeveRetornar201ComDadosCorretos()
{
    // Arrange
    var client = _factory.CreateClient();
    var request = new
    {
        creatorId = 12345L,
        valorSolicitado = 1000m,
        dataSolicitacao = DateTime.UtcNow
    };
    
    // Act
    var response = await client.PostAsJsonAsync("/api/v1/antecipacao", request);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var content = await response.Content.ReadFromJsonAsync<SolicitacaoResponseDto>();
    content.Should().NotBeNull();
    content.ValorLiquido.Should().Be(950m);
    content.Status.Should().Be("Pendente");
    content.CreatorId.Should().Be(12345L);
    content.GuidId.Should().NotBeEmpty();
}

[Test]
public async Task Post_CriarSolicitacao_ValorInvalido_DeveRetornar400()
{
    // Arrange
    var client = _factory.CreateClient();
    var request = new
    {
        creatorId = 12345L,
        valorSolicitado = 50m, // Valor inválido
        dataSolicitacao = DateTime.UtcNow
    };
    
    // Act
    var response = await client.PostAsJsonAsync("/api/v1/antecipacao", request);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
}

[Test]
public async Task Get_ListarPorCreator_DeveRetornarSolicitacoes()
{
    // Arrange
    var client = _factory.CreateClient();
    var creatorId = 12345L;
    
    // Criar uma solicitação primeiro
    await client.PostAsJsonAsync("/api/v1/antecipacao", new
    {
        creatorId = creatorId,
        valorSolicitado = 1000m,
        dataSolicitacao = DateTime.UtcNow
    });
    
    // Act
    var response = await client.GetAsync($"/api/v1/antecipacao/creator/{creatorId}");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var content = await response.Content.ReadFromJsonAsync<List<SolicitacaoResponseDto>>();
    content.Should().HaveCount(1);
}

[Test]
public async Task Put_AprovarSolicitacao_DeveAlterarStatus()
{
    // Arrange
    var client = _factory.CreateClient();
    var creatorId = 12345L;
    
    // Criar solicitação
    var createResponse = await client.PostAsJsonAsync("/api/v1/antecipacao", new
    {
        creatorId = creatorId,
        valorSolicitado = 1000m,
        dataSolicitacao = DateTime.UtcNow
    });
    var solicitacao = await createResponse.Content.ReadFromJsonAsync<SolicitacaoResponseDto>();
    
    // Act
    var response = await client.PutAsync($"/api/v1/antecipacao/{solicitacao.Id}/aprovar", null);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var content = await response.Content.ReadFromJsonAsync<SolicitacaoResponseDto>();
    content.Status.Should().Be("Aprovada");
}
```

### 4. Testes de Integração - Repository

#### SolicitacaoRepository
```csharp
[Test]
public async Task AdicionarAsync_DevePersistirSolicitacao()
{
    // Arrange
    var repository = new SolicitacaoRepository(_context);
    var solicitacao = new SolicitacaoAntecipacao(
        12345L, 
        1000m, 
        DateTime.UtcNow
    );
    
    // Act
    await repository.AdicionarAsync(solicitacao);
    await _context.SaveChangesAsync();
    
    // Assert
    var persistida = await _context.Solicitacoes.FindAsync(solicitacao.Id);
    persistida.Should().NotBeNull();
    persistida.ValorLiquido.Should().Be(950m);
    persistida.CreatorId.Should().Be(12345L);
    persistida.GuidId.Should().NotBeEmpty();
}

[Test]
public async Task ExisteSolicitacaoPendenteAsync_ComSolicitacaoPendente_DeveRetornarTrue()
{
    // Arrange
    var creatorId = 12345L;
    var repository = new SolicitacaoRepository(_context);
    var solicitacao = new SolicitacaoAntecipacao(creatorId, 1000m, DateTime.UtcNow);
    await repository.AdicionarAsync(solicitacao);
    await _context.SaveChangesAsync();
    
    // Act
    var existe = await repository.ExisteSolicitacaoPendenteAsync(creatorId);
    
    // Assert
    existe.Should().BeTrue();
}
```

## 🔄 Testes de Performance

### Testes de Carga
```csharp
[Test]
public async Task CriarSolicitacao_1000Requisicoes_DeveCompletarEmMenosDe5Segundos()
{
    // Arrange
    var client = _factory.CreateClient();
    var tasks = new List<Task<HttpResponseMessage>>();
    
    // Act
    var stopwatch = Stopwatch.StartNew();
    for (int i = 0; i < 1000; i++)
    {
        var request = new
        {
            creatorId = 12345L + i,
            valorSolicitado = 1000m + i,
            dataSolicitacao = DateTime.UtcNow
        };
        tasks.Add(client.PostAsJsonAsync("/api/v1/antecipacao", request));
    }
    
    await Task.WhenAll(tasks);
    stopwatch.Stop();
    
    // Assert
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000);
    tasks.All(t => t.Result.IsSuccessStatusCode).Should().BeTrue();
}
```

## 📊 Cobertura de Testes

### Metas de Cobertura
- **Domain Layer**: 100% (regras de negócio críticas)
- **Application Layer**: 95% (casos de uso)
- **Infrastructure Layer**: 90% (repositórios)
- **Presentation Layer**: 85% (controllers)

### Relatório de Cobertura
```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage" -reporttypes:"Html"
```

## 🚀 Configuração de Testes

### Setup do Ambiente de Teste
```csharp
public class WebApplicationFactory : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly IServiceScope _scope;
    private readonly AntecipacaoDbContext _context;

    public WebApplicationFactory(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Substituir DbContext por in-memory
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AntecipacaoDbContext>));
                services.Remove(descriptor);
                
                services.AddDbContext<AntecipacaoDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });
        
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<AntecipacaoDbContext>();
    }

    public void Dispose()
    {
        _context.Dispose();
        _scope.Dispose();
    }
}
```

## 📝 Checklist de Testes

### Antes de cada commit:
- [ ] Todos os testes unitários passando
- [ ] Todos os testes de integração passando
- [ ] Cobertura de código acima das metas
- [ ] Testes de performance dentro dos limites
- [ ] Validação de contratos de API

### Cenários críticos cobertos:
- [ ] Validação de valor mínimo
- [ ] Prevenção de múltiplas solicitações pendentes
- [ ] Cálculo correto de taxa e valor líquido
- [ ] Transições de status válidas
- [ ] Tratamento de erros de validação
- [ ] Persistência e recuperação de dados
