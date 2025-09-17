using Antecipacao.Domain.DTOs;
using Antecipacao.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Antecipacao.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AntecipacaoController : ControllerBase
    {
        private readonly IAntecipacaoService _servicoAntecipacao;

        public AntecipacaoController(IAntecipacaoService servicoAntecipacao)
        {
            _servicoAntecipacao = servicoAntecipacao;
        }

        private long ObterIdUsuarioAtual()
        {
            var claimIdUsuario = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claimIdUsuario == null || !long.TryParse(claimIdUsuario.Value, out long idUsuario))
            {
                throw new UnauthorizedAccessException("Token inválido ou usuário não identificado.");
            }
            return idUsuario;
        }

        [HttpPost]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<MinhaSolicitacaoResponseDto>> CriarSolicitacao(
            [FromBody] CriarSolicitacaoRequest request)
        {
            try
            {
                var idUsuario = ObterIdUsuarioAtual();

                var solicitacaoCriada = await _servicoAntecipacao.CriarSolicitacaoAsync(idUsuario, request);
                return CreatedAtAction(nameof(CriarSolicitacao), new { guidId = solicitacaoCriada.GuidId }, solicitacaoCriada);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { erro = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { erro = ex.Message });
            }
        }

        [HttpGet("minhas-solicitacoes")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<IEnumerable<MinhaSolicitacaoResponseDto>>> ListarMinhasSolicitacoes()
        {
            try
            {
                var idUsuario = ObterIdUsuarioAtual();
                var solicitacoes = await _servicoAntecipacao.ListarPorCreatorAsync(idUsuario);
                return Ok(solicitacoes);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { erro = ex.Message });
            }
        }

        [HttpPut("{guidId}/aprovar")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<MinhaSolicitacaoResponseDto>> AprovarSolicitacao(Guid guidId)
        {
            try
            {
                var solicitacaoAprovada = await _servicoAntecipacao.AprovarAsync(guidId);
                return Ok(solicitacaoAprovada);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
        }

        [HttpPut("{guidId}/recusar")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<MinhaSolicitacaoResponseDto>> RecusarSolicitacao(Guid guidId)
        {
            try
            {
                var solicitacaoRecusada = await _servicoAntecipacao.RecusarAsync(guidId);
                return Ok(solicitacaoRecusada);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
        }

        [HttpGet("simular")]
        [AllowAnonymous]
        public async Task<ActionResult<SimulacaoDto>> SimularAntecipacao([FromQuery] decimal valor)
        {
            try
            {
                var simulacao = await _servicoAntecipacao.SimularAntecipacaoAsync(valor);
                return Ok(simulacao);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpGet("admin/solicitacoes-usuario/{userId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IEnumerable<MinhaSolicitacaoResponseDto>>> ListarSolicitacoesPorUsuario(long userId)
        {
            try
            {
                var solicitacoes = await _servicoAntecipacao.ListarPorCreatorAsync(userId);
                return Ok(solicitacoes);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
        }
    }
}
