using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ITI_Project.Api.Settings;
using ITI_Project.Core.IServices;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;

namespace ITI_Project.Services
{
    public class CloudinaryFileStorageService : IFileStorageService
    {
        private readonly Cloudinary cloudinary;

        public CloudinaryFileStorageService(IOptions<CloudinarySettings> options)
        {
            var settings = options.Value;

            var account = new Account(
                settings.CloudName,
                settings.ApiKey,
                settings.ApiSecret
            );

            cloudinary = new Cloudinary(account);
        }

        public async Task<(bool Success, string Message, string? FilePath)> UploadFileAsync(
            Stream file,
            string folderName,
            string originalFileName,
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

            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

            var extensions = allowedExtensions ??
                new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (string.IsNullOrWhiteSpace(extension) ||
                !extensions.Contains(extension))
            {
                return (
                    false,
                    $"Invalid file format. Allowed formats: {string.Join(", ", extensions)}",
                    null
                );
            }

            if (file.Length > maxFileSizeBytes)
            {
                return (
                    false,
                    $"File size must be less than {maxFileSizeBytes / (1024 * 1024)}MB.",
                    null
                );
            }

            var baseName = customFileName;

            if (string.IsNullOrWhiteSpace(baseName))
            {
                var givenName = user?.FindFirstValue(ClaimTypes.GivenName);
                var nameId = user?.FindFirstValue(ClaimTypes.NameIdentifier);

                var safeGivenName = string.IsNullOrWhiteSpace(givenName)
                    ? "user"
                    : givenName;

                var safeNameId = string.IsNullOrWhiteSpace(nameId)
                    ? Guid.NewGuid().ToString("N")
                    : nameId;

                var uniqueSuffix = Guid.NewGuid().ToString("N");

                baseName = $"{safeGivenName}-{safeNameId}-{uniqueSuffix}";
            }

            try
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(originalFileName, file),
                    Folder = folderName,
                    PublicId = baseName,
                    UseFilename = false,
                    UniqueFilename = true,
                    Overwrite = false
                };

                var uploadResult = await cloudinary
                    .UploadAsync(uploadParams, cancellationToken);

                if (uploadResult.StatusCode != HttpStatusCode.OK)
                {
                    return (false, "Failed to upload image.", null);
                }

                return (
                    true,
                    "File uploaded successfully.",
                    uploadResult.SecureUrl.ToString()
                );
            }
            catch
            {
                return (
                    false,
                    "An error occurred while uploading the file.",
                    null
                );
            }
        }

        public async Task<string> SaveFileAsync(
            Stream file,
            string originalFileName,
            string? subFolder = null,
            string? fileName = null,
            CancellationToken cancellationToken = default)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(originalFileName, file),
                Folder = subFolder,
                PublicId = fileName,
                UseFilename = false,
                UniqueFilename = true
            };

            var result = await cloudinary
                .UploadAsync(uploadParams, cancellationToken);

            if (result.StatusCode != HttpStatusCode.OK)
                throw new Exception("Failed to upload file.");

            return result.SecureUrl.ToString();
        }

        public void DeleteFile(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return;

            try
            {
                var uri = new Uri(relativePath);

                var segments = uri.AbsolutePath.Split('/');

                var uploadIndex = Array.IndexOf(segments, "upload");

                if (uploadIndex == -1)
                    return;

                var publicIdWithExtension =
                    string.Join("/", segments[(uploadIndex + 2)..]);

                var publicId =
                    Path.ChangeExtension(publicIdWithExtension, null);

                var deletionParams = new DeletionParams(publicId);

                cloudinary.Destroy(deletionParams);
            }
            catch
            {
                // Optional: log error
            }
        }
    }
}