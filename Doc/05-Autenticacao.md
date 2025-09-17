# Autenticação e Autorização

## JWT (JSON Web Tokens) com Refresh Tokens
**📋 Motivos da Escolha:**
-  Stateless e Escalável - Não precisa armazenar sessões no servidor, facilitando escalabilidade horizontal
-  Segurança Robusta - Tokens assinados digitalmente, impossíveis de alterar sem invalidar
-  Padrão da Indústria - Amplamente adotado, bem documentado e suportado
-  Flexível - Funciona bem com SPAs, mobile apps e APIs
-  Performance - Validação local sem consultas ao banco a cada requisição
-  Refresh Token - Permite renovação segura sem reautenticação constante

## 🔒 Estratégia de Segurança:
- Access Token: Curta duração (15 minutos)
- Refresh Token: Longa duração (7 dias), armazenado em HttpOnly Cookie
- Senha: Hash com BCrypt
- HTTPS: Obrigatório em produção

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.3" />
```

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Registrar Usuário:

- **POST** `/api/auth/register`
- **Body**:
```json
{
  "username": "mario@mbrothers.com",
  "fullname": "Mario Brother",
  "password": "SenhaForte123!"
}
```
- **Response 201**:
```json
{
  "username": "mario@mbrothers.com",
  "fullname": "Mario Brother",
}
```
## Login Usuário:

- **POST** `/api/auth/login`
- **Body**:
```json
{
  "username": "mario@mbrothers.com",
  "password": "SenhaForte123!"
}
```

- **Response 200**:
```json
{
  "accessToken": "token",
  "refreshToken": "token"
}
```

## 🔐 Recursos de Segurança Implementados:
-  Senha com BCrypt - Hash seguro e salt automático
-  Access Token de curta duração - Minimiza janela de ataque
-  Refresh Token em HttpOnly Cookie - Protege contra XSS
-  Rotação de Refresh Token - Novo token a cada uso
-  Detecção de reuso de token - Revoga toda a cadeia em caso de comprometimento
-  HTTPS obrigatório - Protege contra man-in-the-middle
-  Validação de IP - Rastreia origem dos tokens
-  Limpeza automática - Remove tokens expirados antigos

##🎯 Vantagens desta Implementação:
-  Segurança em camadas - Múltiplas proteções contra diferentes ataques
-  Escalável - Pronta para microserviços e load balancers
-  Manutenível - Código limpo e bem organizado
-  Compatível - Funciona com SPAs modernas (React, Angular, Vue)
-  Auditável - Rastreamento completo de tokens e acessos