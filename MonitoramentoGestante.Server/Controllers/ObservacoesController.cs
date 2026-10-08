// Eu sou o cara que atende o GET /api/observacoes?chave=...: confiro se veio a chave,
// peço as observações ao ObservacaoService e devolvo a lista em JSON.
// Se o banco falhar, registro só o TIPO do erro (nunca dados de paciente).

using Microsoft.AspNetCore.Mvc;
using MonitoramentoGestante.Server.Models;
using MonitoramentoGestante.Server.Services;

namespace MonitoramentoGestante.Server.Controllers;

[ApiController]
[Route("api/observacoes")]
public class ObservacoesController : ControllerBase
{
    private readonly IObservacaoService _service;
    private readonly ILogger<ObservacoesController> _logger;
    public ObservacoesController(IObservacaoService service, ILogger<ObservacoesController> logger)
    {
        _service = service;
        _logger = logger;
    }
    [HttpGet]
    public ActionResult<IEnumerable<Observacao>> Listar([FromQuery] string? chave)
    {
        if (string.IsNullOrWhiteSpace(chave))
            return BadRequest("Informe a chave da gestante (CNS, ou CPF quando não houver CNS).");
        try
        {
            return Ok(_service.ListarPorGestante(chave));
        }
        catch (Exception erro)
        {
            _logger.LogError("Falha ao listar observaçoes: {tipo}", erro.GetBaseException().GetType().Name);
            return StatusCode(500, "Não foi possível buscar as observações.");
        }
    }
}