using MonitoramentoGestante.Server.Data;

namespace MonitoramentoGestante.Server.Services
{
    public class EnfermeiraService : IEnfermeiraService
    {
        private readonly MonitoramentoContext _context;

        public EnfermeiraService(MonitoramentoContext context)
        {
            _context = context;
        }
        public IEnumerable<string> Listar()
        {
            return _context.GestantesRetorno
                .Where(g => g.Enfermeira != null)
                .Select(g => g.Enfermeira!)
                .Distinct()
                .OrderBy(nome => nome)
                .ToList();
        }
    }
}