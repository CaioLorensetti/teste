#!/bin/bash

echo "🔨 Buildando solução..."

# Restaurar dependências
dotnet restore src/Antecipacao.sln

# Build da solução
dotnet build src/Antecipacao.sln

if [ $? -eq 0 ]; then
    echo "✅ Build realizado com sucesso!"
    
    echo "🧪 Executando testes..."
    dotnet test src/Antecipacao.sln --verbosity normal
    
    if [ $? -eq 0 ]; then
        echo "✅ Todos os testes passaram!"
    else
        echo "❌ Alguns testes falharam!"
        exit 1
    fi
else
    echo "❌ Build falhou!"
    exit 1
fi
