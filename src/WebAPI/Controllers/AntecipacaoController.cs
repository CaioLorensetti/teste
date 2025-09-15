using Antecipacao.Domain.DTOs;
using Antecipacao.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<SolicitacaoResponseDto>> CriarSolicitacao(
            [FromBody] CriarSolicitacaoDto dto)
        {
            try
            {
                var result = await _service.CriarSolicitacaoAsync(dto);
                return CreatedAtAction(nameof(ObterSolicitacao), new { guidId = result.GuidId }, result);
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

        [HttpGet("creator/{creatorId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<SolicitacaoResponseDto>>> ListarPorCreator(
            long creatorId)
        {
            var solicitacoes = await _service.ListarPorCreatorAsync(creatorId);
            return Ok(solicitacoes);
        }

        [HttpPut("{guidId}/aprovar")]
        [Authorize]
        public async Task<ActionResult<SolicitacaoResponseDto>> AprovarSolicitacao(Guid guidId)
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
        public async Task<ActionResult<SolicitacaoResponseDto>> RecusarSolicitacao(Guid guidId)
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
        public async Task<ActionResult<SolicitacaoResponseDto>> ObterSolicitacao(Guid guidId)
        {
            // Implementar se necessário
            return NotFound();
        }
    }
}
