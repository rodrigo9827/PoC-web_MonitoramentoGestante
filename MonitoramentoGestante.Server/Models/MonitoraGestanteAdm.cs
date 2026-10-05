using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitoramentoGestante.Server.Models
{
    // Tabela app.Monitora_Gestante_Adm: o histórico. Cada envio vira UMA linha nova.
    [Table("Monitora_Gestante_Adm", Schema = "app")]
    public class MonitoraGestanteAdm
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("data_hora_modificacao")]
        public DateTime DataHoraModificacao { get; set; }

        [Column("formulario")]
        public string Formulario { get; set; } = "";

        [Column("enfermeira")]
        public string? Enfermeira { get; set; }

        [Column("teleoperador")]
        public string? Teleoperador { get; set; }

        [Column("gestante")]
        public string? Gestante { get; set; }

        [Column("cns", TypeName = "varchar(15)")]
        public string? Cns { get; set; }

        [Column("data_contato")]
        public DateOnly? DataContato { get; set; }

        [Column("numero")]
        public string? Numero { get; set; }

        [Column("conseguiu_contato")]
        public string? ConseguiuContato { get; set; }

        [Column("motivo_sem_contato")]
        public string? MotivoSemContato { get; set; }

        [Column("classificacao")]
        public string? Classificacao { get; set; }

        [Column("tratamento_sifilis")]
        public string? TratamentoSifilis { get; set; }

        [Column("gestante_risco")]
        public string? GestanteRisco { get; set; }

        [Column("pre_natal")]
        public string? PreNatal { get; set; }

        [Column("nasceu_vivo")]
        public string? NasceuVivo { get; set; }

        [Column("local_parto")]
        public string? LocalParto { get; set; }

        [Column("outro_local")]
        public string? OutroLocal { get; set; }

        [Column("bebe_risco")]
        public string? BebeRisco { get; set; }

        [Column("obito_neonatal")]
        public string? ObitoNeonatal { get; set; }

        [Column("obito_materno")]
        public string? ObitoMaterno { get; set; }

        [Column("houve_violencia")]
        public string? HouveViolencia { get; set; }

        [Column("tipo_violencia")]
        public string? TipoViolencia { get; set; }

        [Column("tipo_parto")]
        public string? TipoParto { get; set; }

        [Column("risco_gestacional")]
        public string? RiscoGestacional { get; set; }

        [Column("qual_risco")]
        public string? QualRisco { get; set; }

        // Aqui a IG é TEXTO no formato ss+dd, igual ao que foi digitado
        [Column("ig_semanas")]
        public string? IgSemanas { get; set; }

        [Column("data_retorno")]
        public DateOnly? DataRetorno { get; set; }

        // Aqui o alto risco é TEXTO ("Sim" / "Não"); na Gestante_Atual é bit
        [Column("alto_risco")]
        public string? AltoRisco { get; set; }

        [Column("caso_critico")]
        public string? CasoCritico { get; set; }

        [Column("qual_caso_critico")]
        public string? QualCasoCritico { get; set; }

        [Column("observacoes")]
        public string? Observacoes { get; set; }

        [Column("gestante_puerpera")]
        public string? GestantePuerpera { get; set; }

        [Column("gestante_aborto")]
        public string? GestanteAborto { get; set; }

        [Column("gestante_gestante")]
        public string? GestanteGestante { get; set; }

        [Column("cpf", TypeName = "varchar(11)")]
        public string? Cpf { get; set; }

        [Column("busca_hora_inicio")]
        public DateTime? BuscaHoraInicio { get; set; }

        [Column("busca_hora_envio")]
        public DateTime? BuscaHoraEnvio { get; set; }
    }
}