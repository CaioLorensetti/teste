using Antecipacao.Application.Services;
using Antecipacao.Domain.Interfaces;
using Antecipacao.Domain.ValueObjects;
using Antecipacao.Infrastructure.Configuration;
using Antecipacao.WebAPI.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var configuracoesJwt = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddInfrastructure(builder.Configuration);

ConfigurarAutenticacaoJwt(builder.Services, configuracoesJwt);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder
            .WithOrigins("http://localhost:5016", "https://localhost:7282") // Frontend URL
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

ConfigurarVersionamento(builder.Services);
ConfigurarSwagger(builder.Services);

// Services
builder.Services.AddScoped<IAntecipacaoService, AntecipacaoService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<Antecipacao.Infrastructure.Data.SeedDataService>();

var app = builder.Build();

// Desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Antecipação API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Antecipação API v2");
        options.DocumentTitle = "Antecipação API - Documentação Completa";
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        options.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");

// JWT Middleware
app.UseMiddleware<JwtMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created and seed admin user
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Antecipacao.Infrastructure.Data.AntecipacaoDbContext>();
    context.Database.EnsureCreated();

    // Seed admin user
    var seedDataService = scope.ServiceProvider.GetRequiredService<Antecipacao.Infrastructure.Data.SeedDataService>();
    await seedDataService.SeedAdminUserAsync();
}

app.Run();

static void ConfigurarVersionamento(IServiceCollection services)
{
    services.AddApiVersioning(options =>
    {
        // Define a versão padrão da API
        options.DefaultApiVersion = new ApiVersion(1, 0);

        // Assume a versão padrão quando não especificada
        options.AssumeDefaultVersionWhenUnspecified = true;

        // Define como a versão será lida (Header, Query String, URL Path, etc.)
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),           // /api/v1/controller
            new QueryStringApiVersionReader("version"), // ?version=1.0
            new HeaderApiVersionReader("X-Api-Version") // Header: X-Api-Version: 1.0
        );

        // Estratégia para reportar versões suportadas
        options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
        
        // Configuração para reportar versões suportadas
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        // Configuração para o ApiExplorer (usado pelo Swagger)
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    })
    .AddMvc();

    services.AddEndpointsApiExplorer();
}

static void ConfigurarAutenticacaoJwt(IServiceCollection services, JwtSettings configuracoesJwt)
{
    services.AddAuthentication(options =>
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuracoesJwt.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = configuracoesJwt.Issuer,
            ValidateAudience = true,
            ValidAudience = configuracoesJwt.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
        options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
    });
}

static void ConfigurarSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Antecipação API",
            Version = "v1",
            Description = "API para sistema de antecipação de recebíveis"
        });
        
        c.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Antecipação API",
            Version = "v2",
            Description = "API para sistema de antecipação de recebíveis - Versão 2"
        });

        // Configuração para resolver conflitos de rotas duplicadas
        c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

        // Configuração para incluir comentários XML (opcional)
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }

        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
    });
}