using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Services.Location
{
    public interface ILocationService
    {
        Task<string?> ReverseGeocodeAsync(double latitude, double longitude);
    }
}
