# 🚀 Antecipação API

Sistema de gestão de solicitações de antecipação de recebíveis desenvolvido em .NET 9 com arquitetura limpa, versionamento de API e autenticação JWT.

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [Tecnologias](#-tecnologias)
- [Arquitetura](#-arquitetura)
- [Pré-requisitos](#-pré-requisitos)
- [Instalação e Execução](#-instalação-e-execução)
- [Documentação da API](#-documentação-da-api)
- [Versionamento](#-versionamento)
- [Autenticação](#-autenticação)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Testes](#-testes)
- [Configurações](#-configurações)
- [Endpoints Disponíveis](#-endpoints-disponíveis)

## 🎯 Visão Geral

A **Antecipação API** é um sistema completo para gestão de solicitações de antecipação de recebíveis, permitindo que criadores solicitem a liberação antecipada de parte de seus valores futuros mediante uma taxa de 5%.

### Principais Funcionalidades

- ✅ **Criação de solicitações** de antecipação
- ✅ **Simulação** de valores sem compromisso
- ✅ **Aprovação/Recusa** de solicitações (admin)
- ✅ **Listagem** de solicitações por usuário
- ✅ **Autenticação JWT** com refresh tokens
- ✅ **Versionamento de API** (v1 e v2)
- ✅ **Documentação Swagger** interativa
- ✅ **Validações de negócio** robustas

## 🛠 Tecnologias

- **.NET 9.0** - Framework principal
- **ASP.NET Core Web API** - API REST
- **Entity Framework Core** - ORM
- **SQLite** - Banco de dados
- **JWT Bearer** - Autenticação
- **Swagger/OpenAPI** - Documentação
- **xUnit** - Testes unitários
- **FluentValidation** - Validações

## 🏗 Arquitetura

O projeto segue os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**:

```
📁 src/
├── 🎯 Domain/          # Entidades, DTOs, Interfaces, Regras de Negócio
├── 🔧 Application/     # Serviços, Mappings, Validators
├── 🗄️ Infrastructure/ # Repositórios, DbContext, Configurações
├── 🌐 WebAPI/         # Controllers, Middleware, Program.cs
└── 🧪 Tests/          # Testes unitários e de integração
```

## 📋 Pré-requisitos

Antes de começar, certifique-se de ter instalado:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Git](https://git-scm.com/)
- Um editor de código (VS Code, Visual Studio, Rider, etc.)

### Verificando a Instalação

```bash
# Verificar versão do .NET
dotnet --version

# Verificar versão do Git
git --version
```

## 🚀 Instalação e Execução

### 1. Clone o Repositório

```bash
git clone <url-do-repositorio>
cd teste
```

### 2. Restaure as Dependências

```bash
# Navegue para o diretório do projeto
cd src

# Restaure os pacotes NuGet
dotnet restore
```

### 3. Execute o Projeto

```bash
# Execute a aplicação
dotnet run --project WebAPI/Antecipacao.WebAPI.csproj
```

### 4. Acesse a Aplicação

- **API**: https://localhost:7282
- **Swagger UI**: https://localhost:7282/swagger
- **HTTP**: http://localhost:5016

## 📚 Documentação da API

### Swagger UI

Acesse `https://localhost:7282/swagger` para visualizar a documentação interativa da API.

### Versões Disponíveis

- **v1**: Versão estável com funcionalidades básicas
- **v2**: Versão com melhorias e funcionalidades avançadas

## 🔄 Versionamento

A API suporta versionamento através de:

- **URL Path**: `/api/v1/controller` ou `/api/v2/controller`
- **Query String**: `?version=1.0` ou `?version=2.0`
- **Header**: `X-Api-Version: 1.0` ou `X-Api-Version: 2.0`

### Exemplos de Uso

```bash
# Versão 1 (URL Path)
GET https://localhost:7282/api/v1/Antecipacao/simular?valor=1000

# Versão 2 (URL Path)
GET https://localhost:7282/api/v2/Antecipacao/simular?valor=1000

# Versão via Query String
GET https://localhost:7282/api/Antecipacao/simular?valor=1000&version=2.0

# Versão via Header
GET https://localhost:7282/api/Antecipacao/simular?valor=1000
Headers: X-Api-Version: 2.0
```

## 🔐 Autenticação

A API utiliza **JWT (JSON Web Tokens)** para autenticação.

### 1. Registro de Usuário

```bash
POST https://localhost:7282/api/v1/Auth/register
Content-Type: application/json

{
  "email": "usuario@exemplo.com",
  "password": "MinhaSenh@123",
  "confirmPassword": "MinhaSenh@123"
}
```

### 2. Login

```bash
POST https://localhost:7282/api/v1/Auth/login
Content-Type: application/json

{
  "email": "usuario@exemplo.com",
  "password": "MinhaSenh@123"
}
```

### 3. Usar Token

```bash
GET https://localhost:7282/api/v1/Antecipacao/minhas-solicitacoes
Authorization: Bearer <seu-token-jwt>
```

### Usuário Administrador Padrão

O sistema cria automaticamente um usuário administrador:

- **Email**: `admin@antecipacao.com`
- **Senha**: `Admin@123`
- **Role**: `Admin`

## 📁 Estrutura do Projeto

```
src/
├── Domain/                    # Camada de Domínio
│   ├── DTOs/                 # Data Transfer Objects
│   ├── Entities/             # Entidades do domínio
│   ├── Enums/                # Enumerações
│   ├── Exceptions/           # Exceções customizadas
│   ├── Interfaces/           # Contratos/Interfaces
│   └── ValueObjects/         # Objetos de valor
├── Application/              # Camada de Aplicação
│   ├── Mappings/             # Mapeamentos AutoMapper
│   ├── Services/             # Serviços de aplicação
│   └── Validators/           # Validadores FluentValidation
├── Infrastructure/           # Camada de Infraestrutura
│   ├── Configuration/        # Configurações de DI
│   ├── Data/                 # DbContext e Configurações
│   └── Repositories/         # Implementações dos repositórios
├── WebAPI/                   # Camada de Apresentação
│   ├── Controllers/          # Controllers da API
│   ├── Middleware/           # Middlewares customizados
│   └── Properties/           # Configurações de execução
└── Tests/                    # Testes
    └── UnitTest1.cs          # Testes unitários
```

## 🧪 Testes

### Executar Testes

```bash
# Execute todos os testes
dotnet test

# Execute com detalhes
dotnet test --verbosity normal

# Execute testes de um projeto específico
dotnet test Tests/Antecipacao.Tests.csproj
```

### Cobertura de Testes

```bash
# Instalar ferramenta de cobertura (se necessário)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Executar testes com cobertura
dotnet test --collect:"XPlat Code Coverage"
```

## ⚙️ Configurações

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=antecipacao.db"
  },
  "JwtSettings": {
    "SecretKey": "sua-chave-secreta",
    "Issuer": "AntecipacaoAPI",
    "Audience": "AntecipacaoAPIUsers",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Variáveis de Ambiente

```bash
# Definir ambiente
export ASPNETCORE_ENVIRONMENT=Development

# Ou no Windows
set ASPNETCORE_ENVIRONMENT=Development
```

## 🌐 Endpoints Disponíveis

### Autenticação

| Método | Endpoint | Descrição | Versão |
|--------|----------|-----------|--------|
| POST | `/api/v1/Auth/register` | Registrar usuário | v1 |
| POST | `/api/v1/Auth/login` | Fazer login | v1 |
| POST | `/api/v1/Auth/refresh` | Renovar token | v1 |

### Antecipação

| Método | Endpoint | Descrição | Versão | Auth |
|--------|----------|-----------|--------|------|
| GET | `/api/v1/Antecipacao/simular` | Simular antecipação | v1 | ❌ |
| GET | `/api/v2/Antecipacao/simular` | Simular antecipação (melhorado) | v2 | ❌ |
| POST | `/api/v1/Antecipacao` | Criar solicitação | v1 | ✅ |
| GET | `/api/v1/Antecipacao/minhas-solicitacoes` | Listar minhas solicitações | v1 | ✅ |
| PUT | `/api/v1/Antecipacao/{id}/aprovar` | Aprovar solicitação | v1 | Admin |
| PUT | `/api/v1/Antecipacao/{id}/recusar` | Recusar solicitação | v1 | Admin |

### Exemplos de Uso

#### 1. Simular Antecipação (v1)

```bash
curl -X GET "https://localhost:7282/api/v1/Antecipacao/simular?valor=1000" \
  -H "accept: application/json"
```

**Resposta:**
```json
{
  "valorSolicitado": 1000.00,
  "taxaAplicada": 50.00,
  "valorLiquido": 950.00
}
```

#### 2. Simular Antecipação (v2)

```bash
curl -X GET "https://localhost:7282/api/v2/Antecipacao/simular?valor=1000" \
  -H "accept: application/json"
```

**Resposta:**
```json
{
  "valorSolicitado": 1000.00,
  "valorLiquido": 950.00,
  "taxaAplicada": 50.00,
  "dataSimulacao": "2024-01-15T10:30:00Z",
  "avisos": [
    "Analise o impacto no fluxo de caixa antes de prosseguir",
    "Simulação válida por 24 horas"
  ]
}
```

#### 3. Criar Solicitação

```bash
curl -X POST "https://localhost:7282/api/v1/Antecipacao" \
  -H "accept: application/json" \
  -H "Authorization: Bearer <seu-token>" \
  -H "Content-Type: application/json" \
  -d '{
    "valorSolicitado": 1000.00
  }'
```

## 🔧 Desenvolvimento

### Scripts Úteis

```bash
# Limpar e reconstruir
dotnet clean && dotnet build

# Executar com hot reload
dotnet watch run --project WebAPI/Antecipacao.WebAPI.csproj

# Adicionar nova migração
dotnet ef migrations add NomeDaMigracao --project Infrastructure --startup-project WebAPI

# Aplicar migrações
dotnet ef database update --project Infrastructure --startup-project WebAPI
```

### Debugging

1. Abra o projeto no seu editor preferido
2. Configure breakpoints
3. Execute em modo Debug (`F5` no Visual Studio)
4. Use o Swagger UI para testar endpoints

## 📝 Regras de Negócio

- ✅ Valor mínimo: R$ 100,00
- ✅ Taxa fixa: 5% sobre o valor solicitado
- ✅ Um usuário pode ter apenas uma solicitação pendente
- ✅ Status inicial: "Pendente"
- ✅ Apenas administradores podem aprovar/recusar

## 🐛 Troubleshooting

### Problemas Comuns

1. **Erro de conexão com banco**
   - Verifique se o arquivo `antecipacao.db` existe
   - Execute `dotnet ef database update`

2. **Erro de autenticação JWT**
   - Verifique se o token está sendo enviado corretamente
   - Confirme se o token não expirou

3. **Erro de CORS**
   - Verifique as configurações de CORS no `Program.cs`
   - Confirme se a origem está permitida

### Logs

Os logs são configurados para mostrar informações detalhadas em modo Development. Verifique o console para mensagens de erro.

## 🤝 Contribuição

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## 📞 Suporte

Para dúvidas ou suporte, entre em contato:

- **Email**: dev@antecipacao.com
- **Issues**: [GitHub Issues](https://github.com/seu-usuario/antecipacao-api/issues)

---

**Desenvolvido com ❤️ usando .NET 9 e Clean Architecture**