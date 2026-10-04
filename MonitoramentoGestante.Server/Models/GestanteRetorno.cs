namespace MonitoramentoGestante.Server.Models
{
    public class GestanteRetorno
    {
        public string? Enfermeira { get; set; }
        public string? Cns { get; set; }
        public string? Gestante { get; set; }
        public DateOnly? DataContato { get; set; }
        public int? IgSemanas { get; set; }
        public int? IgDiasResto { get; set; }
        public DateOnly? DataRetorno { get; set; }
        public int? AltoRisco { get; set; }
        public string? Cpf { get; set; }
    }
}
