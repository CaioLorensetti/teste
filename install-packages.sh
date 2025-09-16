#!/bin/bash

echo "📦 Instalando pacotes NuGet..."

# Application Layer
echo "Instalando pacotes para Application..."
dotnet add src/Application/Antecipacao.Application.csproj package FluentValidation --version 11.8.1
dotnet add src/Application/Antecipacao.Application.csproj package MediatR --version 12.2.0
dotnet add src/Application/Antecipacao.Application.csproj package Microsoft.Extensions.DependencyInjection.Abstractions --version 8.0.0

# Infrastructure Layer
echo "Instalando pacotes para Infrastructure..."
dotnet add src/Infrastructure/Antecipacao.Infrastructure.csproj package Microsoft.EntityFrameworkCore --version 8.0.0
dotnet add src/Infrastructure/Antecipacao.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.0
dotnet add src/Infrastructure/Antecipacao.Infrastructure.csproj package Microsoft.EntityFrameworkCore.InMemory --version 8.0.0
dotnet add src/Infrastructure/Antecipacao.Infrastructure.csproj package Microsoft.Extensions.Configuration.Abstractions --version 8.0.0
dotnet add src/Infrastructure/Antecipacao.Infrastructure.csproj package BCrypt.Net-Next --version 4.0.3
dotnet add src/Infrastructure/Antecipacao.Infrastructure.csproj package System.IdentityModel.Tokens.Jwt --version 7.0.3

# WebAPI Layer
echo "Instalando pacotes para WebAPI..."
dotnet add src/WebAPI/Antecipacao.WebAPI.csproj package Microsoft.AspNetCore.OpenApi --version 8.0.0
dotnet add src/WebAPI/Antecipacao.WebAPI.csproj package Swashbuckle.AspNetCore --version 6.5.0
dotnet add src/WebAPI/Antecipacao.WebAPI.csproj package Microsoft.EntityFrameworkCore.Design --version 8.0.0
dotnet add src/WebAPI/Antecipacao.WebAPI.csproj package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0

# Tests
echo "Instalando pacotes para Tests..."
dotnet add src/Tests/Antecipacao.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing --version 8.0.0
dotnet add src/Tests/Antecipacao.Tests.csproj package FluentAssertions --version 6.12.0
dotnet add src/Tests/Antecipacao.Tests.csproj package Moq --version 4.20.69
dotnet add src/Tests/Antecipacao.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory --version 8.0.0

echo "✅ Pacotes instalados com sucesso!"
