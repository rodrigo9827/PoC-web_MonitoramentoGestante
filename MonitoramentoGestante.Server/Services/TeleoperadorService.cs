using MonitoramentoGestante.Server.Data;
using MonitoramentoGestante.Server.Models;

// Eu sou o cara que busca os teleoperadores na app.Teleoperadores,
// só os ativos, em ordem alfabética e com o "Nenhum" no final,
// e entrega só Id e Nome para a tela
namespace MonitoramentoGestante.Server.Services
{
    public class TeleoperadorService : ITeleoperadorService
    {
        private const string Nenhum = "Nenhum";
        private readonly MonitoramentoContext _context;

        public TeleoperadorService(MonitoramentoContext context)
        {
            _context = context;
        }

        public List<OpcaoLista> Listar()
        {
            return _context.Teleoperadores
                .Where(t => t.Ativo)
                // "Nenhum" vai pro fim, o resto em ordem alfabética
                .OrderBy(t => t.Nome == Nenhum ? 1 : 0)
                .ThenBy(t => t.Nome)
                .Select(t => new OpcaoLista { Id = t.Id, Nome = t.Nome })
                .ToList();
        }
    }
}