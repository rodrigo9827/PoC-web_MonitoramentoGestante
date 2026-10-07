using Microsoft.AspNetCore.Mvc;
using MonitoramentoGestante.Server.Models;
using MonitoramentoGestante.Server.Services;

namespace MonitoramentoGestante.Server.Controllers
{
    // Eu sou o cara que atende o endereço /api/buscas-ativas.
    // Recebo o formulário da tela, entrego para o BuscaAtivaService e devolvo a reposta:
    // 200 se gravou, 400 se faltou algum dado, 500 se o banco falhou.
    // eu não confiro regra nenhuma e não falo com o banco.
    [ApiController]
    [Route("api/buscas-ativas")]
    public class BuscasAtivasController : ControllerBase
    {
        private readonly IBuscaAtivaService _service;
        private readonly ILogger<BuscasAtivasController> _logger;

        public BuscasAtivasController(IBuscaAtivaService service, ILogger<BuscasAtivasController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // POST /api/buscas-ativas
        [HttpPost]
        public ActionResult<ResultadoGravacao> Registrar([FromBody] BuscaAtivaEntrada entrada)
        {
            try
            {
                var resultado = _service.Registrar(entrada);
                if (!resultado.Sucesso) return BadRequest(resultado);
                return Ok(resultado);
            }
            catch (Exception erro)
            {
                // No registro vai só o Tipo do erro porque a mensagem do banco pode conter dados da paciente
                _logger.LogError("Falha ao gravar a Busca Ativa: {Tipo}", erro.GetBaseException().GetType().Name);

                return StatusCode(500, new ResultadoGravacao
                {
                    Erros = ["Não foi possível gravar no banco de dados. Tente de novo; se continuar, avise o suporte."]
                });
            }
        }
    }
}
