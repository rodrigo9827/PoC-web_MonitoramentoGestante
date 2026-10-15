//eu sou o cara que representa um hospital da lista de locais de parto, lido da Core.Estabelecimento (hospitais da cidade de São Paulo)




namespace MonitoramentoGestante.Server
{
    public class Hospital
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Cnes { get; set; }
        public string? Cmes { get; set; }
    }
}
