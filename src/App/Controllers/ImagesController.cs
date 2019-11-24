using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Models;
using System;
using Microsoft.AspNetCore.Hosting;

namespace App.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly ImagesDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<ImagesController> _logger;
        private readonly IWebHostEnvironment _env;

        public ImagesController(ImagesDbContext context, IConfiguration config, ILogger<ImagesController> logger, IWebHostEnvironment env)
        {
            _db = context;
            _config = config;
            _logger = logger;
            _env = env;
        }

        [HttpGet("{id}")]
        public ActionResult Get(int id) => Ok(_db.Images.Where(p => p.Id == id).Select(img => new
        {
            img.Id,
            img.Name,
            Data = Convert.ToBase64String(img.Data),
        }).FirstOrDefault());

        [HttpGet]
        public ActionResult Get() => Ok(_db.Images.Select(img => new
        {
            img.Id,
            img.Name,
            Data = Convert.ToBase64String(img.Data),
        }).ToList());

        [HttpPost]
        public async Task<ActionResult> Post(IFormCollection formCollection)
        {
            var file = formCollection.Files.Single();
            await WriteFileAsync(file.Name);
            await UploadAsync(file.OpenReadStream(), file.Name);
            return StatusCode(StatusCodes.Status202Accepted);
        }

        private async Task UploadAsync(Stream imageStream, string name)
        {
            var storageAccount = CloudStorageAccount.Parse(_config["Storage:ConnectionString"]);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_config["Storage:Container"]);
            var blockBlob = container.GetBlockBlobReference(name);
            await blockBlob.UploadFromStreamAsync(imageStream);
        }

        private Task WriteFileAsync(string name)
            => System.IO.File.WriteAllTextAsync(Path.Join(GetRootFolder(), Path.GetFileNameWithoutExtension(name) + ".txt"), DateTime.UtcNow.Ticks.ToString());

        private string GetRootFolder()
        {
            var root = IsRunningInAzure()
                ? Path.Join(_env.ContentRootPath, "..", "uploads")
                : Path.Join(_env.ContentRootPath, "uploads");

            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            return root;
        }

        private bool IsRunningInAzure()
            => !String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
    }
}
