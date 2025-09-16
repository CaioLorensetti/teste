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
        private readonly IAntecipacaoService _service;

        public AntecipacaoController(IAntecipacaoService service)
        {
            _service = service;
        }

        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
            {
                throw new UnauthorizedAccessException("Token inválido ou usuário não identificado.");
            }
            return userId;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<MinhaSolicitacaoResponseDto>> CriarSolicitacao(
            [FromBody] CriarSolicitacaoDto dto)
        {
            try
            {
                var creatorId = GetCurrentUserId();
                dto.CreatorId = creatorId; // Definir o CreatorId do token
                var result = await _service.CriarSolicitacaoAsync(dto);
                return CreatedAtAction(nameof(ObterSolicitacao), new { guidId = result.GuidId }, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpGet("minhas-solicitacoes")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MinhaSolicitacaoResponseDto>>> ListarMinhasSolicitacoes()
        {
            try
            {
                var creatorId = GetCurrentUserId();
                var solicitacoes = await _service.ListarPorCreatorAsync(creatorId);
                return Ok(solicitacoes);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        [HttpPut("{guidId}/aprovar")]
        [Authorize]
        public async Task<ActionResult<MinhaSolicitacaoResponseDto>> AprovarSolicitacao(Guid guidId)
        {
            try
            {
                var result = await _service.AprovarAsync(guidId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPut("{guidId}/recusar")]
        [Authorize]
        public async Task<ActionResult<MinhaSolicitacaoResponseDto>> RecusarSolicitacao(Guid guidId)
        {
            try
            {
                var result = await _service.RecusarAsync(guidId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("simular")]
        [Authorize]
        public async Task<ActionResult<SimulacaoDto>> SimularAntecipacao([FromQuery] decimal valor)
        {
            try
            {
                var result = await _service.SimularAntecipacaoAsync(valor);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{guidId}")]
        [Authorize]
        public async Task<ActionResult<MinhaSolicitacaoResponseDto>> ObterSolicitacao(Guid guidId)
        {
            // Implementar se necessário
            return NotFound();
        }
    }
}
