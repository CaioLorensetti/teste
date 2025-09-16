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
        [Authorize]
        public async Task<ActionResult<MinhaSolicitacaoResponseDto>> CriarSolicitacao(
            [FromBody] CriarSolicitacaoDto dto)
        {
            try
            {
                var idUsuario = ObterIdUsuarioAtual();
                dto.IdCriador = idUsuario;
                
                var solicitacaoCriada = await _servicoAntecipacao.CriarSolicitacaoAsync(dto);
                return CreatedAtAction(nameof(ObterSolicitacao), new { guidId = solicitacaoCriada.GuidId }, solicitacaoCriada);
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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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

        [HttpGet("{guidId}")]
        [Authorize]
        public async Task<ActionResult<MinhaSolicitacaoResponseDto>> ObterSolicitacao(Guid guidId)
        {
            return NotFound();
        }
    }
}
