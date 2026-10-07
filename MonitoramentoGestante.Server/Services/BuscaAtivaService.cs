using System.Data;
using Microsoft.EntityFrameworkCore;
using MonitoramentoGestante.Server.Data;
using MonitoramentoGestante.Server.Models;
using MonitoramentoGestante.Server.Regras;

namespace MonitoramentoGestante.Server.Services
{
    // Eu sou o cara que registra a Busca Ativa de verdade.
    // Recebo o formulário, confiro tudo com as regras do RegrasGestante e, se estiver certo,
    // gravo uma linha nova no histórico (app.Monitora_Gestante_Adm) e insiro ou atualizo
    // a situação da gestante (app.Gestante_Atual), as duas juntas numa transação:
    // ou as duas ficam, ou nenhuma. Nunca escrevo em dbo.* nem em stg.*.
    public class BuscaAtivaService : IBuscaAtivaService
    {
        public const string NomeFormulario = "Busca Ativa";
        private static readonly string[] Classificacoes = ["Gestante", "Puérpera", "Aborto"];

        private readonly MonitoramentoContext _context;

        public BuscaAtivaService(MonitoramentoContext context)
        {
            _context = context;
        }

        public ResultadoGravacao Registrar(BuscaAtivaEntrada entrada)
        {
            var resultado = new ResultadoGravacao { Erros = Validar(entrada) };
            if (!resultado.Sucesso) return resultado;

            // ---------- 1. Preparar os valores (já validados) ----------
            var ig = RegrasGestante.LerIg(entrada.Ig)!.Value;
            var contato = entrada.DataContato!.Value;
            var retorno = entrada.DataRetorno
                          ?? RegrasGestante.CalcularRetorno(contato, ig.Semanas, ig.Dias);
            var agora = AgoraSemFracao();

            var cns = VazioParaNull(entrada.Cns?.Trim());
            var cpf = VazioParaNull(RegrasGestante.SoDigitos(entrada.Cpf));
            var chave = RegrasGestante.Chave(cns, cpf)!;

            bool casoCritico = entrada.CasoCritico!.Value;
            var qualCaso = casoCritico ? entrada.QualCasoCritico!.Trim() : null;
            var observacoes = (entrada.Observacoes ?? "").Trim();
            // na situação atual, o caso crítico vai no começo da observação
            var observacaoComCaso = qualCaso is null ? observacoes : $"[CASO CRÍTICO] {qualCaso}\n{observacoes}";

            // ---------- 2. A linha do histórico ----------
            var historico = new MonitoraGestanteAdm
            {
                DataHoraModificacao = agora,
                Formulario = NomeFormulario,
                Enfermeira = entrada.Enfermeira,
                Gestante = entrada.Gestante!.Trim(),
                Cns = cns,
                Cpf = cpf,
                DataContato = contato,
                IgSemanas = $"{ig.Semanas}+{ig.Dias}",
                DataRetorno = retorno,
                AltoRisco = SimNao(entrada.AltoRisco!.Value),
                CasoCritico = SimNao(casoCritico),
                QualCasoCritico = qualCaso,
                Observacoes = UmaLinha(observacoes),
                GestanteGestante = SimNao(entrada.Classificacao == "Gestante"),
                GestantePuerpera = SimNao(entrada.Classificacao == "Puérpera"),
                GestanteAborto = SimNao(entrada.Classificacao == "Aborto"),
                BuscaHoraInicio = entrada.HoraInicio,
                BuscaHoraEnvio = agora
            };

            // ---------- 3. A situação atual ----------
            var nova = new GestanteAtual
            {
                Chave = chave,
                Cns = cns,
                Cpf = cpf,
                Gestante = entrada.Gestante.Trim(),
                Enfermeira = entrada.Enfermeira,
                Classificacao = entrada.Classificacao,
                DataContato = contato,
                IgSemanas = (byte)ig.Semanas,
                IgDias = (byte)ig.Dias,
                DataRetorno = retorno,
                AltoRisco = entrada.AltoRisco.Value,
                CasoCritico = SimNao(casoCritico),
                QualCasoCritico = qualCaso,
                UltimaObservacao = UmaLinha(observacaoComCaso),
                UltimoFormulario = NomeFormulario,
                DataHoraModificacao = agora
            };

            // ---------- 4. Gravar as duas juntas ----------
            using var transacao = _context.Database.BeginTransaction(IsolationLevel.Serializable);

            _context.HistoricoAdm.Add(historico);

            var atual = _context.GestantesAtuais.FirstOrDefault(g => g.Chave == chave);
            if (atual is null)
            {
                _context.GestantesAtuais.Add(nova);
            }
            else if (nova.DataContato >= atual.DataContato)
            {
                Atualizar(atual, nova);
            }
            // contato mais antigo que o guardado: só o histórico é gravado

            _context.SaveChanges();
            transacao.Commit();

            resultado.Chave = chave;
            resultado.DataRetorno = retorno;
            return resultado;
        }

