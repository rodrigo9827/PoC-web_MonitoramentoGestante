// Eu sou o cara que fala o que o service(serviço) de observações sabe fazer
// no caso listar as observações de uma gestante pela chave (CNS ou CPF)
using MonitoramentoGestante.Server.Models;

namespace MonitoramentoGestante.Server.Services
{
    public interface IObservacaoService
    {
        IEnumerable<Observacao> ListarPorGestante(string chave);
    }
}
