using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Utils
{
    public static class NifValidator
    {
        public static bool IsValid(string nif)
        {
            if (string.IsNullOrWhiteSpace(nif) || nif.Length != 9 || !long.TryParse(nif, out _))
                return false;

            var total = 0;
            for (var i = 0; i < 8; i++)
            {
                total += int.Parse(nif[i].ToString()) * (9 - i);
            }

            var resto = total % 11;
            var checkDigit = (resto < 2) ? 0 : 11 - resto;

            return checkDigit == int.Parse(nif[8].ToString());
        }

        public static bool IsEmpresa(string nif)
        {
            if (!IsValid(nif)) return false;
            char prefix = nif[0];
            // 5: Pessoas Coletivas (Sociedades)
            // 6: Administração Pública
            // 9: Entidades Equiparadas / Irregulares (ex: Condomínios)
            // Nota: Existem outros (70, 71, etc.) mas são casos muito específicos de Heranças/Não Residentes
            return prefix == '5' || prefix == '6' || prefix == '9';
        }

        public static bool IsParticular(string nif)
        {
            if (!IsValid(nif)) return false;
            char prefix = nif[0];
            // 1, 2, 3: Pessoas Singulares (o 3 já está a ser atribuído)
            // 45: Pessoas Singulares Não Residentes (NIF provisório em alguns casos, mas raro ser usado em registo normal)
            return prefix == '1' || prefix == '2' || prefix == '3';
        }
    }
}