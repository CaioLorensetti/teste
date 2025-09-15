# Guia de Implementação - API de Antecipação de Valores

## ⚠️ Warning
-  The EF Core in-memory database is not designed for performance or robustness and should not be used outside of testing environments. It is not designed for production use.

## 🚀 Setup Inicial do Projeto

### 1. Criar Estrutura de Solução

```bash
# Criar solução
dotnet new sln -n AntecipacaoAPI

# Criar projetos
dotnet new classlib -n AntecipacaoAPI.Domain
dotnet new classlib -n AntecipacaoAPI.Application
dotnet new classlib -n AntecipacaoAPI.Infrastructure
dotnet new webapi -n AntecipacaoAPI.Presentation
dotnet new xunit -n AntecipacaoAPI.Tests

# Adicionar projetos à solução
dotnet sln add AntecipacaoAPI.Domain
dotnet sln add AntecipacaoAPI.Application
dotnet sln add AntecipacaoAPI.Infrastructure
dotnet sln add AntecipacaoAPI.Presentation
dotnet sln add AntecipacaoAPI.Tests
```

### 2. Configurar Dependências

#### Domain Layer (AntecipacaoAPI.Domain)
```xml
<!-- Nenhuma dependência externa necessária -->
```

#### Application Layer (AntecipacaoAPI.Application)
```xml
<PackageReference Include="FluentValidation" Version="11.8.1" />
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.0" />
```

#### Infrastructure Layer (AntecipacaoAPI.Infrastructure)
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="8.0.0" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.3" />
```

#### Presentation Layer (AntecipacaoAPI.Presentation)
```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
```

#### Tests (AntecipacaoAPI.Tests)
```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Moq" Version="4.20.69" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
```

## 🏗️ Estrutura Detalhada de Arquivos

### Domain Layer
```
AntecipacaoAPI.Domain/
├── Entities/
│   ├── SolicitacaoAntecipacao.cs
│   ├── User.cs
│   └── RefreshToken.cs
├── ValueObjects/
│   └── ValorMonetario.cs
├── Enums/
│   └── StatusSolicitacao.cs
├── Interfaces/
│   ├── ISolicitacaoRepository.cs
│   └── IAuthenticationService.cs
└── Exceptions/
    └── BusinessException.cs
```

### Application Layer
```
AntecipacaoAPI.Application/
├── Services/
│   ├── AntecipacaoService.cs
│   └── AuthenticationService.cs
├── DTOs/
│   ├── CriarSolicitacaoDto.cs
│   ├── SolicitacaoResponseDto.cs
│   ├── SimulacaoDto.cs
│   ├── LoginRequest.cs
│   ├── RegisterRequest.cs
│   └── AuthenticationResponse.cs
├── Interfaces/
│   ├── IAntecipacaoService.cs
│   └── IAuthenticationService.cs
├── Validators/
│   ├── CriarSolicitacaoValidator.cs
│   └── LoginRequestValidator.cs
└── Mappings/
    └── SolicitacaoMapping.cs
```

### Infrastructure Layer
```
AntecipacaoAPI.Infrastructure/
├── Data/
│   ├── AntecipacaoDbContext.cs
│   └── Configurations/
│       └── SolicitacaoConfiguration.cs
├── Repositories/
│   └── SolicitacaoRepository.cs
└── Configuration/
    └── DependencyInjection.cs
