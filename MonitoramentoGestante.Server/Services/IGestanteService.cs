using MonitoramentoGestante.Server.Models;

namespace MonitoramentoGestante.Server.Services
{
    public interface IGestanteService
    {
        IEnumerable<GestanteLista> ListarPorEnfermeira(string enfermeira);
    }
}