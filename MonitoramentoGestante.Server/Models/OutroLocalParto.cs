/*
  eu sou o cara que representa uma opção de "outro local de parto"
  (fora do município, parto domiciliar...), lida da Core.Parametro
  com a chave OutrosLocaisDePartoRN
*/

namespace MonitoramentoGestante.Server.Models
{
    public class OutroLocalParto
    {
        public int Id { get; set; }                      
        public string Nome { get; set; } = string.Empty;  
    }
}