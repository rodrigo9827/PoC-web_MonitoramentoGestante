using Microsoft.AspNetCore.Mvc;
using MonitoramentoGestante.Server.Models;
using MonitoramentoGestante.Server.Services;

namespace MonitoramentoGestante.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GestantesController : ControllerBase
    {
        private readonly IGestanteService _service;
        public GestantesController(IGestanteService service)
        {
            _service = service;
        }
        // GET /api/gestantes?enfermeira=NOME
        [HttpGet]
        public ActionResult<IEnumerable<GestanteLista>> Listar([FromQuery] string enfermeira)
        {
            return Ok(_service.ListarPorEnfermeira(enfermeira));
        }
    }
}