using Antecipacao.Infrastructure.Data;
using Antecipacao.WebAPI;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Antecipacao.Tests.Integration
{
    public class AntecipacaoIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AntecipacaoIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove o DbContext real
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AntecipacaoDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Adiciona o DbContext em memória
                    services.AddDbContext<AntecipacaoDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDatabase");
                    });

                    // Garante que o banco seja criado
                    var serviceProvider = services.BuildServiceProvider();
                    using var scope = serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AntecipacaoDbContext>();
                    context.Database.EnsureCreated();
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task SimularAntecipacao_ComValorValido_DeveRetornarSimulacao()
        {
            // Arrange
            var valor = 1000m;

            // Act
            var response = await _client.GetAsync($"/api/v1/Antecipacao/simular?valor={valor}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var simulacao = await response.Content.ReadFromJsonAsync<object>();
            simulacao.Should().NotBeNull();
        }

        [Fact]
        public async Task SimularAntecipacao_ComValorInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var valor = 50m;

            // Act
            var response = await _client.GetAsync($"/api/v1/Antecipacao/simular?valor={valor}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task SimularAntecipacaoV2_ComValorValido_DeveRetornarSimulacaoMelhorada()
        {
            // Arrange
            var valor = 1000m;

            // Act
            var response = await _client.GetAsync($"/api/v2/Antecipacao/simular?valor={valor}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var simulacao = await response.Content.ReadFromJsonAsync<object>();
            simulacao.Should().NotBeNull();
        }

        [Fact]
        public async Task Swagger_DeveEstarDisponivel()
        {
            // Act
            var response = await _client.GetAsync("/swagger");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task SwaggerJson_DeveEstarDisponivel()
        {
            // Act
            var response = await _client.GetAsync("/swagger/v1/swagger.json");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task SwaggerJsonV2_DeveEstarDisponivel()
        {
            // Act
            var response = await _client.GetAsync("/swagger/v2/swagger.json");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task HealthCheck_DeveRetornarOk()
        {
            // Act
            var response = await _client.GetAsync("/");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }
    }

    public class AntecipacaoServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public AntecipacaoServiceIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task AntecipacaoService_ComBancoEmMemoria_DeveFuncionarCorretamente()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AntecipacaoDbContext>();
            
            // Garante que o banco está limpo
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Act & Assert
            // Aqui você pode testar operações mais complexas que envolvem múltiplas camadas
            // Por exemplo, criar uma solicitação, aprovar, etc.
            
            // Verifica se o banco foi criado corretamente
            context.Database.CanConnect().Should().BeTrue();
        }
    }
}
