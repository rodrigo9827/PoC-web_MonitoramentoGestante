namespace MonitoramentoGestante.Server.Models
{
    public class GestanteLista
    {
        public string? Chave { get; set; }
        public string? Gestante { get; set; }
        public string? Cns {  get; set; }
        public string? Cpf { get; set; }
        public DateOnly? DataContato { get; set; }
        public int? IgSemanas { get; set; }
        public int? IgDias { get; set; }
        public DateOnly? DataRetorno { get; set; }
        public bool? AltoRisco { get; set; }
        public string? BuscaAtiva { get; set; }
    }
}
