using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Attributes
{
    public class NifPortuguesAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // 1. Se for null ou vazio, assumimos Sucesso. 
            // Porquê? Porque a responsabilidade de ser "Obrigatório" é do atributo [Required].
            // Este atributo só valida: "SE tiver dados, eles têm de ser um NIF válido".
            string nif = value as string;
            if (string.IsNullOrWhiteSpace(nif))
            {
                return ValidationResult.Success;
            }

            // 2. Limpar espaços
            nif = nif.Trim();

            // 3. Verifica se tem 9 dígitos e se é numérico
            if (nif.Length != 9 || !long.TryParse(nif, out _))
            {
                return new ValidationResult("O NIF deve conter exatamente 9 dígitos numéricos.");
            }

            // 4. Algoritmo de Validação (Módulo 11)
            // Lógica oficial da Autoridade Tributária
            int total = 0;
            for (int i = 0; i < 8; i++)
            {
                total += int.Parse(nif[i].ToString()) * (9 - i);
            }

            int resto = total % 11;
            int digitoControloCalculado = (resto < 2) ? 0 : 11 - resto;
            int digitoControloInformado = int.Parse(nif[8].ToString());

            if (digitoControloCalculado != digitoControloInformado)
            {
                return new ValidationResult("O NIF introduzido é inválido.");
            }

            return ValidationResult.Success;
        }
    }
}