using AutoMarket.Services.Interfaces;

namespace AutoMarket.Services
{
    /// <summary>
    /// Helper estático para acessar o serviço de encriptação do NIF.
    /// Usado pela propriedade NIF em Utilizador para encriptação/desencriptação automática.
    /// </summary>
    public static class NifEncryptionHelper
    {
        private static IEncryptionService? _encryptionService;
        private static readonly object _lock = new object();

        /// <summary>
        /// Inicializa o helper com o serviço de encriptação.
        /// Deve ser chamado no Program.cs durante a configuração.
        /// </summary>
        public static void Initialize(IEncryptionService encryptionService)
        {
            lock (_lock)
            {
                _encryptionService = encryptionService;
            }
        }

        /// <summary>
        /// Encripta o NIF antes de guardar na base de dados.
        /// </summary>
        public static string? EncryptNif(string? nif)
        {
            if (string.IsNullOrEmpty(nif))
                return nif;

            if (_encryptionService == null)
            {
                // Se o serviço não estiver inicializado, retornar sem encriptar
                // (útil durante migrações ou inicialização)
                return nif;
            }

            return _encryptionService.Encrypt(nif);
        }

        /// <summary>
        /// Desencripta o NIF ao ler da base de dados.
        /// </summary>
        public static string? DecryptNif(string? encryptedNif)
        {
            if (string.IsNullOrEmpty(encryptedNif))
                return encryptedNif;

            if (_encryptionService == null)
            {
                // Se o serviço não estiver inicializado, retornar como está
                return encryptedNif;
            }

            // Se não estiver encriptado (dados antigos), retornar como está
            if (!_encryptionService.IsEncrypted(encryptedNif))
                return encryptedNif;

            try
            {
                return _encryptionService.Decrypt(encryptedNif);
            }
            catch
            {
                // Se falhar a desencriptação, retornar como está (pode ser dado antigo)
                return encryptedNif;
            }
        }
    }
}


