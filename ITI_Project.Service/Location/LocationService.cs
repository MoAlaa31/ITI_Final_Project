using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ITI_Project.Services.Location
{
    public class LocationService(HttpClient httpClient) : ILocationService
    {
        public async Task<string?> ReverseGeocodeAsync(double latitude, double longitude)
        {
            var url =
                $"https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat={latitude}&lon={longitude}";

            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("HereafyApp/1.0");

            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(json);

            if (document.RootElement.TryGetProperty("display_name", out var displayName))
                return displayName.GetString();

            return null;
        }
    }
}
