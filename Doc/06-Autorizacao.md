# 06 - Autorização por Role

## Visão Geral

Este documento descreve a implementação do sistema de autorização baseado em roles (funções) para a API de Antecipação. O sistema utiliza JWT tokens que contêm informações de role do usuário para controlar o acesso aos endpoints da API.

## Estrutura de Roles

O sistema possui duas roles principais:

- **Admin**: 
  - Pode aprovar e recusar solicitações
  - Pode listar solicitações de qualquer usuário User
  - **NÃO pode** criar solicitações para si mesmo
  - **NÃO pode** criar solicitações
- **User**: 
  - Pode criar solicitações
  - Pode listar suas próprias solicitações
  - Pode consultar o status de suas solicitações

## Implementação

### 1. Entidade User

A entidade `User` já possui a propriedade `Role` do tipo `UserRole`:

```csharp
public enum UserRole
{
    Admin = 0,
    User = 1
}

public class User
{
    // ... outras propriedades
    public UserRole Role { get; set; }
    // ... outras propriedades
}
```

### 2. JWT Token com Role

O JWT token é gerado com a role do usuário incluída nas claims:

```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Email, user.Username),
    new Claim(ClaimTypes.Role, user.Role.ToString())
};
```

### 3. Políticas de Autorização

Foram criadas três políticas de autorização no `Program.cs`:

```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
});
```

### 4. Aplicação de Autorização nos Controllers

#### AntecipacaoController

- **Endpoints Admin Only** (apenas Admin):
  - `PUT /api/v1/antecipacao/{guidId}/aprovar` - Aprovar solicitação
  - `PUT /api/v1/antecipacao/{guidId}/recusar` - Recusar solicitação
  - `GET /api/v1/antecipacao/admin/solicitacoes-usuario/{userId}` - Listar solicitações de usuário específico

- **Endpoints User Only** (apenas User):
  - `POST /api/v1/antecipacao` - Criar solicitação
  - `GET /api/v1/antecipacao/minhas-solicitacoes` - Listar minhas solicitações
  - `GET /api/v1/antecipacao/{guidId}` - Obter solicitação específica

- **Endpoints Públicos** (sem autenticação):
  - `GET /api/v1/antecipacao/simular` - Simular antecipação

#### AuthController

- **Endpoints Públicos** (sem autorização):
  - `POST /api/auth/login` - Login
  - `POST /api/auth/register` - Registro (apenas para role User)
  - `POST /api/auth/refresh-token` - Renovar token
  - `POST /api/auth/logout` - Logout

### 5. Usuário Admin Padrão

Foi implementado um sistema de seed data que cria automaticamente um usuário admin padrão:

- **Username**: `admin@antecipacao.com`
- **Password**: `Admin@123`
- **Role**: `Admin`

O usuário admin é criado automaticamente na inicialização da aplicação se não existir.

### 6. Regras de Negócio por Role

#### Regras para Role Admin:
- ✅ Pode aprovar solicitações
- ✅ Pode recusar solicitações
- ✅ Pode listar solicitações de qualquer usuário User
- ❌ **NÃO pode** criar solicitações para si mesmo
- ❌ **NÃO pode** criar solicitações

#### Regras para Role User:
- ✅ Pode criar solicitações
- ✅ Pode listar suas próprias solicitações
- ✅ Pode consultar o status de suas solicitações
- ❌ **NÃO pode** aprovar ou recusar solicitações
- ❌ **NÃO pode** listar solicitações de outros usuários

#### Endpoints Públicos:
- ✅ Registro de usuários (apenas role User)
- ✅ Login
- ✅ Logout
- ✅ Simulação de antecipação

## Fluxo de Autorização

1. **Login**: Usuário faz login e recebe um JWT token com sua role
2. **Requisição**: Cliente envia requisição com o JWT token no header `Authorization: Bearer {token}`
3. **Validação**: Middleware JWT valida o token e extrai as claims, incluindo a role
4. **Autorização**: ASP.NET Core verifica se a role do usuário atende aos requisitos do endpoint
5. **Resposta**: Se autorizado, processa a requisição; caso contrário, retorna 403 Forbidden

## Exemplos de Uso

### Login como Admin

```http
POST /api/auth/login
Content-Type: application/json

{
    "username": "admin@antecipacao.com",
    "password": "Admin@123"
}
```

**Resposta:**
```json
{
    "username": "admin@antecipacao.com",
    "role": "Admin",
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### Aprovar Solicitação (Admin Only)

```http
PUT /api/v1/antecipacao/{guidId}/aprovar
Authorization: Bearer {admin_token}
```

### Listar Solicitações de Usuário (Admin Only)

```http
GET /api/v1/antecipacao/admin/solicitacoes-usuario/{userId}
Authorization: Bearer {admin_token}
```

### Criar Solicitação (User Only)

```http
POST /api/v1/antecipacao
Authorization: Bearer {user_token}
Content-Type: application/json

{
    "valorSolicitado": 1000.00,
    "dataVencimento": "2024-12-31"
}
```

### Simular Antecipação (Público)

```http
GET /api/v1/antecipacao/simular?valor=1000.00
```

### Registrar Usuário (Público - apenas User)

```http
POST /api/auth/register
Content-Type: application/json

{
    "username": "usuario@exemplo.com",
    "password": "MinhaSenh@123",
    "role": "User"
}
```

## Segurança

- **JWT Tokens**: Tokens são assinados e validados para garantir autenticidade
- **HTTPS**: Todos os endpoints requerem HTTPS em produção
- **Role Validation**: Cada endpoint valida a role do usuário antes de processar
- **Token Expiration**: Tokens têm tempo de expiração configurável
- **Refresh Tokens**: Sistema de renovação de tokens para melhor segurança

## Configuração

As configurações de JWT estão no `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "sua-chave-secreta-super-segura",
    "Issuer": "AntecipacaoAPI",
    "Audience": "AntecipacaoClient",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

## Monitoramento

Para monitorar tentativas de acesso não autorizado, verifique os logs da aplicação. Tentativas de acesso com roles insuficientes retornarão status HTTP 403 Forbidden.

## Considerações de Desenvolvimento

- O sistema de roles é extensível - novas roles podem ser adicionadas facilmente
- A validação de roles é feita no nível do controller, não no serviço
- O usuário admin padrão é criado apenas se não existir outro usuário admin
- Tokens são invalidados no logout para maior segurança
