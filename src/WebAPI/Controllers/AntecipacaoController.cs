using Antecipacao.Domain.DTOs;
using Antecipacao.Domain.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Antecipacao.WebAPI.Controllers
{
    [ApiController]
    [ApiVersion("1.0", Deprecated = true)]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
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
        [MapToApiVersion("1.0")]
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
        [MapToApiVersion("1.0")]
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
        [MapToApiVersion("1.0")]
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
        [MapToApiVersion("1.0")]
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

        [HttpGet("admin/solicitacoes-usuario/{userId}")]
        [Authorize(Policy = "AdminOnly")]
        [MapToApiVersion("1.0")]
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

        /// <summary>
        /// Simula uma antecipação de recebíveis
        /// </summary>
        /// <param name="valor">Valor para simulação</param>
        /// <returns>Resultado da simulação</returns>
        [HttpGet("simular")]
        [AllowAnonymous]
        [MapToApiVersion("1.0")]
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

        /// <summary>
        /// Simula uma antecipação de recebíveis com informações adicionais
        /// </summary>
        /// <param name="valor">Valor para simulação</param>
        /// <returns>Resultado da simulação com informações estendidas</returns>
        [HttpGet("simular", Name = "SimularV2")]
        [AllowAnonymous]
        [MapToApiVersion("2.0")]
        public async Task<ActionResult<object>> SimularAntecipacaoV2([FromQuery] decimal valor)
        {
            try
            {
                var simulacao = await _servicoAntecipacao.SimularAntecipacaoAsync(valor);

                var simulacaoV2 = new
                {
                    simulacao.ValorSolicitado,
                    simulacao.ValorLiquido,
                    simulacao.TaxaAplicada,
                    DataSimulacao = DateTime.UtcNow,
                    Avisos = new[]
                    {
                        "Analise o impacto no fluxo de caixa antes de prosseguir",
                        "Simulação válida por 24 horas"
                    }
                };

                return Ok(simulacaoV2);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}
