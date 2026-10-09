// Eu sou o cara que carrega o que a tela manda no "Finalizar Atendimento"
namespace MonitoramentoGestante.Server.Models
{
    public class FinalizarAtendimentoEntrada
    {
        // sempre aparecem
        public string? Enfermeira {  get; set; }
        public string? Teleoperador { get; set; }
        public bool? ConseguiuContato { get; set; }
        public string? Gestante { get; set; }
        public string? Cns {  get; set; }
        public string? Cpf { get; set; }
        public string? Numero { get; set; }
        public DateOnly? DataContato { get; set; }

        // não conseguiu o contato
        public string? MotivoSemContato { get; set; }
        
        // conseguiu o contato
        public string? Classificacao { get; set; }
        public bool? TratamentoSifilis { get; set; }
        
        // Classificação gestante
        public bool? GestanteRisco { get; set; }
        public bool? PreNatal {  get; set; }
        public string? Ig { get; set; }
        public DateOnly? DataRetorno { get; set; }

        // classificação Puérpera
        public bool? NasceuVivo { get; set; }
        public string? LocalParto { get; set; }
        public string? OutroLocal {  get; set; }
        public bool? BebeRisco { get; set; }
        public bool? ObitoNeonatal {  get; set; }
        public bool? ObitoMaterno { get; set; }
        public bool? HouveViolencia {  get; set; }
        public string? TipoViolencia { get; set; }
        public string? TipoParto { get; set; }
        public bool? RiscoGestacional { get; set; }
        public string? QualRisco { get; set; }

        // sempre aparecem Fim do formulário

        public bool? CasoCritico { get; set; }
        public string? QualCasoCritico { get; set;  }
        public string? Observacoes { get; set; }
    }
}
