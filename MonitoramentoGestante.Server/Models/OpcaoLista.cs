/*
  eu sou o cara que leva pra tela só o Id e o Nome de uma opção de lista
  (local de parto, teleoperador...), sem expor dado interno como CNES e CMES
*/

namespace MonitoramentoGestante.Server.Models
{
    public class OpcaoLista
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }
}