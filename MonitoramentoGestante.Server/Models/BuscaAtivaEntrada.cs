namespace MonitoramentoGestante.Server.Models
{
    // O que a tela envia ao clicar em "Enviar pra enfermeira"
    public class BusaAtivaEntrada
    {
        public string? Enfermeira {  get; set; }
        public string? Gestante { get; set; }
        public string? Cns { get; set; }
    }
}