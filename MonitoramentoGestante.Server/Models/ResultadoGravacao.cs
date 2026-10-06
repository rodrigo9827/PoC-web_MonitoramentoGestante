namespace MonitoramentoGestante.Server.Models
{
    // Eu sou o cara que carrega o formulário da Busca Ativa da tela até o servidor.
    // Quando a enfermeira clica em "Enviar pra enfermeira", o JSON vira um objeto meu.
    // Eu não confiro nada e não gravo nada: só levo os campos (por isso tudo pode vir vazio).
    public class ResultadoGravacao
    {
        public List<string> Erros { get; set; } = [];
        public bool Sucesso => Erros.Count == 0;
        public string? Chave { get; set; }
        public DateOnly? DataRetorno { get; set; }
    }
}
