using Microsoft.AspNetCore.Mvc.Formatters;
using MonitoramentoGestante.Server.Data;
using MonitoramentoGestante.Server.Models;
using MonitoramentoGestante.Server.Regras;
using System.Reflection.Metadata.Ecma335;
using static MonitoramentoGestante.Server.Services.GravacaoGestante;
// Eu sou o cara que registra o finalizar atendimento de verdade,
// recebo o formulário, confiro tudo com as regras do forms.py da PoC e,
// se estiver certo, monto a linha do histórico e a situação atual de
// gestante, aprooveitando só os campos da situação marcada
// (sem contato / Gestante / Puérpera / Aborto), e entrego as duas para o
// GravacaoGestante gravar junto.
namespace MonitoramentoGestante.Server.Services
{
    public class FinalizarAtendimentoService : IFinalizarAtendimentoService
    {
        public const string NomeFormulario = "Finalizar o Atendimento";
        private static readonly string[] Classificacoes = ["Gestante", "Puérpera", "Aborto"];
        private readonly MonitoramentoContext _context;

        public FinalizarAtendimentoService(MonitoramentoContext context)
        {
            _context = context;
        }

        public ResultadoGravacao Finalizar(FinalizarAtendimentoEntrada entrada)
        {
            var resultado = new ResultadoGravacao { Erros = Validar(entrada) };
            if (!resultado.Sucesso) return resultado;

            // prepara os valores Já Validados
            var contato = entrada.DataContato!.Value;
            var agora = AgoraSemFracao();

            var cns = VazioParaNull(entrada.Cns?.Trim());
            var cpf = VazioParaNull(RegrasGestante.SoDigitos(entrada.Cpf));
            var chave = RegrasGestante.Chave(cns, cpf)!;

            // Qual parte do formulário vale
            bool conseguiu = entrada.ConseguiuContato!.Value;
            bool ehGestante = conseguiu && entrada.Classificacao == "Gestante";
            bool ehPuerpera = conseguiu && entrada.Classificacao == "Puérpera";

            // só aparece IG e data de retorno para Gestante, e os dois são opcionais
            var ig = ehGestante ? RegrasGestante.LerIg(entrada.Ig) : null;
            DateOnly? retorno = null;
            if (ehGestante)
            {
                if (entrada.DataRetorno is not null)
                    retorno = entrada.DataRetorno;
                else if (ig is not null)
                    retorno = RegrasGestante.CalcularRetorno(contato, ig.Value.Semanas, ig.Value.Dias);
            }
            // Mudar para o da aplicação do Pedro
            bool casoCritico = entrada.CasoCritico!.Value;
            var qualCaso = casoCritico ? entrada.QualCasoCritico!.Trim() : null;
            var observacoes = (entrada.Observacoes ?? "").Trim();
            // caso critico no começo da obs.
            var observacaoComCaso = qualCaso is null ? observacoes : $"[CASO CRITÍCO] {qualCaso}\n{observacoes}";

            // A linha do Histórico 
            var historico = new MonitoraGestanteAdm
            {
                DataHoraModificacao = agora,
                Formulario = NomeFormulario,
                Enfermeira = entrada.Enfermeira,
                Teleoperador = Texto(entrada.Teleoperador),
                Gestante = Texto(entrada.Gestante),
                Cns = cns,
                Cpf = cpf,
                DataContato = contato,
                Numero = Texto(entrada.Numero),
                ConseguiuContato = SimNao(conseguiu),

                // não conseguiu o contato
                MotivoSemContato = conseguiu ? null : Texto(entrada.MotivoSemContato),

                // conseguiu o contato
                Classificacao = conseguiu ? entrada.Classificacao : null,
                TratamentoSifilis = conseguiu ? SimNaoOuNull(entrada.TratamentoSifilis) : null,

                // Gestante
                GestanteRisco = ehGestante ? SimNaoOuNull(entrada.GestanteRisco) : null,
                PreNatal = ehGestante ? SimNaoOuNull(entrada.PreNatal) : null,
                IgSemanas = ig is null ? null : $"{ig.Value.Semanas}+{ig.Value.Dias}",
                DataRetorno = retorno,

                // Puérpera
                NasceuVivo = ehPuerpera ? SimNaoOuNull(entrada.NasceuVivo) : null,
                LocalParto = ehPuerpera ? Texto(entrada.LocalParto) : null,
                OutroLocal = ehPuerpera && entrada.LocalParto == "Outro" ? Texto(entrada.OutroLocal) : null,
                BebeRisco = ehPuerpera ? SimNaoOuNull(entrada.BebeRisco) : null,
                ObitoNeonatal = ehPuerpera ? SimNaoOuNull(entrada.ObitoNeonatal) : null,
                ObitoMaterno = ehPuerpera ? SimNaoOuNull(entrada.ObitoMaterno) : null,
                HouveViolencia = ehPuerpera ? SimNaoOuNull(entrada.HouveViolencia) : null,
                TipoViolencia = ehPuerpera && entrada.HouveViolencia == true ? Texto(entrada.TipoViolencia) : null,
                TipoParto = ehPuerpera ? Texto(entrada.TipoParto) : null,
                RiscoGestacional = ehPuerpera ? SimNaoOuNull(entrada.RiscoGestacional) : null,
                QualRisco = ehPuerpera && entrada.RiscoGestacional == true ? Texto(entrada.QualRisco) : null,

                // sempre
                CasoCritico = SimNao(casoCritico),
                QualCasoCritico = qualCaso,
                Observacoes = UmaLinha(observacoes)
            };

            // Situação Atual
            var nova = new GestanteAtual
            {
                Chave = chave,
                Cns = cns,
                Cpf = cpf,
                Gestante = Texto(entrada.Gestante),
                Enfermeira = entrada.Enfermeira,
                Teleoperador = Texto(entrada.Teleoperador),
                Classificacao = conseguiu ? entrada.Classificacao : null,
                ConseguiuContato = SimNao(conseguiu),
                DataContato = contato,
                IgSemanas = ig is null ? null : (byte)ig.Value.Semanas,
                IgDias = ig is null ? null : (byte)ig.Value.Dias,
                DataRetorno = retorno,
                AltoRisco = null,                 
                CasoCritico = SimNao(casoCritico),
                QualCasoCritico = qualCaso,
                UltimaObservacao = UmaLinha(observacaoComCaso),
                UltimoFormulario = NomeFormulario,
                DataHoraModificacao = agora
            };
            // Gravar juntas as solicitações e as regras do Merge

            GravacaoGestante.Gravar(_context, historico, nova);

            resultado.Chave = chave;
            resultado.DataRetorno = retorno;
            return resultado;
        }

