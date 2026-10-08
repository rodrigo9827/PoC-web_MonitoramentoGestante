// Eu sou o cara que leva uma observação para a tela, quando ela foi escrita,
// emqual formulário, qual enfermeira escreveu e o que ela escreveu
// para facilitar a identificação visual
namespace MonitoramentoGestante.Server.Models
{
    public class Observacao
    {
        public DateTime DataHora { get; set; }
        public string Formulario { get; set; } = "";
        public string? Enfermeira { get; set; }
        public string Texto { get; set; } = "";
    }
}