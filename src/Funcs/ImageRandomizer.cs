using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Models;
using SkiaSharp;

namespace Func
{
    public class ImageRandomizer
    {
        private readonly Random _rnd = new Random();
        private readonly ImagesDbContext _db;

        public ImageRandomizer(ImagesDbContext db) => _db = db;

        [FunctionName("ImageRandomizer")]
        public async Task Run([TimerTrigger("0 0 */3 * * *")]TimerInfo timer, ILogger log)
        {
            log.LogInformation("Function triggered by timer");

            log.LogInformation("Generating random image...");
            using var ms = await GetRandomImage();

            log.LogInformation("Saving image...");
            await _db.Images.AddAsync(new Image
            {
                Name = Guid.NewGuid().ToString(),
                Size = ms.Length,
                Data = ms.ToArray()
            });

            try
            {
                log.LogInformation("Persisting db...");
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Could not save image");
            }

            log.LogInformation("Done.");
        }

        private async Task<MemoryStream> GetRandomImage()
        {
            var width = 960;
            var height = 720;

            using var bmp = new SKBitmap(width, height);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var a = (byte)_rnd.Next(256);
                    var r = (byte)_rnd.Next(256);
                    var g = (byte)_rnd.Next(256);
                    var b = (byte)_rnd.Next(256);

                    bmp.SetPixel(x, y, new SKColor(r, g, b, a));
                }
            }

            var ms = new MemoryStream();
            var imgData = SKImage.FromBitmap(bmp).Encode(SKEncodedImageFormat.Jpeg, 100);
            await imgData.AsStream().CopyToAsync(ms);

            return ms;
        }
    }
}
