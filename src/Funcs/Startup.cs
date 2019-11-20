using System.Configuration;
using System.IO;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Models;

[assembly: FunctionsStartup(typeof(Func.Startup))]
namespace Func
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var connStr = config.GetConnectionString("Default");

            builder.Services.AddDbContext<ImagesDbContext>(
                options => SqlServerDbContextOptionsExtensions.UseSqlServer(options, connStr));
        }
    }
}