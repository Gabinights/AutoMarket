using AutoMarket.Services.Interfaces;

namespace AutoMarket.Services
{
    /// <summary>
    /// Serviço para gerenciar upload e processamento de ficheiros.
    /// </summary>
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<FileService> _logger;

        public FileService(IWebHostEnvironment webHostEnvironment, ILogger<FileService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        /// <summary>
        /// Faz upload de um ficheiro único.
        /// </summary>
        public async Task<string> UploadFileAsync(IFormFile file, string uploadFolder)
        {
            try
            {
                // Validação
                if (file == null || file.Length == 0)
                    throw new ArgumentException("Ficheiro inválido ou vazio.");

                // Validar extensão
                var extension = Path.GetExtension(file.FileName).ToLower();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!allowedExtensions.Contains(extension))
                    throw new ArgumentException($"Extensão '{extension}' não permitida. Use: {string.Join(", ", allowedExtensions)}");

                // Validar tamanho (10 MB)
                const long maxFileSize = 10 * 1024 * 1024;
                if (file.Length > maxFileSize)
                    throw new ArgumentException($"Ficheiro demasiado grande. Máximo: 10 MB");

                // Criar pasta se não existir
                var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, uploadFolder);
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                // Gerar nome único com GUID
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadPath, fileName);

                // Guardar ficheiro
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("Ficheiro guardado com sucesso: {FileName}", fileName);
                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fazer upload do ficheiro: {FileName}", file?.FileName);
                throw;
            }
        }

        /// <summary>
        /// Faz upload de múltiplos ficheiros.
        /// </summary>
        public async Task<List<string>> UploadMultipleFilesAsync(List<IFormFile> files, string uploadFolder)
        {
            var uploadedFiles = new List<string>();

            // Validar quantidade
            if (files == null || files.Count == 0)
                throw new ArgumentException("Nenhum ficheiro fornecido.");

            const int maxFilesPerCar = 10;
            if (files.Count > maxFilesPerCar)
                throw new ArgumentException($"Máximo de {maxFilesPerCar} ficheiros permitidos.");

            // Validar tamanho total
            const long maxTotalSize = 50 * 1024 * 1024; // 50 MB
            var totalSize = files.Sum(f => f.Length);

            if (totalSize > maxTotalSize)
                throw new ArgumentException($"Tamanho total demasiado grande. Máximo: 50 MB");

            // Fazer upload de cada ficheiro
            foreach (var file in files)
            {
                try
                {
                    var fileName = await UploadFileAsync(file, uploadFolder);
                    uploadedFiles.Add(fileName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao fazer upload de um ficheiro. Continuando com os restantes.");
                    // Continuar com os próximos ficheiros
                }
            }

            return uploadedFiles;
        }

        /// <summary>
        /// Valida um ficheiro.
        /// </summary>
        public bool ValidateFile(IFormFile file, long maxSizeInBytes, string[] allowedExtensions)
        {
            if (file == null || file.Length == 0)
                return false;

            // Validar extensão
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return false;

            // Validar tamanho
            if (file.Length > maxSizeInBytes)
                return false;

            return true;
        }

        /// <summary>
        /// Obtém mensagem de erro de validação de ficheiro.
        /// </summary>
        public string GetFileValidationError(IFormFile file, long maxSizeInBytes, string[] allowedExtensions)
        {
            if (file == null)
                return "Ficheiro não fornecido.";

            if (file.Length == 0)
                return "Ficheiro vazio.";

            // Validar extensão
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return $"Extensão '{extension}' não permitida. Use: {string.Join(", ", allowedExtensions)}";

            // Validar tamanho
            if (file.Length > maxSizeInBytes)
            {
                var maxSizeMB = maxSizeInBytes / (1024 * 1024);
                return $"Ficheiro demasiado grande. Máximo: {maxSizeMB} MB";
            }

            return string.Empty;
        }

        /// <summary>
        /// Apaga um ficheiro.
        /// </summary>
        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath);

                if (!File.Exists(fullPath))
                {
                    _logger.LogWarning("Ficheiro não encontrado: {FilePath}", fullPath);
                    return false;
                }

                File.Delete(fullPath);
                _logger.LogInformation("Ficheiro apagado com sucesso: {FilePath}", filePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao apagar ficheiro: {FilePath}", filePath);
                return false;
            }
        }
    }
}
