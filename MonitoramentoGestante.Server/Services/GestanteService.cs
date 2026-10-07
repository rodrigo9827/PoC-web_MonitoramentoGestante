using Microsoft.EntityFrameworkCore;
using MonitoramentoGestante.Server.Data;
using MonitoramentoGestante.Server.Models;
using MonitoramentoGestante.Server.Regras;

namespace MonitoramentoGestante.Server.Services
{
    // Eu sou o cara que monta a lista de gestantes de uma enfermeira.
    // Leio a VIEW (dbo.vw_RetornoGestantes) e junto com o que foi registrado pelo sistema
    // (app.Gestante_Atual): contato, IG, retorno e alto risco mais novos, as gestantes
    // novas da Busca Ativa e o aviso "Busca ativa dd/mm hh:mm". Eu só leio, não gravo nada.
    public class GestanteService : IGestanteService
    {
        private readonly MonitoramentoContext _context;

        public GestanteService(MonitoramentoContext context)
        {
            _context = context;
        }

        public IEnumerable<GestanteLista> ListarPorEnfermeira(string enfermeira)
        {
            // 1. As gestantes da VIEW
            var daView = _context.GestantesRetorno
                .Where(g => g.Enfermeira == enfermeira)
                .ToList();

            var chavesDaView = daView
                .Select(g => RegrasGestante.Chave(g.Cns, g.Cpf))
                .Where(chave => chave != null)
                .Select(chave => chave!)
                .Distinct()
                .ToList();

            // 2. O que o sistema registrou: dessas gestantes e as dessa enfermeira
            var atuais = _context.GestantesAtuais
                .AsNoTracking()
                .Where(a => a.Enfermeira == enfermeira || chavesDaView.Contains(a.Chave))
                .ToList()
                .ToDictionary(a => a.Chave);

            // 3. Linhas da VIEW, atualizadas com o que o sistema registrou
            var lista = new List<GestanteLista>();
            var chavesNaLista = new HashSet<string>();

            foreach (var g in daView)
            {
                var item = new GestanteLista
                {
                    Chave = RegrasGestante.Chave(g.Cns, g.Cpf),
                    Gestante = g.Gestante,
                    Cns = g.Cns,
                    Cpf = g.Cpf,
                    DataContato = g.DataContato,
                    IgSemanas = g.IgSemanas,
                    IgDias = g.IgDiasResto,
                    DataRetorno = g.DataRetorno,
                    AltoRisco = g.AltoRisco == 1
                };

                if (item.Chave != null)
                {
                    chavesNaLista.Add(item.Chave);
                    if (atuais.TryGetValue(item.Chave, out var atual))
                    {
                        Aplicar(item, atual, g.DataContato);
                    }
                }

                lista.Add(item);
            }

            // 4. Gestantes NOVAS da Busca Ativa (ainda não estão na VIEW)
            foreach (var a in atuais.Values)
            {
                bool dessaEnfermeira = string.Equals(a.Enfermeira, enfermeira, StringComparison.OrdinalIgnoreCase);
                if (!dessaEnfermeira || chavesNaLista.Contains(a.Chave) || !EhGestante(a)) continue;

                lista.Add(new GestanteLista
                {
                    Chave = a.Chave,
                    Gestante = a.Gestante,
                    Cns = a.Cns,
                    Cpf = a.Cpf,
                    DataContato = a.DataContato,
                    IgSemanas = a.IgSemanas,
                    IgDias = a.IgDias,
                    DataRetorno = a.DataRetorno,
                    AltoRisco = a.AltoRisco == true,
                    BuscaAtiva = TextoBuscaAtiva(a)
                });
            }

            // 5. Mais urgente primeiro; sem data de retorno vai para o fim
            return lista
                .OrderBy(i => i.DataRetorno is null)
                .ThenBy(i => i.DataRetorno)
                .ToList();
        }

        // O que o sistema registrou só vale se for do mesmo dia (ou mais novo) que o contato da VIEW
        private static void Aplicar(GestanteLista item, GestanteAtual atual, DateOnly? contatoDaView)
        {
            if (contatoDaView is not null && atual.DataContato < contatoDaView) return;

            item.DataContato = atual.DataContato;
            item.BuscaAtiva = TextoBuscaAtiva(atual);

            if (!EhGestante(atual)) return;         // Puérpera e Aborto não mexem na IG nem no retorno

            if (atual.IgSemanas is not null)
            {
                item.IgSemanas = atual.IgSemanas;
                item.IgDias = atual.IgDias ?? 0;
            }
            item.DataRetorno = atual.DataRetorno ?? item.DataRetorno;
            item.AltoRisco = atual.AltoRisco ?? item.AltoRisco;
        }

        // Registros antigos, sem classificação, contam como gestante
        private static bool EhGestante(GestanteAtual atual)
        {
            return atual.Classificacao != "Puérpera" && atual.Classificacao != "Aborto";
        }

        // Pendente = o último registro da gestante foi uma Busca Ativa
        private static string? TextoBuscaAtiva(GestanteAtual atual)
        {
            if (atual.UltimoFormulario != BuscaAtivaService.NomeFormulario) return null;
            return $"Busca ativa {atual.DataHoraModificacao:dd/MM HH:mm}";
        }
    }
}