using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MonitoramentoGestante.Server.Models;
using MonitoramentoGestante.Server.Regras;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
// Eu sou o cara que controla o Chrome com o e-Saúde (a classe testada no projeto AutomacaoTeste).
// Abro um Chrome limpo (não guardo cookies nem perfil), faço o login sozinho com o usuário e a
// senha dos Segredos do Usuário (secrets.json) e, a cada pedido, clico em Pacientes, marco
// "todas as unidades", busco pelo CNS (ou CPF), abro a gestante e devolvo o LINK da ficha dela
// (.../app/person/ID). Faço uma busca de cada vez, em 2º plano, e conto em que pé está no Status.
// O log NUNCA tem CNS, CPF, nome, usuário nem senha: só os passos.
namespace MonitoramentoGestante.Server.Services
{
    
    public class AutomacaoESaude : IAutomacaoESaude, IDisposable
    {
        // ================= CONFIGURAÇÕES (as mesmas da PoC) =================
        private const string UrlLogin = "https://e-saudesp-telemedicina.prefeitura.sp.gov.br/e-saudesp";

        private static readonly By SinalLogin = By.XPath("//app-nav//button[contains(., 'Início')]");
        private static readonly By MenuPacientes = By.XPath("//app-nav//button[contains(., 'Pacientes')]");
        private static readonly By OpcaoTodasUnidades = By.XPath("//mat-radio-button[contains(., 'Buscar em todas as unidades')]");
        private static readonly By OpcaoTodasUnidadesInput = By.XPath("//mat-radio-button[contains(., 'Buscar em todas as unidades')]//input");
        private static readonly By CampoCns = CampoPorTexto("CNS");
        private static readonly By CampoCpf = CampoPorTexto("CPF");
        private static readonly By BotaoFiltrar = By.XPath("//button[contains(., 'Filtrar')]");
        private static readonly By CartoesPaciente = By.XPath("//app-person//app-infinite-scroll//mk-card");
        private static readonly By Carregando = By.CssSelector("mat-spinner, mat-progress-spinner, mat-progress-bar");

        // ===== LOGIN AUTOMÁTICO (preencher depois de inspecionar a tela de login com o F12) =====
        // Deixe "" em qualquer um para NÃO automatizar o login (aí a automação espera você logar à mão).
        private const string XPathUsuario = "//*[@id=\"mat-input-0\"]";   
        private const string XPathSenha = "//app-login//input[@type='password']";   
        private const string XPathEntrar = "/html/body/app-root/mk-login-container/div/div[2]/app-login/form/mk-button/div/button/span[2]/div";   

        private static readonly TimeSpan TempoLogin = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan TempoTela = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan TempoCarregando = TimeSpan.FromSeconds(3);
        private static readonly TimeSpan Estabilidade = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan EsperaMesmoResultado = TimeSpan.FromSeconds(5);
        private const int Pausa = 1000;

        // acha, no cartão, os elementos com a "mãozinha" (os que abrem a paciente)
        private const string JsClicaveis = @"
            const cartao = arguments[0];
            const encontrados = [];
            for (const el of [cartao, ...cartao.querySelectorAll('*')]) {
                const estilo = getComputedStyle(el);
                if (estilo.cursor !== 'pointer') continue;
                if (el.offsetWidth === 0 || el.offsetHeight === 0) continue;
                if (el.closest('button, a')) continue;
                encontrados.push(el);
            }
            return encontrados;";

        // o campo pelo texto da tela (rótulo, placeholder ou aria-label), nunca pelo id (mat-input-0...)
        private static By CampoPorTexto(string texto) => By.XPath(
            $"(//mat-form-field[contains(., '{texto}')]//input" +
            $" | //input[contains(@placeholder, '{texto}') or contains(@aria-label, '{texto}')])[1]");

        // ================= ESTADO (guardado entre um clique e outro) =================
        private readonly ILogger<AutomacaoESaude> _logger;
        private readonly IConfiguration _config;                 
        private readonly SemaphoreSlim _trava = new(1, 1);       
        private readonly object _lockStatus = new();             
        private readonly StatusAutomacao _status = new();
        private ChromeDriver? _driver;

        private IWebDriver D => _driver!;

