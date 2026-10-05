using MonitoramentoGestante.Server.Regras;
namespace MonitoramentoGestante.Tests
{
    public class RegrasGestanteTests
    {
        [Theory]
        [InlineData("123.456-7", "1234567")]
        [InlineData("30+2", "302")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void SoDigitos_GuardaSoOsNumeros(string? texto, string esperado)
        {
            Assert.Equal(esperado, RegrasGestante.SoDigitos(texto));
        }

        [Theory]
        [InlineData("123456789012345", true)]
        [InlineData(" 123456789012345 ", true)]
        [InlineData("12345678901234", false)]
        [InlineData("12345678901234+", false)]
        [InlineData("12345678901234a", false)]
        [InlineData(null, false)]
        public void CnsValido_SoAceita15Numeros(string? cns, bool esperado)
        {
            Assert.Equal(esperado, RegrasGestante.CnsValido(cns));
        }

        [Theory]
        [InlineData("52998224725", true)]
        [InlineData("529.982.247-25", true)]
        [InlineData("52998224724", false)]
        [InlineData("11111111111", false)]
        [InlineData("123", false)]
        [InlineData(null, false)]
        public void CpfValido_ConfereOsDigitosVerificadores(string? cpf, bool esperado)
        {
            Assert.Equal(esperado, RegrasGestante.CpfValido(cpf));
        }

        [Theory]
        [InlineData("123456789012345", "52998224725", "123456789012345")]
        [InlineData(null, "529.982.247-25", "CPF 52998224725")]
        [InlineData("123", "52998224725", "CPF 52998224725")]
        [InlineData(null, null, null)]
        public void Chave_UsaCnsOuCpf(string? cns,  string? cpf, string? esperado)
        {
            Assert.Equal(esperado, RegrasGestante.Chave(cns, cpf));
        }

        [Theory]
        [InlineData("30+2", 30, 2)]
        [InlineData(" 30+2 ", 30, 2)]
        [InlineData("0+0", 0, 0)]
        [InlineData("45+6", 45, 6)]
        public void LerIg_FormatoCerto(string texto, int semanas, int dias)
        {
            var ig = RegrasGestante.LerIg(texto);
            Assert.NotNull(ig);
            Assert.Equal(semanas, ig.Value.Semanas);
            Assert.Equal(dias, ig.Value.Dias);
        }

        [Theory]
        [InlineData("30")]        
        [InlineData("30+7")]      
        [InlineData("46+0")]      
        [InlineData("abc+2")]
        [InlineData("30+2+1")]
        [InlineData("")]
        [InlineData(null)]
        public void LerIg_FormatoErrado_DevolveNull(string? texto)
        {
            Assert.Null(RegrasGestante.LerIg(texto));
        }

        [Theory]
        [InlineData(30, 2, "30s 02d")]
        [InlineData(8, 2, "08s 02d")]
        [InlineData(0, 0, "00s 00d")]
        public void FormatarIg_DoisAlgarismos(int semanas, int dias, string esperado)
        {
            Assert.Equal(esperado, RegrasGestante.FormatarIg(semanas, dias));
        }

        [Theory]
        [InlineData(27, 6, 28)]
        [InlineData(28, 0, 14)]     
        [InlineData(33, 6, 14)]     
        [InlineData(34, 0, 7)]      
        [InlineData(38, 6, 7)]      
        [InlineData(39, 0, 3)]      
        [InlineData(40, 6, 3)]      
        [InlineData(41, 0, 1)]      
        public void CalcularRetorno_SegueAsFaixasDaView(int semanas, int dias, int diasSomados)
        {
            var contato = new DateOnly(2026, 1, 1);

            var retorno = RegrasGestante.CalcularRetorno(contato, semanas, dias);

            Assert.Equal(contato.AddDays(diasSomados), retorno);
        }
    }
}