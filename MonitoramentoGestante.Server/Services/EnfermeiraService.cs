namespace MonitoramentoGestante.Server.Services
{
    public class EnfermeiraService : IEnfermeiraService
    {
        public IEnumerable<string> Listar()
        {
            return ["Enfermeira Teste 1","Enfermeira Teste 2","Enfermeira Teste 3"];
        }
    }
}
