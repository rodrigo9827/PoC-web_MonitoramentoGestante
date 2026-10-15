using Microsoft.AspNetCore.Mvc;
using MonitoramentoGestante.Server.Models;
using MonitoramentoGestante.Server.Services;
// Eu sou o cara que atende a tela na automação do e-Saúde:
//   POST /api/automacao/buscar -> começa a buscar a gestante clicada (202),
//                                 ou avisa que ainda está na anterior (409),
//                                 ou que não tem CNS/CPF (400)
//   GET  /api/automacao/status -> conta em que pé está a busca (a tela pergunta de tempos em tempos)
namespace MonitoramentoGestante.Server.Controllers
{
    
    [ApiController]
    [Route("api/automacao")]
    public class AutomacaoController : ControllerBase
    {
        private readonly IAutomacaoESaude _automacao;
        public AutomacaoController(IAutomacaoESaude automacao)
        {
            _automacao = automacao;
        }

        [HttpPost("buscar")]
        public ActionResult<StatusAutomacao> Buscar([FromBody] PedidoAutomacao pedido)
        {
            if (string.IsNullOrWhiteSpace(pedido.Cns) && string.IsNullOrWhiteSpace(pedido.Cpf))
                return BadRequest(new StatusAutomacao
                {
                    Estado = "erro",
                    Mensagem = "Essa Gestante não tem CNS nem CPF, não da para Buscar automaticamente no e-Saude"
                });
            if (!_automacao.Iniciar(pedido.Cns, pedido.Cpf))
                return Conflict(new StatusAutomacao
                {
                    Estado = "Buscando",
                    Mensagem = "Aguarde, ainda buscando a gestante anterior."
                });
            return Accepted(_automacao.Status);
        }
        [HttpGet("status")]
        public ActionResult<StatusAutomacao> LerStatus() => Ok(_automacao.Status);
    }
}