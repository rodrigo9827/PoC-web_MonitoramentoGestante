using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitoramentoGestante.Server.Models
{
    [Table("Gestante_Atual", Schema = "app")]
    public class GestanteAtual
    {
        [Key]
        [Column("chave", TypeName = "varchar(20)")]
        public string Chave { get; set; } = "";

        [Column("cns", TypeName = "varchar(15)")]
        public string? Cns { get; set; }

        [Column("cpf", TypeName = "varchar(11)")]
        public string? Cpf { get; set; }

        [Column("gestante")]
        public string? Gestante { get; set; }

        [Column("enfermeira")]
        public string? Enfermeira { get; set; }

        [Column("teleoperador")]
        public string? Teleoperador { get; set; }

        [Column("classificacao")]
        public string? Classificacao { get; set; }

        [Column("conseguiu_contato")]
        public string? ConseguiuContato { get; set; }

        [Column("data_contato")]
        public DateOnly DataContato { get; set; }

        [Column("ig_semanas")]
        public byte? IgSemanas { get; set; }

        [Column("ig_dias")]
        public byte? IgDias { get; set; }

        [Column("data_retorno")]
        public DateOnly? DataRetorno { get; set; }

        [Column("alto_risco")]
        public bool? AltoRisco { get; set; }

        [Column("caso_critico")]
        public string? CasoCritico { get; set; }

        [Column("qual_caso_critico")]
        public string? QualCasoCritico { get; set; }

        [Column("ultima_observacao")]
        public string? UltimaObservacao { get; set; }

        [Column("ultimo_formulario")]
        public string UltimoFormulario { get; set; } = "";

        [Column("data_hora_modificacao")]
        public DateTime DataHoraModificacao { get; set; }


    }
}