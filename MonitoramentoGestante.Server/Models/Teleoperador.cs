// eu sou o cara que representa um teleoperador da tabela app.Teleoperadores
namespace MonitoramentoGestante.Server.Models
{
    public class Teleoperador
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool Ativo {  get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime? DataAlteracao { get; set; }
        public string? UsuarioAlteracao { get; set; }
    }
}
