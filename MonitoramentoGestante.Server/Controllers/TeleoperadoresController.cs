using Microsoft.AspNetCore.Mvc;
using MonitoramentoGestante.Server.Models;
using MonitoramentoGestante.Server.Services;

// Eu sou o cara que atende o GET /api/teleoperadores:
// peço a lista ao TeleoperadorService e devolvo Id e Nome dos ativos para a tela;
// se o banco falhar, registro no log e respondo erro 500 com uma mensagem simples
namespace MonitoramentoGestante.Server.Controllers
{
    [ApiController]
    [Route("api/teleoperadores")]
    public class TeleoperadoresController : ControllerBase
    {
        private readonly ITeleoperadorService _service;
        private readonly ILogger<TeleoperadoresController> _logger;

        public TeleoperadoresController(ITeleoperadorService service, ILogger<TeleoperadoresController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // GET /api/teleoperadores
        [HttpGet]
        public ActionResult<List<OpcaoLista>> Listar()
        {
            try
            {
                return Ok(_service.Listar());
            }
            catch (Exception erro)
            {
                _logger.LogError("Falha ao listar Teleoperadores: {Tipo}", erro.GetBaseException().GetType().Name);
                return StatusCode(500, "Não foi possível buscar os Teleoperadores");
            }
        }
    }
}