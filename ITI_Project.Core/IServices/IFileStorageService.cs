using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ITI_Project.Core.IServices
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(
            IFormFile file,
            string? subFolder = null,
            string? fileName = null,
            CancellationToken cancellationToken = default);

        void DeleteFile(string relativePath);

        Task<(bool Success, string Message, string? FilePath)> UploadFileAsync(
            IFormFile file,
            string folderName,
            ClaimsPrincipal? user,
            string? customFileName = null,
            IReadOnlyCollection<string>? allowedExtensions = null,
            long maxFileSizeBytes = 5 * 1024 * 1024,
            CancellationToken cancellationToken = default);
    }
}