        public AutomacaoESaude(ILogger<AutomacaoESaude> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        // devolve uma CÓPIA do status (para a tela não mexer no de dentro)
        public StatusAutomacao Status
        {
            get
            {
                lock (_lockStatus)
                    return new StatusAutomacao { Estado = _status.Estado, Mensagem = _status.Mensagem, Link = _status.Link };
            }
        }

        private void DefinirStatus(string estado, string mensagem, string? link = null)
        {
            lock (_lockStatus)
            {
                _status.Estado = estado;
                _status.Mensagem = mensagem;
                _status.Link = link;
            }
        }

        // ================= PORTA DE ENTRADA (chamada pelo controller) =================
        public bool Iniciar(string? cns, string? cpf)
        {
            var cnsLimpo = (cns ?? "").Trim();
            var cpfLimpo = RegrasGestante.SoDigitos(cpf);
            if (cnsLimpo == "" && cpfLimpo == "") return true;     

            if (!_trava.Wait(0)) return false;                       

            DefinirStatus("buscando", "Procurando a gestante no e-Saúde...");
            _ = Task.Run(() => Executar(cnsLimpo, cpfLimpo));        
            return true;
        }

        private void Executar(string cns, string cpf)
        {
            try
            {
                if (!NavegadorAberto()) AbrirEEsperarLogin();
                Registrar("===== nova busca =====");

                var link = Buscar(cns, cpf);
                if (link is not null)
                    DefinirStatus("pronta", "Gestante encontrada. Abrindo a ficha...", link);
                else
                    DefinirStatus("erro", "Encontrei a gestante, mas não consegui abrir a ficha. Tente de novo.");
            }
            catch (Exception erro)
            {
                // só o tipo do erro: nunca mostra CNS, CPF ou nome da paciente
                DefinirStatus("erro", $"Erro na automação ({erro.GetType().Name}). Clique na gestante de novo.");
                Registrar($"ERRO: {erro.GetType().Name}");
            }
            finally
            {
                FecharNavegador();
                _trava.Release();
            }
        }

        // ================= ABRIR O CHROME E LOGAR =================
        private void AbrirEEsperarLogin()
        {
            var opcoes = new ChromeOptions();
            // TODO: --no-sandbox só é preciso sob a conta Administrador; numa conta normal, remover.
            opcoes.AddArgument("--no-sandbox");
            opcoes.AddArgument("--no-first-run");
            opcoes.AddArgument("--no-default-browser-check");

            DefinirStatus("buscando", "Abrindo o e-Saúde no Chrome...");
            _driver = new ChromeDriver(opcoes);
            _driver.Manage().Window.Maximize();
            _driver.Navigate().GoToUrl(UrlLogin);
            EsperarTelaCarregar();

            // (dentro de uma mesma execução, o _driver é reaproveitado; se já estiver logado, segue)
            if (D.FindElements(SinalLogin).Any(e => e.Displayed))
            {
                Registrar("já estava logado");
                return;
            }

            // tenta logar sozinho (se os XPaths e as credenciais estiverem preenchidos)
            TentarLoginAutomatico();

            // confirma o login: o automático que acabou de enviar, OU alguém logando à mão
            Esperar(TempoTela).Until(d => d.FindElements(SinalLogin).Any(e => e.Displayed));
            EsperarTelaCarregar();
            Registrar("login confirmado");
        }

        // Preenche usuário e senha (dos Segredos) e envia. Se faltar XPath ou credencial, ou se falhar,
        // cai no login manual (a automação segue esperando alguém logar na janela do Chrome).
        private void TentarLoginAutomatico()
        {
            if (XPathUsuario == "" || XPathSenha == "")
            {
                DefinirStatus("buscando", "Faça o login no Chrome (até 5 minutos)...");
                Registrar("sem XPaths de login: login manual");
                return;
            }

            var usuario = _config["ESaude:Usuario"];
            var senha = _config["ESaude:Senha"];
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(senha))
            {
                DefinirStatus("buscando", "Faça o login no Chrome (até 5 minutos)...");
                Registrar("sem usuário/senha nos Segredos: login manual");
                return;
            }

            try
            {
                DefinirStatus("buscando", "Entrando no e-Saúde...");
                var campoUsuario = EsperarClicavel(By.XPath(XPathUsuario));
                Clicar(campoUsuario);
                campoUsuario.SendKeys(usuario);

                var campoSenha = D.FindElement(By.XPath(XPathSenha));
                campoSenha.SendKeys(senha);

                if (XPathEntrar != "")
                    Clicar(D.FindElement(By.XPath(XPathEntrar)));
                else
                    campoSenha.SendKeys(Keys.Enter);

                Registrar("login automático enviado");   
            }
            catch (WebDriverException)
            {
                // a tela de login pode estar diferente: deixa para o login manual
                DefinirStatus("buscando", "Não consegui logar sozinho. Faça o login no Chrome.");
                Registrar("login automático falhou: login manual");
            }
        }

