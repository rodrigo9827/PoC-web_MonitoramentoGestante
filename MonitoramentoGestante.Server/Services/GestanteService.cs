using Microsoft.Identity.Client;
using MonitoramentoGestante.Server.Data;
using MonitoramentoGestante.Server.Models;

namespace MonitoramentoGestante.Server.Services
{
    public class GestanteService : IGestanteService
    {
        private readonly MonitoramentoContext _context;
        public GestanteService(MonitoramentoContext context)
        {
            _context = context;
        }

        public IEnumerable<GestanteRetorno> ListarPorEnfermeira(string enfermeira)
        {
            return _context.GestantesRetorno
                .Where(g => g.Enfermeira == enfermeira)
                .OrderBy(g => g.DataRetorno)
                .ToList();
        }
    }
}