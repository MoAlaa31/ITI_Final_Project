using ITI_Project.Core.Models.AiResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Services.AzureAi
{
    public interface IImageQualityService
    {
        bool IsImageClear(string filePath);
    }
}