        // ================= BUSCAR A GESTANTE E PEGAR O LINK (idêntico ao teste) =================
        private string? Buscar(string cns, string cpf)
        {
            Clicar(EsperarClicavel(MenuPacientes));
            EsperarTelaCarregar();
            Registrar("etapa 3: menu Pacientes clicado");

            var opcao = Esperar(TempoLogin).Until(d => d.FindElements(OpcaoTodasUnidadesInput).FirstOrDefault());
            if (opcao is not null && !opcao.Selected)
            {
                Clicar(D.FindElement(OpcaoTodasUnidades));
                EsperarTelaCarregar();
                Registrar("etapa 4: 'todas as unidades' marcado agora");
            }
            else
            {
                Registrar("etapa 4: 'todas as unidades' já estava marcado");
            }

            if (cns != "")
            {
                Registrar("busca pelo CNS");
                Preencher("CPF", CampoCpf, "", obrigatorio: false);
                Preencher("CNS", CampoCns, cns, obrigatorio: true);
            }
            else
            {
                Registrar("busca pelo CPF");
                Preencher("CNS", CampoCns, "", obrigatorio: false);
                Preencher("CPF", CampoCpf, cpf, obrigatorio: true);
            }
            Thread.Sleep(Pausa);

            var filtrar = EsperarClicavel(BotaoFiltrar);
            var antes = CartoesVisiveis();
            string? textoAntigo = antes.Count > 0 ? antes[0].Text : null;
            Clicar(filtrar);
            var momentoFiltro = DateTime.Now;
            Registrar($"etapa 7: Filtrar clicado ({antes.Count} cartão(ões) antes)");

            string? textoVisto = null;
            var desde = DateTime.MinValue;

            IWebElement? ResultadoPronto(IWebDriver d)
            {
                var cartoes = CartoesVisiveis();
                if (cartoes.Count == 0) { textoVisto = null; return null; }

                var primeiro = cartoes[0];
                string texto;
                try { texto = primeiro.Text; }
                catch (StaleElementReferenceException) { return null; }

                var agora = DateTime.Now;
                if (texto != textoVisto) { textoVisto = texto; desde = agora; return null; }
                if (agora - desde < Estabilidade) return null;
                if (textoAntigo is null || texto != textoAntigo) return primeiro;
                if (agora - momentoFiltro >= EsperaMesmoResultado) return primeiro;
                return null;
            }

            try
            {
                Esperar(TempoTela).Until(ResultadoPronto);
            }
            catch (WebDriverTimeoutException)
            {
                Registrar($"ERRO etapa 8: nenhum resultado estável em {TempoTela.TotalSeconds} s");
                return null;
            }
            EsperarTelaCarregar();
            var paciente = Esperar(TempoTela).Until(ResultadoPronto)!;
            RolarAte(paciente);
            Registrar("etapa 8: resultado estável");

            var enderecoAntes = D.Url;
            var abasAntes = D.WindowHandles.ToList();

            var resultadoJs = ((IJavaScriptExecutor)D).ExecuteScript(JsClicaveis, paciente);
            var clicaveis = (resultadoJs as IEnumerable<object>)?.OfType<IWebElement>().ToList() ?? [];
            Registrar($"etapa 8: {clicaveis.Count} elemento(s) com a 'mãozinha' no cartão");

            foreach (var el in clicaveis)
            {
                try
                {
                    RolarAte(el);
                    new Actions(D).MoveToElement(el).Click().Perform();
                    EsperarTelaCarregar();
                }
                catch (StaleElementReferenceException) { }

                var link = LinkDaFicha(enderecoAntes, abasAntes);
                if (link is not null) { Registrar("etapa 8: ficha aberta"); return link; }
            }

            try
            {
                RolarAte(paciente);
                new Actions(D).MoveToElement(paciente).Click().Perform();
                EsperarTelaCarregar();
            }
            catch (WebDriverException) { }

            var ultimo = LinkDaFicha(enderecoAntes, abasAntes);
            Registrar(ultimo is not null ? "etapa 8: ficha aberta (cartão)" : "etapa 8: não abriu a ficha");
            return ultimo;
        }