        // As mesmas conferências da janela "Registrar Busca Ativa" do Python
        private static List<string> Validar(BuscaAtivaEntrada e)
        {
            var erros = new List<string>();

            if (string.IsNullOrWhiteSpace(e.Enfermeira))
                erros.Add("Escolha a enfermeira.");

            if (string.IsNullOrWhiteSpace(e.Gestante))
                erros.Add("Informe o nome da gestante.");
            else if (e.Gestante.Trim().Length > 255)
                erros.Add("O nome da gestante passou de 255 letras.");

            var cns = (e.Cns ?? "").Trim();
            var cpf = (e.Cpf ?? "").Trim();
            if (cns != "" && !RegrasGestante.CnsValido(cns))
                erros.Add("O CNS precisa ter exatamente 15 números (ou deixe vazio e informe o CPF).");
            if (cpf != "" && !RegrasGestante.CpfValido(cpf))
                erros.Add("CPF inválido: confira os 11 números.");
            if (cns == "" && cpf == "")
                erros.Add("Informe o CNS ou o CPF da gestante.");

            if (e.DataContato is null)
                erros.Add("Informe a data do contato.");
            else if (e.DataContato > DateOnly.FromDateTime(DateTime.Now))
                erros.Add("A data do contato não pode ser depois de hoje.");

            if (RegrasGestante.LerIg(e.Ig) is null)
                erros.Add("Idade gestacional deve estar no formato semanas+dias, ex.: 30+2 (semanas 0 a 45, dias 0 a 6).");

            if (e.DataRetorno is not null && e.DataContato is not null && e.DataRetorno < e.DataContato)
                erros.Add("A data de retorno não pode ser antes da data do contato.");

            if (e.AltoRisco is null)
                erros.Add("Responda se é paciente de alto risco.");

            if (e.CasoCritico is null)
                erros.Add("Responda se é caso crítico.");
            else if (e.CasoCritico == true && string.IsNullOrWhiteSpace(e.QualCasoCritico))
                erros.Add("Informe qual é o caso crítico.");
            else if (e.CasoCritico == true && e.QualCasoCritico!.Trim().Length > 1000)
                erros.Add("A descrição do caso crítico passou de 1000 letras.");

            if (!Classificacoes.Contains(e.Classificacao))
                erros.Add("Marque a classificação da gestante (Gestante, Puérpera ou Aborto).");

            return erros;
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

        private static string SimNao(bool valor) => valor ? "Sim" : "Não";

        private static string? VazioParaNull(string? texto) => string.IsNullOrEmpty(texto) ? null : texto;

        // Quebras de linha viram " / " (igual ao Python); texto vazio vira null
        private static string? UmaLinha(string? texto)
        {
            var partes = (texto ?? "").Split('\n',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return partes.Length == 0 ? null : string.Join(" / ", partes);
        }

        // O banco guarda data e hora sem frações de segundo
        private static DateTime AgoraSemFracao()
        {
            var n = DateTime.Now;
            return new DateTime(n.Year, n.Month, n.Day, n.Hour, n.Minute, n.Second);
        }
    }
}