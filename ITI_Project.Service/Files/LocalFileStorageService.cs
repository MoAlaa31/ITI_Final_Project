using ITI_Project.Core.IServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Reflection.Metadata;
using System.Security.Claims;

namespace ITI_Project.Services.Files
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment environment;
        private readonly IConfiguration configuration;

        public LocalFileStorageService(IWebHostEnvironment environment, IConfiguration configuration)
        {
            this.environment = environment;
            this.configuration = configuration;
        }

        public async Task<(bool Success, string Message, string? FilePath)> UploadFileAsync(
            IFormFile file,
            string folderName,
            ClaimsPrincipal? user,
            string? customFileName = null,
            IReadOnlyCollection<string>? allowedExtensions = null,
            long maxFileSizeBytes = 5 * 1024 * 1024,
            CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                return (false, "File is required.", null);

            if (string.IsNullOrWhiteSpace(folderName))
                return (false, "Folder name is required.", null);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var extensions = allowedExtensions ?? new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };

            if (string.IsNullOrWhiteSpace(extension) || !extensions.Contains(extension))
                return (false, $"Invalid file format. Allowed formats: {string.Join(", ", extensions)}", null);

            if (file.Length > maxFileSizeBytes)
                return (false, $"File size must be less than {maxFileSizeBytes / (1024 * 1024)}MB.", null);

            var baseName = customFileName;
            if (string.IsNullOrWhiteSpace(baseName))
            {
                var givenName = user?.FindFirstValue(ClaimTypes.GivenName);
                var nameId = user?.FindFirstValue(ClaimTypes.NameIdentifier);

                var safeGivenName = string.IsNullOrWhiteSpace(givenName) ? "user" : givenName;
                var safeNameId = string.IsNullOrWhiteSpace(nameId) ? Guid.NewGuid().ToString("N") : nameId;
                var uniqueSuffix = Guid.NewGuid().ToString("N");

                baseName = $"{safeGivenName}-{safeNameId}-{uniqueSuffix}";
            }

            var fileName = $"{baseName}{extension}";

            try
            {
                var relativePath = await SaveFileAsync(file, folderName, fileName, cancellationToken);
                return (true, "File uploaded successfully.", relativePath);
            }
            catch (Exception)
            {
                return (false, "An error occurred while uploading the file.", null);
            }
        }

        public async Task<string> SaveFileAsync(
            IFormFile file,
            string? subFolder = null,
            string? fileName = null,
            CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is required.", nameof(file));

            var webRootPath = environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var uploadRoot = Path.Combine(webRootPath, "uploads");
            var targetFolder = string.IsNullOrWhiteSpace(subFolder)
                ? uploadRoot
                : Path.Combine(uploadRoot, subFolder);

            Directory.CreateDirectory(targetFolder);

            var extension = Path.GetExtension(file.FileName);
            var finalFileName = string.IsNullOrWhiteSpace(fileName)
                ? $"{Guid.NewGuid():N}{extension}"
                : Path.GetFileName(fileName);

            var filePath = Path.Combine(targetFolder, finalFileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream, cancellationToken);

            var relativePath = string.IsNullOrWhiteSpace(subFolder)
                ? Path.Combine("uploads", finalFileName)
                : Path.Combine("uploads", subFolder, finalFileName);

            return relativePath.Replace("\\", "/");
        }

        public void DeleteFile(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return;

            var webRootPath = environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var sanitizedPath = relativePath.TrimStart('/', '\\');
            var fullPath = Path.Combine(webRootPath, sanitizedPath);

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}
