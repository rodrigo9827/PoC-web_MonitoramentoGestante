using MonitoramentoGestante.Server.Models;
// sou a interface da busca ativa
namespace MonitoramentoGestante.Server.Services
{
    public interface IBuscaAtivaService
    {
        ResultadoGravacao Registrar(BuscaAtivaEntrada entrada);
    }
}
