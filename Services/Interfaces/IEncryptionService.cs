namespace AutoMarket.Services.Interfaces
{
    /// <summary>
    /// Interface para serviços de encriptação de dados sensíveis (RGPD compliance).
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encripta um valor sensível.
        /// </summary>
        string Encrypt(string plainText);

        /// <summary>
        /// Desencripta um valor encriptado.
        /// </summary>
        string Decrypt(string cipherText);

        /// <summary>
        /// Verifica se uma string está encriptada.
        /// </summary>
        bool IsEncrypted(string value);
    }
}


