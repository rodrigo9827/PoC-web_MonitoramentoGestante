using Microsoft.AspNetCore.Mvc;
using MonitoramentoGestante.Server.Services;

namespace MonitoramentoGestante.Server.Controllers
{
    [ApiController]
    [Route ("api/[controller]")]
    public class EnfermeirasController : ControllerBase
    {
        private readonly IEnfermeiraService _service;

        // O ASP .NET Entrega o service pronto aqui (Injeçao de dependência)
        public EnfermeirasController(IEnfermeiraService service) 
        { 
            _service = service;
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> Listar()
        {
            return Ok(_service.Listar());
        }
    }
}