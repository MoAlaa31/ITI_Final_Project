using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ITI_Project.Services.Files
{
    /// <summary>
    /// Common helpers used by concrete file storage implementations.
    /// </summary>
    public abstract class FileStorageServiceBase
    {
        protected IReadOnlyCollection<string> DefaultAllowedExtensions { get; } =
            new[] { ".jpg", ".jpeg", ".png", ".webp" };

        protected long DefaultMaxFileSizeBytes { get; } = 5 * 1024 * 1024;

        protected (bool Success, string? Message) ValidateFile(
            Stream? file,
            string? originalFileName,
            IReadOnlyCollection<string>? allowedExtensions,
            long maxFileSizeBytes)
        {
            if (file == null || file.Length == 0)
                return (false, "File is required.");

            if (string.IsNullOrWhiteSpace(originalFileName))
                return (false, "Original file name is required.");

            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            var extensions = allowedExtensions ?? DefaultAllowedExtensions;

            if (string.IsNullOrWhiteSpace(extension) || !extensions.Contains(extension))
                return (false, $"Invalid file format. Allowed formats: {string.Join(", ", extensions)}");

            if (file.Length > maxFileSizeBytes)
                return (false, $"File size must be less than {maxFileSizeBytes / (1024 * 1024)}MB.");

            return (true, null);
        }

        protected string BuildBaseName(string? givenName, string? nameId, string? customFileName)
        {
            if (!string.IsNullOrWhiteSpace(customFileName))
                return customFileName;

            var safeGivenName = string.IsNullOrWhiteSpace(givenName) ? "user" : givenName;
            var safeNameId = string.IsNullOrWhiteSpace(nameId) ? Guid.NewGuid().ToString("N") : nameId;
            var uniqueSuffix = Guid.NewGuid().ToString("N");

            return $"{safeGivenName}-{safeNameId}-{uniqueSuffix}";
        }

        protected string BuildFileName(string baseName, string extension) =>
            $"{baseName}{extension}";

        protected string NormalizeRelativePath(string path) =>
            string.IsNullOrEmpty(path) ? path : path.Replace("\\", "/");
    }
}
