using MonitoramentoGestante.Server.Models;

namespace MonitoramentoGestante.Server.Services
{
    public interface IGestanteService
    {
        IEnumerable<GestanteRetorno> ListarPorEnfermeira(string enfermeira);
    }
}