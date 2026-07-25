using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Core.IServices
{
    public class FileUploadRequest
    {
        public required Stream File { get; init; }
        public required string FolderName { get; init; }
        public required string OriginalFileName { get; init; }
        public string? GivenName { get; init; }
        public string? NameId { get; init; }
        public string? CustomFileName { get; init; }
        public IReadOnlyCollection<string>? AllowedExtensions { get; init; }
        public long MaxFileSizeBytes { get; init; } = 5 * 1024 * 1024;
    }
}
