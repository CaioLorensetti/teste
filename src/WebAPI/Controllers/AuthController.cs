using Antecipacao.Domain.DTOs;
using Antecipacao.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Antecipacao.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _servicoAutenticacao;

        public AuthController(IAuthenticationService servicoAutenticacao)
        {
            _servicoAutenticacao = servicoAutenticacao;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var resposta = await _servicoAutenticacao.AuthenticateAsync(request, ObterEnderecoIp());

                if (resposta == null)
                    return BadRequest(new { erro = "Usuário ou senha incorretos" });

                DefinirCookieToken(resposta.RefreshToken);

                return Ok(new
                {
                    resposta.Username,
                    resposta.Role,
                    resposta.AccessToken
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                await _servicoAutenticacao.RegisterAsync(request);
                return Ok(new { mensagem = "Registro realizado com sucesso" });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var tokenRefresh = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(tokenRefresh))
                return BadRequest(new { erro = "Token de refresh é obrigatório" });

            try
            {
                var resposta = await _servicoAutenticacao.RefreshTokenAsync(tokenRefresh, ObterEnderecoIp());
                DefinirCookieToken(resposta.RefreshToken);

                return Ok(new
                {
                    resposta.Username,
                    resposta.Role,
                    resposta.AccessToken
                });
            }
            catch (System.Security.SecurityException ex)
            {
                return Unauthorized(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var tokenRefresh = Request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(tokenRefresh))
            {
                await _servicoAutenticacao.RevokeTokenAsync(tokenRefresh, ObterEnderecoIp());
            }

            Response.Cookies.Delete("refreshToken");

            return Ok(new { mensagem = "Logout realizado com sucesso" });
        }

        private void DefinirCookieToken(string token)
        {
            var opcoesCookie = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                SameSite = SameSiteMode.Strict,
                Secure = true
            };

            Response.Cookies.Append("refreshToken", token, opcoesCookie);
        }

        private string ObterEnderecoIp()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"].ToString();
            else
                return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Desconhecido";
        }
    }
}
