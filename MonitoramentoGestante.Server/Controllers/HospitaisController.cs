using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitoramentoGestante.Server.Data;
using MonitoramentoGestante.Server.Models;

// Eu sou o cara que entrega pra tela a lista de locais de parto (só Id e Nome),
// lendo os hospitais da cidade de São Paulo na Core.Estabelecimento
// (consulta do supervisor, mapeada no MonitoramentoContext),
// em ordem alfabética e sem deixar CNES e CMES saírem do servidor
namespace MonitoramentoGestante.Server.Controllers
{
    [ApiController]
    [Route("api/hospitais")]
    public class HospitaisController : ControllerBase
    {
        private readonly MonitoramentoContext _context;

        public HospitaisController(MonitoramentoContext context)
        {
            _context = context;
        }

        // GET /api/hospitais
        [HttpGet]
        public async Task<ActionResult<List<OpcaoLista>>> Listar()
        {
            var lista = await _context.Hospitais
                .OrderBy(h => h.Nome)
                // só Id e Nome viajam pra tela
                .Select(h => new OpcaoLista { Id = h.Id, Nome = h.Nome })
                .ToListAsync();

            return Ok(lista);
        }
    }
}