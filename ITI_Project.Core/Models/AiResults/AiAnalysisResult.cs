using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Core.Models.AiResults
{
    public class AiAnalysisResult
    {
        public float Confidence { get; set; }

        public string? DocumentNumber { get; set; }

        public string? FullName { get; set; }

        public bool HasDocument { get; set; }

        public DateTime? ExpirationDate { get; set; }
    }
}
