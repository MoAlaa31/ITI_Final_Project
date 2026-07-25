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

        public Task<(bool Success, string Message, string? FilePath)>
            UploadFileAsync(
                FileUploadRequest request,
                CancellationToken cancellationToken = default);
    }
}
