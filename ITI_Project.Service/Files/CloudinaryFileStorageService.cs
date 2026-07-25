using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ITI_Project.Api.Settings;
using ITI_Project.Core.IServices;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ITI_Project.Services.Files
{
    public class CloudinaryFileStorageService : FileStorageServiceBase, IFileStorageService
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

            try
            {
                if (request.File.CanSeek) request.File.Position = 0;

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(request.OriginalFileName, request.File),
                    Folder = request.FolderName,
                    PublicId = baseName,
                    UseFilename = false,
                    UniqueFilename = true,
                    Overwrite = false
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams, cancellationToken);

                if (uploadResult == null || uploadResult.StatusCode != HttpStatusCode.OK)
                    return (false, "Failed to upload image.", null);

                return (true, "File uploaded successfully.", uploadResult.SecureUrl.ToString());
            }
            catch
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
            if (file.CanSeek) file.Position = 0;

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(originalFileName, file),
                Folder = subFolder,
                PublicId = fileName,
                UseFilename = false,
                UniqueFilename = true
            };

            var result = await cloudinary.UploadAsync(uploadParams, cancellationToken);

            if (result == null || result.StatusCode != HttpStatusCode.OK)
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
                var path = uri.AbsolutePath; // e.g. /image/upload/v123/folder/file.jpg
                var marker = "/upload/";
                var uploadIndex = path.IndexOf(marker, StringComparison.OrdinalIgnoreCase);

                if (uploadIndex == -1)
                    return;

                var afterUpload = path.Substring(uploadIndex + marker.Length); // e.g. v123/folder/file.jpg or folder/file.jpg

                // Remove version token if present (v123/)
                if (afterUpload.Length > 0 && afterUpload[0] == 'v')
                {
                    var slash = afterUpload.IndexOf('/');
                    if (slash > 0)
                    {
                        var versionToken = afterUpload.Substring(1, slash - 1);
                        if (versionToken.All(char.IsDigit))
                            afterUpload = afterUpload.Substring(slash + 1);
                    }
                }

                var publicIdWithExtension = afterUpload.TrimStart('/');
                var publicId = Path.ChangeExtension(publicIdWithExtension, null);

                if (string.IsNullOrWhiteSpace(publicId))
                    return;

                var deletionParams = new DeletionParams(publicId);
                cloudinary.Destroy(deletionParams);
            }
            catch
            {
                // optional logging
            }
        }
    }
}