        private string? LinkDaFicha(string enderecoAntes, List<string> abasAntes)
        {
            var novas = D.WindowHandles.Except(abasAntes).ToList();
            if (novas.Count > 0)
            {
                D.SwitchTo().Window(novas[0]);
                EsperarTelaCarregar();
                return D.Url;
            }
            return D.Url != enderecoAntes ? D.Url : null;
        }

        // ================= FERRAMENTAS (idênticas ao teste) =================
        private WebDriverWait Esperar(TimeSpan tempo)
        {
            var espera = new WebDriverWait(D, tempo);
            espera.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
            return espera;
        }

        private IWebElement EsperarClicavel(By localizador) =>
            Esperar(TempoTela).Until(d => d.FindElements(localizador).FirstOrDefault(e => e.Displayed && e.Enabled))!;

        private bool NavegadorAberto()
        {
            if (_driver is null) return false;
            try { _ = _driver.Title; return true; }
            catch (WebDriverException)
            {
                try { _driver.Quit(); } catch (WebDriverException) { }
                _driver = null;
                return false;
            }
        }

        private void EsperarTelaCarregar()
        {
            Esperar(TempoTela).Until(d =>
                ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState")?.ToString() == "complete");

            try
            {
                Esperar(TempoCarregando).Until(d => !d.FindElements(Carregando).Any(e => e.Displayed));
            }
            catch (WebDriverTimeoutException) { }

            Thread.Sleep(Pausa);
        }

        private void RolarAte(IWebElement elemento)
        {
            ((IJavaScriptExecutor)D).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", elemento);
            Thread.Sleep(300);
        }

        private void Clicar(IWebElement elemento)
        {
            RolarAte(elemento);
            try { elemento.Click(); }
            catch (ElementClickInterceptedException)
            {
                ((IJavaScriptExecutor)D).ExecuteScript("arguments[0].click();", elemento);
            }
        }

        private List<IWebElement> CartoesVisiveis()
        {
            var visiveis = new List<IWebElement>();
            foreach (var cartao in D.FindElements(CartoesPaciente))
            {
                try { if (cartao.Displayed) visiveis.Add(cartao); }
                catch (StaleElementReferenceException) { }
            }
            return visiveis;
        }

        private void Preencher(string nomeCampo, By localizador, string texto, bool obrigatorio)
        {
            IWebElement campo;
            if (obrigatorio)
            {
                try { campo = EsperarClicavel(localizador); }
                catch (WebDriverTimeoutException)
                {
                    Registrar($"ERRO: campo {nomeCampo} não encontrado");
                    throw;
                }
            }
            else
            {
                var visivel = D.FindElements(localizador).FirstOrDefault(c => c.Displayed);
                if (visivel is null) { Registrar($"campo {nomeCampo} não existe nesta tela"); return; }
                campo = visivel;
            }

            var valorAtual = campo.GetDomProperty("value") ?? "";
            if (texto == "" && valorAtual.Trim() == "") return;

            Clicar(campo);
            campo.SendKeys(Keys.Control + "a");
            if (texto != "") { campo.SendKeys(texto); Registrar($"campo {nomeCampo} preenchido"); }
            else { campo.SendKeys(Keys.Delete); Registrar($"campo {nomeCampo} limpo"); }
        }

        // o log vai para o console do servidor
        private void Registrar(string passo) => _logger.LogInformation("Automação: {Passo}", passo);

        // fecha o Chrome da automação (a próxima busca abre de novo)
        private void FecharNavegador()
        {
            try { _driver?.Quit(); } catch (WebDriverException) { }
            _driver = null;
        }

        // quando o servidor é desligado, fecho o Chrome da automação
        public void Dispose()
        {
            try { _driver?.Quit(); } catch (WebDriverException) { }
            _driver = null;
            _trava.Dispose();
        }
    
    }
}