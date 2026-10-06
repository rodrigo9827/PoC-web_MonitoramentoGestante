namespace MonitoramentoGestante.Server.Models
{
    // Eu sou o cara que carrega o formulário da Busca Ativa da tela até o servidor.
    // Quando a enfermeira clica em "Enviar pra enfermeira", o JSON vira um objeto meu.
    // Eu não confiro nada e não gravo nada: só levo os campos (por isso tudo pode vir vazio).
    public class BuscaAtivaEntrada
    {
        public string? Enfermeira { get; set; }
        public string? Gestante { get; set; }
        public string? Cns { get; set; }
        public string? Cpf { get; set; }
        public DateOnly? DataContato { get; set; }
        public string? Ig { get; set; }
        public DateOnly? DataRetorno { get; set; }
        public bool? AltoRisco { get; set; }
        public bool? CasoCritico { get; set; }
        public string? QualCasoCritico { get; set; }
        public string? Classificacao { get; set; }
        public string? Observacoes { get; set; }
        public DateTime? HoraInicio { get; set; }
    }
}