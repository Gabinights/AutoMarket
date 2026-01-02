namespace AutoMarket.Services.Interfaces
{
    /// <summary>
    /// Interface para gerenciar upload e processamento de ficheiros.
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Faz upload de um ficheiro único.
        /// </summary>
        /// <param name="file">Ficheiro a fazer upload</param>
        /// <param name="uploadFolder">Pasta de destino (relativa a wwwroot)</param>
        /// <returns>Nome do ficheiro guardado</returns>
        Task<string> UploadFileAsync(IFormFile file, string uploadFolder);

        /// <summary>
        /// Faz upload de múltiplos ficheiros.
        /// </summary>
        /// <param name="files">Lista de ficheiros a fazer upload</param>
        /// <param name="uploadFolder">Pasta de destino (relativa a wwwroot)</param>
        /// <returns>Lista de nomes de ficheiros guardados</returns>
        Task<List<string>> UploadMultipleFilesAsync(List<IFormFile> files, string uploadFolder);

        /// <summary>
        /// Valida um ficheiro.
        /// </summary>
        /// <param name="file">Ficheiro a validar</param>
        /// <param name="maxSizeInBytes">Tamanho máximo em bytes</param>
        /// <param name="allowedExtensions">Extensões permitidas (ex: .jpg, .png)</param>
        /// <returns>True se válido, False caso contrário</returns>
        bool ValidateFile(IFormFile file, long maxSizeInBytes, string[] allowedExtensions);

        /// <summary>
        /// Obtém mensagem de erro de validação de ficheiro.
        /// </summary>
        /// <param name="file">Ficheiro a validar</param>
        /// <param name="maxSizeInBytes">Tamanho máximo em bytes</param>
        /// <param name="allowedExtensions">Extensões permitidas</param>
        /// <returns>Mensagem de erro ou string vazia se válido</returns>
        string GetFileValidationError(IFormFile file, long maxSizeInBytes, string[] allowedExtensions);

        /// <summary>
        /// Apaga um ficheiro.
        /// </summary>
        /// <param name="filePath">Caminho relativo do ficheiro (relativo a wwwroot)</param>
        /// <returns>True se apagado com sucesso, False caso contrário</returns>
        Task<bool> DeleteFileAsync(string filePath);
    }
}
