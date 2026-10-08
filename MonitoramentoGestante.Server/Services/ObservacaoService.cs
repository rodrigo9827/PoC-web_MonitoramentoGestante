// Eu sou o cara que busca as observações de uma gestante no histórico por (app.Monitora_Gestante_Adm)
// acho as linhas dela pela cahve (cns ou cpf) junto com o caso ccritico e texto
// e devolvo da mais nova para a mais antiga. Só leio, não gravo nada.
using Microsoft.EntityFrameworkCore;
using MonitoramentoGestante.Server.Data;
using MonitoramentoGestante.Server.Models;

namespace MonitoramentoGestante.Server.Services
{
    public class ObservacaoService : IObservacaoService
    {
        private const string PrefixoCpf = "CPF ";
        private readonly MonitoramentoContext _context;
        public ObservacaoService(MonitoramentoContext context)
        {
            _context = context;
        }

        public IEnumerable<Observacao> ListarPorGestante(string chave)
        {
            chave = (chave ?? "").Trim();

            // só as linhas que têm alguma coisa escrita
            // Alterar para interagir co a aplicação do pedro
            var consulta = _context.HistoricoAdm
                .AsNoTracking()
                .Where(a => a.Observacoes != null || a.QualCasoCritico != null);
            if (chave.StartsWith(PrefixoCpf))
            {
                // gestante Sem cns chave é cpf
                string cpf = chave.Substring(PrefixoCpf.Length);
                consulta = consulta.Where(a => (a.Cns == null || a.Cns == "") && a.Cpf == cpf);
            }
            else
            {
                // gestante com cns a chave é o cns
                consulta = consulta.Where(a => a.Cns == chave);
            }

            // Mais nova primeiro quem foi gravada por ultimo fica em cima
            var linhas = consulta
                .OrderByDescending(a => a.DataHoraModificacao)
                .ThenByDescending(a => a.Id)
                .ToList();
            var resultado = new List<Observacao>();
            foreach (var a in linhas)
            {
                string texto = MontarTexto(a.CasoCritico, a.QualCasoCritico, a.Observacoes);
                if (texto == "") continue;
                resultado.Add(new Observacao
                {
                    DataHora = a.DataHoraModificacao,
                    Formulario = a.Formulario,
                    Enfermeira = a.Enfermeira,
                    Texto = texto
                });
            }
            return resultado;
        }

        // Caso crítico "Sim" com descrição: "[CASO CRÍTICO] descrição" na primeira linha
        // e a observação embaixo (igual ao quadro da PoC em Python)
        // alterar para incluir a aplicação do Pedro
        public static string MontarTexto(string? casoCritico, string? qualCasoCritico, string? observacoes)
        {
            string obs = (observacoes ?? "").Trim();
            string descricao = (qualCasoCritico ?? "").Trim();

            if (casoCritico == "Sim" && descricao != "")
                return obs == "" ? $"[CASO CRÍTICO] {descricao}" : $"[CASO CRÍTICO] {descricao}\n{obs}";

            return obs;
        }
    }
}

