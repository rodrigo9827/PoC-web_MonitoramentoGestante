// sou o cara que confere o valor e devolve a resposta
namespace MonitoramentoGestante.Server.Regras
{
    // Regras do atendimento
    public static class RegrasGestante
    {
        public static string SoDigitos(string? texto)
        {// Salvar apenas os dígitos do texto
            return new string((texto ?? "").Where(c => c >= '0' && c <= '9').ToArray());
        }
        // Cns Válido só aceita 15 números e apenas digitos de 0 - 9
        public static bool CnsValido(string? cns)
        {
            var texto = (cns ?? "").Trim();
            return texto.Length == 15 && SoDigitos(texto).Length == 15;
        }
        // Cpf só é válido com 11 Números
        public static bool CpfValido(string? cpf)
        {
            var d = SoDigitos(cpf);
            if (d.Length != 11) return false;
            if (d.Distinct().Count() == 1) return false;

            for (int tamanho = 9; tamanho <= 10; tamanho++)
            {
                int soma = 0;
                for (int i = 0; i < tamanho; i++)
                {
                    soma += (d[i] - '0') * (tamanho + 1 - i);
                }
                int digito = soma * 10 % 11 % 10;
                if (digito != d[tamanho] - '0') return false;
            }
            return true;
        }
        // Chave para gestante se não tiver CNS usar o cpf
        public static string? Chave(string? cns, string? cpf)
        {
            if (CnsValido(cns)) return cns!.Trim();
            if (CpfValido(cpf)) return "CPF " + SoDigitos(cpf);
            return null;
        }

        // Lê a idade gestacional digitada: "ss+dd" (ex.: 30+2) ou só "ss" (dias = 0)
        // semanas de 0 - 45, dias de 0 - 6, só algarismos, no máximo 2 em cada parte
        // "30+2" vira os números (30, 2); fora do formato devolve null
        public static (int Semanas, int Dias)? LerIg(string? texto)
        {
            var partes = (texto ?? "").Replace(" ", "").Split('+');
            if (partes.Length > 2) return null;

            foreach (var parte in partes)
            {
                bool soAlgarismos = parte.Length > 0 && SoDigitos(parte).Length == parte.Length;
                if (!soAlgarismos || parte.Length > 2) return null;
            }

            int semanas = int.Parse(partes[0]);
            int dias = partes.Length == 2 ? int.Parse(partes[1]) : 0;
            if (semanas > 45 || dias > 6) return null;
            return (semanas, dias);
        }
        // Aqui é a formatação só fala onde vão os dias e as semanas
        public static string FormatarIg(int semanas, int dias)
        {
            return $"{semanas:00}s {dias:00}d";
        }
        // Regra das datas do tempo de ligação.
        public static DateOnly CalcularRetorno(DateOnly contato, int semanas, int dias)
        {
            int total = semanas * 7 + dias;
            int somar;
            if (total < 196) somar = 28;
            else if (total < 238) somar = 14;
            else if (total < 273) somar = 7;
            else if (total < 287) somar = 3;
            else somar = 1;
            return contato.AddDays(somar);
        }
    }
}