using System.Data;
using Microsoft.EntityFrameworkCore;
using MonitoramentoGestante.Server.Data;
using MonitoramentoGestante.Server.Models;
// Eu sou o cara que GRAVA no banco cada envio de formulário (Busca Ativa ou Finalizar Atendimento).
// Cada envio é gravado sozinho, separado dos outros, e escreve em DUAS tabelas:
//   - uma linha nova no histórico (app.Monitora_Gestante_Adm);
//   - a situação atual da gestante (app.Gestante_Atual), criada ou atualizada.
// As duas tabelas são gravadas juntas numa transação: se uma falhar, a outra também não fica.
// Também guardo as ferramentas pequenas que os formulários usam (Sim/Não, vazio vira null,
// texto numa linha só, hora sem frações). Nunca escrevo em dbo.* nem em stg.*.

namespace MonitoramentoGestante.Server.Services
{
    public static class GravacaoGestante
    {
        public static void Gravar(MonitoramentoContext context, MonitoraGestanteAdm historico, GestanteAtual nova)
        {
            using var transacao = context.Database.BeginTransaction(IsolationLevel.Serializable);

            context.HistoricoAdm.Add(historico);

            var atual = context.GestantesAtuais.FirstOrDefault(g => g.Chave == nova.Chave);
            if (atual is null)
            {
                context.GestantesAtuais.Add(nova);
            }
            else if (nova.DataContato >= atual.DataContato)
            {
                Atualizar(atual, nova);
            }
            // contato mais antigo que o guardado: só o histórico é gravado

            context.SaveChanges();
            transacao.Commit();
        }

        // Mesmas regras do MERGE do gravar_banco.py:
        // campo que veio vazio NÃO apaga o que já estava guardado
        private static void Atualizar(GestanteAtual atual, GestanteAtual nova)
        {
            atual.Cns = nova.Cns ?? atual.Cns;
            atual.Cpf = nova.Cpf ?? atual.Cpf;
            atual.Gestante = nova.Gestante ?? atual.Gestante;
            atual.Enfermeira = nova.Enfermeira ?? atual.Enfermeira;
            atual.Teleoperador = nova.Teleoperador ?? atual.Teleoperador;
            atual.Classificacao = nova.Classificacao ?? atual.Classificacao;
            atual.ConseguiuContato = nova.ConseguiuContato ?? atual.ConseguiuContato;
            atual.DataContato = nova.DataContato;

            if (nova.IgSemanas is not null)             // semanas e dias andam juntos
            {
                atual.IgSemanas = nova.IgSemanas;
                atual.IgDias = nova.IgDias;
            }

            atual.DataRetorno = nova.DataRetorno ?? atual.DataRetorno;
            atual.AltoRisco = nova.AltoRisco ?? atual.AltoRisco;
            atual.CasoCritico = nova.CasoCritico ?? atual.CasoCritico;
            atual.QualCasoCritico = nova.CasoCritico == "Não"
                ? null                                  // caso crítico "Não" limpa a descrição
                : nova.QualCasoCritico ?? atual.QualCasoCritico;
            atual.UltimaObservacao = nova.UltimaObservacao ?? atual.UltimaObservacao;
            atual.UltimoFormulario = nova.UltimoFormulario;
            atual.DataHoraModificacao = nova.DataHoraModificacao;
        }

        // ---------- ferramentas pequenas usadas pelos formulários ----------

        public static string SimNao(bool valor) => valor ? "Sim" : "Não";

        // pergunta que a enfermeira pode deixar sem resposta: sem resposta vira null
        public static string? SimNaoOuNull(bool? valor) => valor is null ? null : SimNao(valor.Value);

        public static string? VazioParaNull(string? texto) => string.IsNullOrEmpty(texto) ? null : texto;

        // Quebras de linha viram " / " (igual ao Python); texto vazio vira null
        public static string? UmaLinha(string? texto)
        {
            var partes = (texto ?? "").Split('\n',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return partes.Length == 0 ? null : string.Join(" / ", partes);
        }

        // O banco guarda data e hora sem frações de segundo
        public static DateTime AgoraSemFracao()
        {
            var n = DateTime.Now;
            return new DateTime(n.Year, n.Month, n.Day, n.Hour, n.Minute, n.Second);
        }
    }
}