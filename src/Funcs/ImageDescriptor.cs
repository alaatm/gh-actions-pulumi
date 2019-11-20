using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Models;
using SkiaSharp;

namespace Func
{
    public class ImageDescriptor
    {
        private readonly ImagesDbContext _db;

        public ImageDescriptor(ImagesDbContext db) => _db = db;

        [FunctionName("ImageDescriptor")]
        public async Task Run([BlobTrigger("%Container%/{name}")]Stream blob, string name, ILogger log)
        {
            log.LogInformation($"Function triggered by {name}");

            log.LogInformation($"Resizing image...");
            using var ms = await ResizeAsync(blob);

            log.LogInformation("Saving image...");
            await _db.Images.AddAsync(new Image
            {
                Name = name,
                Size = ms.Length,
                Data = ms.ToArray()
            });

            try
            {
                log.LogInformation("Persisting db...");
                await _db.SaveChangesAsync();
                log.LogInformation("Removing temp files...");
                await DeleteBlobAsync(name);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Could not process image");
            }

            log.LogInformation("Done.");
        }

        private static async Task<MemoryStream> ResizeAsync(Stream source)
        {
            var ms = new MemoryStream();
            using (var bmp = SKBitmap.Decode(source))
            {
                if (bmp.Width > 960)
                {
                    var ratio = 960.0 / bmp.Width;
                    var height = (int)Math.Round(ratio * bmp.Height);

                    var scaledBmp = bmp.Resize(new SKImageInfo(960, height), SKFilterQuality.High);
                    var imgData = SKImage.FromBitmap(scaledBmp).Encode(SKEncodedImageFormat.Jpeg, 80);
                    await imgData.AsStream().CopyToAsync(ms);
                }
                else
                {
                    await source.CopyToAsync(ms);
                }
            }

            return ms;
        }

        private async Task DeleteBlobAsync(string name)
        {
            var connStr = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
            var containerName = Environment.GetEnvironmentVariable("Container", EnvironmentVariableTarget.Process);

            var storageAccount = CloudStorageAccount.Parse(connStr);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            var blockBlob = container.GetBlockBlobReference(name);
            await blockBlob.DeleteAsync();
        }
    }
}
