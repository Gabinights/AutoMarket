using AutoMarket.Services.Interfaces;

namespace AutoMarket.Services
{
    /// <summary>
    /// Servi�o para gerenciar upload e processamento de ficheiros.
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

                // VALIDAÇÃO CRÍTICA: Verificar Magic Numbers (bytes de cabeçalho)
                // Previne upload de ficheiros maliciosos renomeados (ex: malware.exe -> image.jpg)
                if (!ValidateImageMagicNumbers(file))
                {
                    throw new ArgumentException("Ficheiro inválido: o conteúdo do ficheiro não corresponde a uma imagem válida.");
                }

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
        /// Faz upload de m�ltiplos ficheiros.
        /// </summary>
        public async Task<List<string>> UploadMultipleFilesAsync(List<IFormFile> files, string uploadFolder)
        {
            var uploadedFiles = new List<string>();

            // Validar quantidade
            if (files == null || files.Count == 0)
                throw new ArgumentException("Nenhum ficheiro fornecido.");

            const int maxFilesPerCar = 10;
            if (files.Count > maxFilesPerCar)
                throw new ArgumentException($"M�ximo de {maxFilesPerCar} ficheiros permitidos.");

            // Validar tamanho total
            const long maxTotalSize = 50 * 1024 * 1024; // 50 MB
            var totalSize = files.Sum(f => f.Length);

            if (totalSize > maxTotalSize)
                throw new ArgumentException($"Tamanho total demasiado grande. M�ximo: 50 MB");

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
                    // Continuar com os pr�ximos ficheiros
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

            // Validar Magic Numbers (segurança crítica)
            if (!ValidateImageMagicNumbers(file))
                return false;

            return true;
        }

        /// <summary>
        /// Obt�m mensagem de erro de valida��o de ficheiro.
        /// </summary>
        public string GetFileValidationError(IFormFile file, long maxSizeInBytes, string[] allowedExtensions)
        {
            if (file == null)
                return "Ficheiro n�o fornecido.";

            if (file.Length == 0)
                return "Ficheiro vazio.";

            // Validar extens�o
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return $"Extens�o '{extension}' n�o permitida. Use: {string.Join(", ", allowedExtensions)}";

            // Validar tamanho
            if (file.Length > maxSizeInBytes)
            {
                var maxSizeMB = maxSizeInBytes / (1024 * 1024);
                return $"Ficheiro demasiado grande. M�ximo: {maxSizeMB} MB";
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

        /// <summary>
        /// Valida os Magic Numbers (bytes de cabeçalho) de um ficheiro de imagem.
        /// Esta validação é crítica para segurança: previne upload de ficheiros maliciosos
        /// que foram renomeados para parecer imagens (ex: malware.exe -> image.jpg).
        /// </summary>
        private bool ValidateImageMagicNumbers(IFormFile file)
        {
            try
            {
                // Ler os primeiros bytes do ficheiro (Magic Numbers)
                var header = new byte[12];
                using (var stream = file.OpenReadStream())
                {
                    var bytesRead = stream.Read(header, 0, header.Length);
                    if (bytesRead < 4) // Precisamos pelo menos 4 bytes para validar
                        return false;

                    stream.Position = 0; // Reset para permitir leitura posterior
                }

                // JPEG: FF D8 FF
                if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                {
                    return true;
                }

                // PNG: 89 50 4E 47 0D 0A 1A 0A
                if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                    header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
                {
                    return true;
                }

                // WebP: RIFF ... WEBP
                // RIFF signature: 52 49 46 46 (RIFF em ASCII)
                // WebP signature começa no byte 8: 57 45 42 50 (WEBP em ASCII)
                if (header.Length >= 12 &&
                    header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                    header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
                {
                    return true;
                }

                // GIF: 47 49 46 38 (GIF8)
                if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38)
                {
                    return true;
                }

                // BMP: 42 4D (BM em ASCII)
                if (header[0] == 0x42 && header[1] == 0x4D)
                {
                    return true;
                }

                _logger.LogWarning("Ficheiro rejeitado: Magic Numbers não correspondem a uma imagem válida. Primeiros bytes: {Bytes}", 
                    BitConverter.ToString(header.Take(8).ToArray()));
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar Magic Numbers do ficheiro: {FileName}", file.FileName);
                return false;
            }
        }
    }
}