        private static List<string> Validar(FinalizarAtendimentoEntrada e)
        {
            var erros = new List<string>();

            if (string.IsNullOrWhiteSpace(e.Enfermeira))
                erros.Add("Escolha uma Enfermeira.");
            if (string.IsNullOrWhiteSpace(e.Teleoperador))
                erros.Add("Escolha o Teleoperador ou nenhum (NÃO DEIXE VAZIO).");
            // Verificar se precisa adicionar nome como chave
            var cns = (e.Cns ?? "").Trim();
            var cpf = (e.Cpf ?? "").Trim();
            if (cns != "" && !RegrasGestante.CnsValido(cns))
                erros.Add("O CNS precisa ter EXATAMENTE 15 Números ou deixe vazio e informe o CPF");
            if (cpf != "" && !RegrasGestante.CpfValido(cpf))
                erros.Add("CPF Inválido: confira se tem 11 Números");
            if (cns == "" && cpf == "")
                erros.Add("Informe o CNS ou o CPF da Gestante");

            if ((e.Gestante ?? "").Trim().Length > 255)
                erros.Add("O nome da gestante passou de 255 digitos");
            if ((e.Numero ?? "").Trim().Length > 50)
                erros.Add("O número do contato passou de 50 caracteres (padrãro de 9 a 11 digitos)");

            if (e.DataContato is null)
                erros.Add("Informe a data do contato.");
            else if (e.DataContato > DateOnly.FromDateTime(DateTime.Now))
                erros.Add("A data do contato não pode ser depois de hoje!");

            // Alterara para a aplicação do pedro
            if (e.CasoCritico is null)
                erros.Add("Responda se é Caso Crítico.");
            else if (e.CasoCritico == true && string.IsNullOrWhiteSpace(e.QualCasoCritico))
                erros.Add("Informe qual é o Caso Crítico.");
            else if (e.CasoCritico == true && e.QualCasoCritico!.Trim().Length > 1000)
                erros.Add("A descrição do Caso Crítico passou de 1000 (mil) letras. \nÉ apenas uma breve descrição para bater o olho e ver o Caso Crítico!");

            if (e.ConseguiuContato is null)
            {
                erros.Add("Responda se conseguiu o contato.");
            }
            else if (e.ConseguiuContato == false)
            {
                if (string.IsNullOrWhiteSpace(e.MotivoSemContato))
                    erros.Add("Escolha o motivo de não ter conseguido o contato.");
            }
            else   // conseguiu o contato
            {
                if (!Classificacoes.Contains(e.Classificacao))
                    erros.Add("Escolha a classificação (Gestante, Puérpera ou Aborto).");

                if (e.Classificacao == "Gestante")
                {
                    if (!string.IsNullOrWhiteSpace(e.Ig) && RegrasGestante.LerIg(e.Ig) is null)
                        erros.Add("Idade gestacional deve estar no formato semanas+dias, ex.: 30+2 (semanas 0 a 45, dias 0 a 6).");
                    if (e.DataRetorno is not null && e.DataContato is not null && e.DataRetorno < e.DataContato)
                        erros.Add("A data de retorno não pode ser antes da data do contato.");
                }

                if (e.Classificacao == "Puérpera")
                {
                    if (e.LocalParto == "Outro" && string.IsNullOrWhiteSpace(e.OutroLocal))
                        erros.Add("Informe qual foi o outro local do parto.");
                    else if (e.LocalParto == "Outro" && e.OutroLocal!.Trim().Length > 500)
                        erros.Add("O outro local do parto passou de 500 letras.");

                    if (e.HouveViolencia == true && string.IsNullOrWhiteSpace(e.TipoViolencia))
                        erros.Add("Escolha qual foi a violência.");

                    if (e.RiscoGestacional == true && string.IsNullOrWhiteSpace(e.QualRisco))
                        erros.Add("Informe qual foi o risco gestacional.");
                    else if (e.RiscoGestacional == true && e.QualRisco!.Trim().Length > 500)
                        erros.Add("A descrição do risco gestacional passou de 500 letras.");
                }
            }

            return erros;
        }

        // texto digitado: tira os espaços das pontas; vazio vira null
        private static string? Texto(string? texto) => VazioParaNull(texto?.Trim());
    }


}
    

