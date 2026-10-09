using Microsoft.AspNetCore.Mvc;
using MonitoramentoGestante.Server.Models;
using MonitoramentoGestante.Server.Services;

// Eu sou o cara que atende o POST /api/atendimentos do botão Enviar do "Finalizar Atendimento"
// entrego o formulário ao FinalizarAtendimentoService e respondo 200 ok (gravou), 400 (Faltou dados, com a lista de erros do que faltou)
// 500 se o banco falhou.
// se o banco falhar, registro só o TIPO de erro.
namespace MonitoramentoGestante.Server.Controllers
{
    [ApiController]
    [Route("api/atendimentos")]
    public class AtendimentosController : ControllerBase
    {
        private readonly IFinalizarAtendimentoService _service;
        private readonly ILogger<AtendimentosController> _logger;
        public AtendimentosController(IFinalizarAtendimentoService service, ILogger<AtendimentosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost]
        public ActionResult<ResultadoGravacao> Finalizar([FromBody] FinalizarAtendimentoEntrada entrada)
        {
            try
            {
                var resultado = _service.Finalizar(entrada);
                if (!resultado.Sucesso) return BadRequest(resultado);
                return Ok(resultado);
            }
            catch (Exception erro)
            {
                _logger.LogError("Falha ao finalizar atendimento: {Tipo}", erro.GetBaseException().GetType().Name);
                var falha = new ResultadoGravacao();
                falha.Erros.Add("Não foi possivel gravar o atendimento no banco. Tente de novo; se continuar, avise o suporte.");
                return StatusCode(500, falha);

            }
        }
    } 
}
