#!/bin/bash

echo "🚀 Criando solução Antecipacao..."

# Criar solução
dotnet new sln -n Antecipacao -o src

# Criar projetos
echo "📁 Criando projetos..."
dotnet new classlib -n Antecipacao.Domain -o src/Domain
dotnet new classlib -n Antecipacao.Application -o src/Application
dotnet new classlib -n Antecipacao.Infrastructure -o src/Infrastructure
dotnet new webapi -n Antecipacao.WebAPI -o src/WebAPI
dotnet new xunit -n Antecipacao.Tests -o src/Tests

# Adicionar projetos à solução
echo "🔗 Adicionando projetos à solução..."
dotnet sln add src/Domain/Antecipacao.Domain.csproj
dotnet sln add src/Application/Antecipacao.Application.csproj
dotnet sln add src/Infrastructure/Antecipacao.Infrastructure.csproj
dotnet sln add src/WebAPI/Antecipacao.WebAPI.csproj
dotnet sln add src/Tests/Antecipacao.Tests.csproj

# Configurar referências entre projetos
echo "🔗 Configurando referências..."
dotnet add src/Application/Antecipacao.Application.csproj reference src/Domain/Antecipacao.Domain.csproj
dotnet add src/Infrastructure/Antecipacao.Infrastructure.csproj reference src/Domain/Antecipacao.Domain.csproj
dotnet add src/Infrastructure/Antecipacao.Infrastructure.csproj reference src/Application/Antecipacao.Application.csproj
dotnet add src/WebAPI/Antecipacao.WebAPI.csproj reference src/Application/Antecipacao.Application.csproj
dotnet add src/WebAPI/Antecipacao.WebAPI.csproj reference src/Infrastructure/Antecipacao.Infrastructure.csproj
dotnet add src/Tests/Antecipacao.Tests.csproj reference src/WebAPI/Antecipacao.WebAPI.csproj
dotnet add src/Tests/Antecipacao.Tests.csproj reference src/Application/Antecipacao.Application.csproj
dotnet add src/Tests/Antecipacao.Tests.csproj reference src/Infrastructure/Antecipacao.Infrastructure.csproj

echo "✅ Solução criada com sucesso!"
echo "📂 Estrutura de pastas:"
tree -I 'bin|obj|*.user' -a
