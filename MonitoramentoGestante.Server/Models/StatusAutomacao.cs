// Eu sou o cara que carrega o estado da automação do e-Saúde, para a tela acompanhar:
//   Estado   -> "ociosa" | "buscando" | "pronta" | "erro"
//   Mensagem -> o texto que a enfermeira lê ("Procurando...", "Erro...")
//   Link     -> o endereço da ficha da gestante (.../app/person/ID), só quando Estado = "pronta"
namespace MonitoramentoGestante.Server.Models
{
    
    public class StatusAutomacao
    {
        public string Estado { get; set; } = "ociosa";
        public string Mensagem { get; set; } = "";
        public string? Link { get; set; }
    }
}