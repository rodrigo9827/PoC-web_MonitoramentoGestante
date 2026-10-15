using MonitoramentoGestante.Server.Data;
using MonitoramentoGestante.Server.Models;
using MonitoramentoGestante.Server.Regras;
using static MonitoramentoGestante.Server.Services.GravacaoGestante;

namespace MonitoramentoGestante.Server.Services
{
    // Eu sou o cara que registra a Busca Ativa de verdade.
    // Recebo o formulário, troco o Id do teleoperador pelo nome que está na app.Teleoperadores,
    // confiro tudo com as regras do RegrasGestante e, se estiver certo,
    // monto a linha do histórico e a situação atual da gestante e entrego as duas
    // para o GravacaoGestante gravar juntas numa transação. Nunca escrevo em dbo.* nem em stg.*.
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
            // troca o Id que veio da tela pelo nome que vai pro histórico
            var nomeTeleoperador = BuscarTeleoperador(entrada.TeleoperadorId);

            var resultado = new ResultadoGravacao { Erros = Validar(entrada, nomeTeleoperador) };
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
            var teleoperador = nomeTeleoperador!;                 

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
                Teleoperador = teleoperador,
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
                Teleoperador = teleoperador,
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

            // ---------- 4. Gravar as duas juntas (transação + regras do MERGE) ----------
            GravacaoGestante.Gravar(_context, historico, nova);

            resultado.Chave = chave;
            resultado.DataRetorno = retorno;
            return resultado;
        }

        // busca o nome do teleoperador pelo Id (só os ativos); null = não achou
        private string? BuscarTeleoperador(int? id)
        {
            if (id is null) return null;
            return _context.Teleoperadores
                .Where(t => t.Id == id && t.Ativo)
                .Select(t => t.Nome)
                .FirstOrDefault();
        }

        // As mesmas conferências da janela "Registrar Busca Ativa" do Python
        // (+ o teleoperador, que a PoC não tinha)
        private static List<string> Validar(BuscaAtivaEntrada e, string? nomeTeleoperador)
        {
            var erros = new List<string>();

            if (string.IsNullOrWhiteSpace(e.Enfermeira))
                erros.Add("Escolha a enfermeira.");

            if (e.TeleoperadorId is null)
                erros.Add("Escolha o teleoperador (ou 'Nenhum').");
            else if (nomeTeleoperador is null)
                erros.Add("Teleoperador não encontrado ou desativado. Atualize a página e escolha de novo.");
            else if (nomeTeleoperador.Length > 50)
                erros.Add("O nome do teleoperador passou de 50 letras.");

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
    }
}