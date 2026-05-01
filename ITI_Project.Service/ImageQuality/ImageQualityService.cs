using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using OpenCvSharp;

namespace ITI_Project.Services.AzureAi
{
    public class ImageQualityService : IImageQualityService
    {
        private readonly IWebHostEnvironment environment;
        public ImageQualityService(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        public bool IsImageClear(string filePath)
        {
            var webRootPath = environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
                webRootPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot");

            var sanitizedPath = filePath.TrimStart('/', '\\');

            var fullPath = Path.Combine(webRootPath, sanitizedPath);

            using var image = Cv2.ImRead(fullPath, ImreadModes.Grayscale);

            if (image.Empty())
            {
                return false;
            }

            // Reject low resolution images
            //if (image.Width < 800 || image.Height < 600)
            //    return false;

            using var laplacian = new Mat();

            Cv2.Laplacian(image, laplacian, MatType.CV_64F);

            Cv2.MeanStdDev(laplacian, out _, out var stddev);

            double variance = stddev.Val0 * stddev.Val0;

            return variance >= 100;
        }
    }
}
