using ITI_Project.Core.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Services.User.DTOs
{
    public class UpdateClientProfileDTO
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public Gender? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public int GovernorateId { get; set; }
        public int RegionId { get; set; }
        public FileData? Picture { get; set; }
        public List<string>? PhoneNumbers { get; set; }

        public string? UserGivenName { get; set; }
        public string? UserNameIdentifier { get; set; }
    }

    public class FileData
    {
        public required Stream Content { get; init; }
        public required string FileName { get; init; }
    }
}
