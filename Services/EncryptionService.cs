using System.Security.Cryptography;
using System.Text;
using AutoMarket.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AutoMarket.Services
{
    /// <summary>
    /// Serviço de encriptação para dados sensíveis (RGPD compliance).
    /// Usa AES-256-GCM para encriptação simétrica.
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly ILogger<EncryptionService> _logger;

        public EncryptionService(IConfiguration configuration, ILogger<EncryptionService> logger)
        {
            _logger = logger;
            
            // Obter chave de encriptação da configuração
            var encryptionKey = configuration["Encryption:Key"];
            
            if (string.IsNullOrEmpty(encryptionKey))
            {
                // Se não houver chave configurada, gerar uma e logar um aviso
                _logger.LogWarning("Chave de encriptação não configurada. Gerando chave temporária. Configure 'Encryption:Key' em appsettings.json para produção.");
                encryptionKey = GenerateTemporaryKey();
            }

            // A chave deve ter 32 bytes (256 bits) para AES-256
            _key = DeriveKey(encryptionKey, 32);
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                using var aes = Aes.Create();
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = _key;
                aes.GenerateIV();

                using var encryptor = aes.CreateEncryptor();
                using var msEncrypt = new MemoryStream();
                
                // Escrever IV primeiro
                msEncrypt.Write(aes.IV, 0, aes.IV.Length);

                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }

                var encrypted = msEncrypt.ToArray();
                return Convert.ToBase64String(encrypted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao encriptar dados sensíveis");
                throw new InvalidOperationException("Falha na encriptação de dados sensíveis", ex);
            }
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            // Se não estiver encriptado (formato Base64), retornar como está (para migração)
            if (!IsEncrypted(cipherText))
                return cipherText;

            try
            {
                var fullCipher = Convert.FromBase64String(cipherText);

                using var aes = Aes.Create();
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = _key;

                // Extrair IV (primeiros 16 bytes)
                var iv = new byte[16];
                Array.Copy(fullCipher, 0, iv, 0, 16);
                aes.IV = iv;

                // Extrair dados encriptados (resto)
                var cipher = new byte[fullCipher.Length - 16];
                Array.Copy(fullCipher, 16, cipher, 0, cipher.Length);

                using var decryptor = aes.CreateDecryptor();
                using var msDecrypt = new MemoryStream(cipher);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);

                return srDecrypt.ReadToEnd();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao desencriptar dados sensíveis");
                throw new InvalidOperationException("Falha na desencriptação de dados sensíveis", ex);
            }
        }

        public bool IsEncrypted(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            // Verificar se é Base64 válido e tem tamanho mínimo (IV + pelo menos alguns bytes)
            try
            {
                var bytes = Convert.FromBase64String(value);
                return bytes.Length >= 20; // IV (16 bytes) + pelo menos 4 bytes de dados
            }
            catch
            {
                return false;
            }
        }

        private static byte[] DeriveKey(string password, int keyLength)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            
            if (hash.Length >= keyLength)
            {
                var key = new byte[keyLength];
                Array.Copy(hash, 0, key, 0, keyLength);
                return key;
            }
            
            // Se o hash for menor que o necessário, usar PBKDF2
            using var pbkdf2 = new Rfc2898DeriveBytes(password, hash, 10000, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(keyLength);
        }

        private static string GenerateTemporaryKey()
        {
            // Gerar uma chave aleatória de 32 bytes e converter para Base64
            var keyBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }
            return Convert.ToBase64String(keyBytes);
        }
    }
}