```

### Presentation Layer
```
AntecipacaoAPI.Presentation/
├── Controllers/
│   ├── AntecipacaoController.cs
│   └── AuthController.cs
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   └── JwtMiddleware.cs
├── Program.cs
└── appsettings.json
```

## 📝 Implementação Passo a Passo

### 1. Domain Layer - Entidades e Value Objects

#### User.cs
```csharp
namespace AntecipacaoAPI.Domain.Entities
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; } // Email válido e único
        public string PasswordHash { get; set; }
        public string Role { get; set; } = "User";
        public DateTime CreatedAt { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
```

#### RefreshToken.cs
```csharp
namespace AntecipacaoAPI.Domain.Entities
{
    public class RefreshToken
    {
        public long Id { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public string CreatedByIp { get; set; }
        public DateTime? Revoked { get; set; }
        public string RevokedByIp { get; set; }
        public string ReplacedByToken { get; set; }
        public string ReasonRevoked { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsRevoked => Revoked != null;
        public bool IsActive => !IsRevoked && !IsExpired;
        
        public long UserId { get; set; }
        public User User { get; set; }
    }
}
```

#### StatusSolicitacao.cs
```csharp
namespace AntecipacaoAPI.Domain.Enums
{
    public enum StatusSolicitacao
    {
        Pendente = 0,
        Aprovada = 1,
        Recusada = 2
    }
}
```

#### ValorMonetario.cs
```csharp
namespace AntecipacaoAPI.Domain.ValueObjects
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
```

#### SolicitacaoAntecipacao.cs
```csharp
namespace AntecipacaoAPI.Domain.Entities
{
    public class SolicitacaoAntecipacao
    {
        public long Id { get; private set; }                    // ID sequencial único
        public Guid GuidId { get; private set; }                // GUID para uso futuro (não utilizado por enquanto)
        public long CreatorId { get; private set; }             // ID sequencial do creator
        public decimal ValorSolicitado { get; private set; }
        public decimal TaxaAplicada { get; private set; }
        public decimal ValorLiquido { get; private set; }
        public DateTime DataSolicitacao { get; private set; }
        public StatusSolicitacao Status { get; private set; }
        public DateTime? DataAprovacao { get; private set; }
        public DateTime? DataRecusa { get; private set; }

        private SolicitacaoAntecipacao() { }

        public SolicitacaoAntecipacao(long creatorId, decimal valorSolicitado, DateTime dataSolicitacao)
        {
            // Id será definido pelo banco de dados (Identity)
            GuidId = Guid.NewGuid(); // Gerar GUID para uso futuro
            CreatorId = creatorId;
            ValorSolicitado = valorSolicitado;
            TaxaAplicada = 0.05m;
            ValorLiquido = valorSolicitado - (valorSolicitado * 0.05m);
            DataSolicitacao = dataSolicitacao;
            Status = StatusSolicitacao.Pendente;
        }

        public void Aprovar()
        {
            if (Status != StatusSolicitacao.Pendente)
                throw new InvalidOperationException("Apenas solicitações pendentes podem ser aprovadas");
                
            Status = StatusSolicitacao.Aprovada;
            DataAprovacao = DateTime.UtcNow;
        }

        public void Recusar()
        {
            if (Status != StatusSolicitacao.Pendente)
                throw new InvalidOperationException("Apenas solicitações pendentes podem ser recusadas");
                
            Status = StatusSolicitacao.Recusada;
            DataRecusa = DateTime.UtcNow;
        }
    }
}
```

### 2. Application Layer - Serviços e DTOs

#### LoginRequest.cs
```csharp
namespace AntecipacaoAPI.Application.DTOs
{
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
```

#### RegisterRequest.cs
```csharp
namespace AntecipacaoAPI.Application.DTOs
{
    public class RegisterRequest
    {
        public string Username { get; set; } // Email válido e único
        public string Password { get; set; }
    }
}
```

#### AuthenticationResponse.cs
```csharp
namespace AntecipacaoAPI.Application.DTOs
{
    public class AuthenticationResponse
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
```

#### CriarSolicitacaoDto.cs
```csharp
namespace AntecipacaoAPI.Application.DTOs
{
    public class CriarSolicitacaoDto
    {
        public long CreatorId { get; set; }
        public decimal ValorSolicitado { get; set; }
        public DateTime DataSolicitacao { get; set; }
    }
}
```

#### SolicitacaoResponseDto.cs
```csharp
namespace AntecipacaoAPI.Application.DTOs
{
    public class SolicitacaoResponseDto
    {
        public long Id { get; set; }
        public Guid GuidId { get; set; }
        public long CreatorId { get; set; }
        public decimal ValorSolicitado { get; set; }
        public decimal TaxaAplicada { get; set; }
        public decimal ValorLiquido { get; set; }
        public DateTime DataSolicitacao { get; set; }
        public string Status { get; set; }
        public DateTime? DataAprovacao { get; set; }
        public DateTime? DataRecusa { get; set; }
    }
}
```

#### AntecipacaoService.cs
```csharp
namespace AntecipacaoAPI.Application.Services
{
    public class AntecipacaoService : IAntecipacaoService
    {
        private readonly ISolicitacaoRepository _repository;

        public AntecipacaoService(ISolicitacaoRepository repository)
        {
            _repository = repository;
        }

        public async Task<SolicitacaoResponseDto> CriarSolicitacaoAsync(CriarSolicitacaoDto dto)
        {
            // Validar valor mínimo
            if (dto.ValorSolicitado <= 100)
                throw new ArgumentException("Valor deve ser maior que R$ 100,00");

            // Verificar se já existe solicitação pendente
            var existePendente = await _repository.ExisteSolicitacaoPendenteAsync(dto.CreatorId);
            if (existePendente)
                throw new InvalidOperationException("Creator já possui uma solicitação pendente");

            // Criar solicitação
            var solicitacao = new SolicitacaoAntecipacao(
                dto.CreatorId, 
                dto.ValorSolicitado, 
                dto.DataSolicitacao
            );

            await _repository.AdicionarAsync(solicitacao);
            await _repository.SaveChangesAsync();

            return MapToResponseDto(solicitacao);
        }

        public async Task<IEnumerable<SolicitacaoResponseDto>> ListarPorCreatorAsync(Guid creatorId)
        {
            var solicitacoes = await _repository.ListarPorCreatorAsync(creatorId);
            return solicitacoes.Select(MapToResponseDto);
        }

        public async Task<SolicitacaoResponseDto> AprovarAsync(long id)
        {
            var solicitacao = await _repository.ObterPorIdAsync(id);
            if (solicitacao == null)
                throw new ArgumentException("Solicitação não encontrada");

            solicitacao.Aprovar();
            await _repository.SaveChangesAsync();

            return MapToResponseDto(solicitacao);
        }

        public async Task<SolicitacaoResponseDto> RecusarAsync(long id)
        {
            var solicitacao = await _repository.ObterPorIdAsync(id);
            if (solicitacao == null)
                throw new ArgumentException("Solicitação não encontrada");

            solicitacao.Recusar();
            await _repository.SaveChangesAsync();

            return MapToResponseDto(solicitacao);
        }

        private static SolicitacaoResponseDto MapToResponseDto(SolicitacaoAntecipacao solicitacao)
        {
            return new SolicitacaoResponseDto
            {
                Id = solicitacao.Id,
                GuidId = solicitacao.GuidId,
                CreatorId = solicitacao.CreatorId,
                ValorSolicitado = solicitacao.ValorSolicitado,
                TaxaAplicada = solicitacao.TaxaAplicada,
                ValorLiquido = solicitacao.ValorLiquido,
                DataSolicitacao = solicitacao.DataSolicitacao,
                Status = solicitacao.Status.ToString(),
                DataAprovacao = solicitacao.DataAprovacao,
                DataRecusa = solicitacao.DataRecusa
            };
        }
    }
}
```

### 3. Infrastructure Layer - Data Access

#### AntecipacaoDbContext.cs
```csharp
namespace AntecipacaoAPI.Infrastructure.Data
{
    public class AntecipacaoDbContext : DbContext
    {
        public AntecipacaoDbContext(DbContextOptions<AntecipacaoDbContext> options) : base(options)
        {
        }

        public DbSet<SolicitacaoAntecipacao> Solicitacoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new SolicitacaoConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}
```

#### SolicitacaoConfiguration.cs
```csharp
namespace AntecipacaoAPI.Infrastructure.Data.Configurations
{
    public class SolicitacaoConfiguration : IEntityTypeConfiguration<SolicitacaoAntecipacao>
    {
        public void Configure(EntityTypeBuilder<SolicitacaoAntecipacao> builder)
        {
            builder.HasKey(s => s.Id);
            
            // Configurar ID como Identity (auto-incremento)
            builder.Property(s => s.Id)
                   .ValueGeneratedOnAdd();
            
            // Configurar GuidId como único (para uso futuro)
            builder.Property(s => s.GuidId)
                   .IsRequired();
            
            builder.HasIndex(s => s.GuidId)
                   .IsUnique();
            
            builder.Property(s => s.CreatorId)
                   .IsRequired();
                   
            builder.Property(s => s.ValorSolicitado)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();
                   
            builder.Property(s => s.TaxaAplicada)
                   .HasColumnType("decimal(5,4)")
                   .IsRequired();
                   
            builder.Property(s => s.ValorLiquido)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();
                   
            builder.Property(s => s.DataSolicitacao)
                   .IsRequired();
                   
            builder.Property(s => s.Status)
                   .HasConversion<string>()
                   .IsRequired();
                   
            builder.Property(s => s.DataAprovacao)
                   .IsRequired(false);
                   
            builder.Property(s => s.DataRecusa)
                   .IsRequired(false);
        }
    }
}
```

### 4. Presentation Layer - Controllers

#### AuthController.cs
```csharp
namespace AntecipacaoAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;

        public AuthController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.AuthenticateAsync(request, GetIpAddress());
                
                if (response == null)
                    return BadRequest(new { message = "Username or password is incorrect" });

                SetTokenCookie(response.RefreshToken);
                
                return Ok(new
                {
                    response.Id,
                    response.Username,
                    response.Role,
                    response.AccessToken
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = await _authService.RegisterAsync(request);
                return Ok(new { message = "Registration successful", userId = user.Id });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(new { message = "Refresh token is required" });

            try
            {
                var response = await _authService.RefreshTokenAsync(refreshToken, GetIpAddress());
                SetTokenCookie(response.RefreshToken);
                
                return Ok(new
                {
                    response.Id,
                    response.Username,
                    response.Role,
                    response.AccessToken
                });
            }
            catch (SecurityException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _authService.RevokeTokenAsync(refreshToken, GetIpAddress());
            }

            Response.Cookies.Delete("refreshToken");
            
            return Ok(new { message = "Logged out successfully" });
        }

        private void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                SameSite = SameSiteMode.Strict,
                Secure = true // Usar HTTPS em produção
            };
            
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        }
    }
}
```

#### AntecipacaoController.cs
```csharp
namespace AntecipacaoAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AntecipacaoController : ControllerBase
    {
        private readonly IAntecipacaoService _service;

        public AntecipacaoController(IAntecipacaoService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<SolicitacaoResponseDto>> CriarSolicitacao(
            [FromBody] CriarSolicitacaoDto dto)
        {
            try
            {
                var result = await _service.CriarSolicitacaoAsync(dto);
                return CreatedAtAction(nameof(ObterSolicitacao), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpGet("creator/{creatorId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<SolicitacaoResponseDto>>> ListarPorCreator(
            long creatorId)
        {
            var solicitacoes = await _service.ListarPorCreatorAsync(creatorId);
            return Ok(solicitacoes);
        }

        [HttpPut("{id}/aprovar")]
        [Authorize]
        public async Task<ActionResult<SolicitacaoResponseDto>> AprovarSolicitacao(long id)
        {
            try
            {
                var result = await _service.AprovarAsync(id);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPut("{id}/recusar")]
        [Authorize]
        public async Task<ActionResult<SolicitacaoResponseDto>> RecusarSolicitacao(long id)
        {
            try
            {
                var result = await _service.RecusarAsync(id);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("simular")]
        [Authorize]
        public ActionResult<SimulacaoDto> SimularAntecipacao([FromQuery] decimal valor)
        {
            if (valor <= 100)
                return BadRequest(new { error = "Valor deve ser maior que R$ 100,00" });

            var taxa = 0.05m;
            var valorLiquido = valor - (valor * taxa);

            return Ok(new SimulacaoDto
            {
                ValorSolicitado = valor,
                TaxaAplicada = taxa,
                ValorLiquido = valorLiquido
            });
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<SolicitacaoResponseDto>> ObterSolicitacao(long id)
        {
            // Implementar se necessário
            return NotFound();
        }
    }
}
```

### 5. Configuração de Dependency Injection

#### Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddSingleton(jwtSettings);

// Database
builder.Services.AddDbContext<AntecipacaoDbContext>(options =>
    options.UseSqlite("Data Source=:memory:"));

// Authentication JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder
            .WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// Repositories
builder.Services.AddScoped<ISolicitacaoRepository, SolicitacaoRepository>();

// Services
builder.Services.AddScoped<IAntecipacaoService, AntecipacaoService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");

// JWT Middleware
app.UseMiddleware<JwtMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AntecipacaoDbContext>();
    context.Database.EnsureCreated();
}

app.Run();
```

## 🧪 Configuração de Testes

### TestBase.cs
```csharp
namespace AntecipacaoAPI.Tests
{
    public class TestBase : IDisposable
    {
        protected readonly AntecipacaoDbContext Context;
        protected readonly IServiceProvider ServiceProvider;

        public TestBase()
        {
            var services = new ServiceCollection();
            services.AddDbContext<AntecipacaoDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            
            services.AddScoped<ISolicitacaoRepository, SolicitacaoRepository>();
            services.AddScoped<IAntecipacaoService, AntecipacaoService>();
            
            ServiceProvider = services.BuildServiceProvider();
            Context = ServiceProvider.GetRequiredService<AntecipacaoDbContext>();
            Context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Context.Dispose();
            ServiceProvider.Dispose();
        }
    }
}
```

## 📋 Checklist de Implementação

### Fase 1 - Estrutura Base
- [ ] Criar solução e projetos
- [ ] Configurar dependências
- [ ] Implementar entidades do domínio
- [ ] Implementar value objects
- [ ] Configurar DbContext

### Fase 2 - Lógica de Negócio
- [ ] Implementar serviços de aplicação
- [ ] Implementar repositórios
- [ ] Implementar validações
- [ ] Configurar dependency injection

### Fase 3 - API
- [ ] Implementar controllers
- [ ] Configurar middleware
- [ ] Implementar tratamento de erros
- [ ] Configurar Swagger

### Fase 4 - Testes
- [ ] Implementar testes unitários
- [ ] Implementar testes de integração
- [ ] Configurar cobertura de código
- [ ] Executar todos os testes

### Fase 5 - Documentação
- [ ] Criar README.md
- [ ] Documentar endpoints
- [ ] Adicionar exemplos de uso
- [ ] Configurar CI/CD (opcional)

## 🚀 Comandos Úteis

### Desenvolvimento
```bash
# Executar aplicação
dotnet run --project AntecipacaoAPI.Presentation

# Executar testes
dotnet test

# Executar testes com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Restaurar dependências
dotnet restore

# Build da solução
dotnet build
```

### Database
```bash
# Adicionar migration
dotnet ef migrations add InitialCreate --project AntecipacaoAPI.Infrastructure --startup-project AntecipacaoAPI.Presentation

# Atualizar database
dotnet ef database update --project AntecipacaoAPI.Infrastructure --startup-project AntecipacaoAPI.Presentation
```
