using ITI_Project.Core.IServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ITI_Project.Services.Files
{
    public class LocalFileStorageService : FileStorageServiceBase, IFileStorageService
    {
        private readonly IWebHostEnvironment environment;
        private readonly IConfiguration configuration;

        public LocalFileStorageService(IWebHostEnvironment environment, IConfiguration configuration)
        {
            this.environment = environment;
            this.configuration = configuration;
        }

        public async Task<(bool Success, string Message, string? FilePath)>
        UploadFileAsync(
            FileUploadRequest request,
            CancellationToken cancellationToken = default)
        {
            var validation = ValidateFile(
                request.File,
                request.OriginalFileName,
                request.AllowedExtensions,
                request.MaxFileSizeBytes);

            if (!validation.Success)
                return (false, validation.Message!, null);

            var extension = Path.GetExtension(request.OriginalFileName).ToLowerInvariant();

            var baseName = BuildBaseName(request.GivenName, request.NameId, request.CustomFileName);
            var fileName = BuildFileName(baseName, extension);

            try
            {
                var relativePath = await SaveFileAsync(
                    request.File,
                    request.OriginalFileName,
                    request.FolderName,
                    fileName,
                    cancellationToken);

                return (true, "File uploaded successfully.", relativePath);
            }
            catch (Exception)
            {
                return (false, "An error occurred while uploading the file.", null);
            }
        }

        public async Task<string> SaveFileAsync(
            Stream file,
            string originalFileName,
            string? subFolder = null,
            string? fileName = null,
            CancellationToken cancellationToken = default)
        {
            if (file == null)
                throw new ArgumentException("File is required.", nameof(file));

            var webRootPath = environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var uploadRoot = Path.Combine(webRootPath, "uploads");
            var targetFolder = string.IsNullOrWhiteSpace(subFolder)
                ? uploadRoot
                : Path.Combine(uploadRoot, subFolder);

            Directory.CreateDirectory(targetFolder);

            var extension = Path.GetExtension(originalFileName);
            var finalFileName = string.IsNullOrWhiteSpace(fileName)
                ? $"{Guid.NewGuid():N}{extension}"
                : Path.GetFileName(fileName);

            var filePath = Path.Combine(targetFolder, finalFileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            if (file.CanSeek) file.Position = 0;
            await file.CopyToAsync(stream, cancellationToken);

            var relativePath = string.IsNullOrWhiteSpace(subFolder)
                ? Path.Combine("uploads", finalFileName)
                : Path.Combine("uploads", subFolder, finalFileName);

            return NormalizeRelativePath(relativePath);
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
