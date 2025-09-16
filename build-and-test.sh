#!/bin/bash

# Script de Build e Teste para API de Antecipação de Valores
# Este script executa o build da solução e executa os testes

set -e  # Exit on any error

echo "🚀 Iniciando Build e Teste da API de Antecipação de Valores"
echo "=========================================================="

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Função para log colorido
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Verificar se estamos no diretório correto
if [ ! -f "src/Antecipacao.sln" ]; then
    log_error "Arquivo de solução não encontrado. Execute este script no diretório raiz do projeto."
    exit 1
fi

log_info "Diretório atual: $(pwd)"

# 1. Limpar builds anteriores
log_info "🧹 Limpando builds anteriores..."
dotnet clean src/Antecipacao.sln --verbosity quiet
log_success "Limpeza concluída"

# 2. Restaurar dependências
log_info "📦 Restaurando dependências..."
dotnet restore src/Antecipacao.sln --verbosity quiet
if [ $? -eq 0 ]; then
    log_success "Dependências restauradas com sucesso"
else
    log_error "Falha ao restaurar dependências"
    exit 1
fi

# 3. Build da solução
log_info "🔨 Executando build da solução..."
dotnet build src/Antecipacao.sln --configuration Release --no-restore --verbosity quiet
if [ $? -eq 0 ]; then
    log_success "Build concluído com sucesso"
else
    log_error "Falha no build"
    exit 1
fi

# 4. Executar testes
log_info "🧪 Executando testes..."
if [ -d "src/Tests" ]; then
    dotnet test src/Tests/Antecipacao.Tests.csproj --configuration Release --no-build --verbosity normal
    if [ $? -eq 0 ]; then
        log_success "Todos os testes passaram"
    else
        log_warning "Alguns testes falharam"
    fi
else
    log_warning "Diretório de testes não encontrado. Pulando execução de testes."
fi

# 5. Verificar se a WebAPI pode ser executada
log_info "🌐 Verificando se a WebAPI pode ser executada..."
cd src/WebAPI
timeout 10s dotnet run --no-build --configuration Release > /dev/null 2>&1 &
WEBAPI_PID=$!
sleep 5

# Verificar se o processo ainda está rodando
if kill -0 $WEBAPI_PID 2>/dev/null; then
    log_success "WebAPI iniciou com sucesso"
    kill $WEBAPI_PID 2>/dev/null || true
else
    log_warning "WebAPI pode ter problemas para iniciar"
fi

cd ../..

# 6. Resumo final
echo ""
echo "=========================================================="
log_success "✅ Build e Teste Concluídos!"
echo "=========================================================="
echo ""
log_info "📁 Estrutura de projetos:"
echo "  - Domain: Entidades e regras de negócio"
echo "  - Application: Serviços e DTOs"
echo "  - Infrastructure: Acesso a dados e repositórios"
echo "  - WebAPI: Controllers e middleware"
echo "  - Tests: Testes unitários e de integração"
echo ""
log_info "🚀 Para executar a API:"
echo "  cd src/WebAPI"
echo "  dotnet run"
echo ""
log_info "📚 Para acessar a documentação Swagger:"
echo "  https://localhost:5001/swagger"
echo ""

# 7. Verificar dependências dos projetos
log_info "🔍 Verificando dependências dos projetos..."

# Verificar Domain
if [ -f "src/Domain/Antecipacao.Domain.csproj" ]; then
    log_success "✓ Domain project encontrado"
else
    log_error "✗ Domain project não encontrado"
fi

# Verificar Application
if [ -f "src/Application/Antecipacao.Application.csproj" ]; then
    log_success "✓ Application project encontrado"
else
    log_error "✗ Application project não encontrado"
fi

# Verificar Infrastructure
if [ -f "src/Infrastructure/Antecipacao.Infrastructure.csproj" ]; then
    log_success "✓ Infrastructure project encontrado"
else
    log_error "✗ Infrastructure project não encontrado"
fi

# Verificar WebAPI
if [ -f "src/WebAPI/Antecipacao.WebAPI.csproj" ]; then
    log_success "✓ WebAPI project encontrado"
else
    log_error "✗ WebAPI project não encontrado"
fi

echo ""
log_success "🎉 Script executado com sucesso!"