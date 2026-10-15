using MonitoramentoGestante.Server.Models;
// Eu sou o cara que diz o que o serviço de teleoperadores sabe fazer:
// listar os teleoperadores ativos (Id e Nome) para a tela
namespace MonitoramentoGestante.Server.Services
{
    public interface ITeleoperadorService
    {
        List<OpcaoLista> Listar();
    }
}