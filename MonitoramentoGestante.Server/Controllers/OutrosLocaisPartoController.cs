using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitoramentoGestante.Server.Data;
using MonitoramentoGestante.Server.Models;

// Eu sou o cara que entrega pra tela as opções de "outro local de parto"
// (fora do estado, fora do município, parto domiciliar...) da Core.Parametro,
// só com Id e Nome, para quando o parto não foi num hospital da lista
namespace MonitoramentoGestante.Server.Controllers
{
    [ApiController]
    [Route("api/outros-locais-parto")]
    public class OutrosLocaisPartoController : ControllerBase
    {
        private readonly MonitoramentoContext _context;

        public OutrosLocaisPartoController(MonitoramentoContext context)
        {
            _context = context;
        }

        // GET /api/outros-locais-parto
        [HttpGet]
        public async Task<ActionResult<List<OpcaoLista>>> Listar()
        {
            var lista = await _context.OutrosLocaisParto
                .OrderBy(o => o.Id)
                .Select(o => new OpcaoLista { Id = o.Id, Nome = o.Nome })
                .ToListAsync();

            return Ok(lista);
        }
    }
}