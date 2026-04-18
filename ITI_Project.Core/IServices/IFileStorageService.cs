using System.Security.Claims;

namespace ITI_Project.Core.IServices
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(
            Stream file,
            string originalFileName,
            string? subFolder = null,
            string? fileName = null,
            CancellationToken cancellationToken = default);

        void DeleteFile(string relativePath);

        Task<(bool Success, string Message, string? FilePath)> UploadFileAsync(
            Stream file,
            string folderName,
            string originalFileName,
            ClaimsPrincipal? user,
            string? customFileName = null,
            IReadOnlyCollection<string>? allowedExtensions = null,
            long maxFileSizeBytes = 5 * 1024 * 1024,
            CancellationToken cancellationToken = default);
    }
}
