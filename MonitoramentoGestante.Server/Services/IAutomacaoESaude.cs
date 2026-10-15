using MonitoramentoGestante.Server.Models;
// Eu sou o cara que diz O QUE a automação do e-Saúde sabe fazer:
// começar a buscar uma gestante e contar em que pé está (para a tela acompanhar).
namespace MonitoramentoGestante.Server.Services
{
    
    public interface IAutomacaoESaude
    {
        // o estado atual (a tela lê para mostrar a mensagem e, quando pronto, abrir o link)
        StatusAutomacao Status { get; }

        // começa a busca em 2º plano; devolve false se ainda estiver buscando a anterior
        bool Iniciar(string? cns, string? cpf);
    }
}