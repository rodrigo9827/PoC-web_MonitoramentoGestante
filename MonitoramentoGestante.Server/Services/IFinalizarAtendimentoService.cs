using MonitoramentoGestante.Server.Models;

namespace MonitoramentoGestante.Server.Services
    //Eu sou o cara que diz o que o service do FinalizarAtendimento sabe fazer
    //receber Formulários e devolver o resultado (erros, ou chave e o retorno)
{
    public interface IFinalizarAtendimentoService
    {
        ResultadoGravacao Finalizar(FinalizarAtendimentoEntrada entrada);
    }
}
