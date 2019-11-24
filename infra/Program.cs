using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Pulumi;
using Pulumi.Azure.Core;
using Pulumi.Azure.Sql;
using Pulumi.Azure.Storage;
using Pulumi.Azure.AppService;
using Pulumi.Azure.AppInsights;
using Pulumi.Azure.AppService.Inputs;

class Program
{
    static (Output<string> username, Output<string> password) GetConfig()
    {
        var config = new Pulumi.Config();
        var username = config.RequireSecret("sqlUsername");
        var password = config.RequireSecret("sqlPassword");

        return (username, password);
    }

    static (SqlServer sqlServer, Output<string> connStr) CreateSqlServerAndDatabase(ResourceGroup rg, Output<string> username, Output<string> password)
    {
        const string connStr = "Server=tcp:{0}.database.windows.net;Database={1};MultipleActiveResultSets=true;User Id={2};Password=\"{3}\";";

        var sqlServer = new SqlServer("sql-", new SqlServerArgs
        {
            ResourceGroupName = rg.Name,
            AdministratorLogin = username,
            AdministratorLoginPassword = password,
            Version = "12.0",
        });

        var database = new Database("db-", new DatabaseArgs
        {
            ResourceGroupName = rg.Name,
            ServerName = sqlServer.Name,
            Edition = "Free",
        });

        return (sqlServer, Pulumi.Output.All<string>(sqlServer.Name, database.Name, username, password).Apply(arr => String.Format(connStr, arr[0], arr[1], arr[2], arr[3])));
    }

    static Plan CreateAppServicePlan(ResourceGroup rg) => new Plan("asp-", new PlanArgs
    {
        ResourceGroupName = rg.Name,
        Kind = "Windows",
        Sku = new PlanSkuArgs
        {
            Tier = "Free",
            Size = "F1",
        },
    });

    static (Account storage, Container imagesContainer) CreateStorageAccount(ResourceGroup rg)
    {
        var storageAccount = new Account("storage", new AccountArgs
        {
            ResourceGroupName = rg.Name,
            AccountKind = "StorageV2",
            AccountTier = "Standard",
            AccountReplicationType = "LRS",
        });

        var imagesContainer = new Container("images-", new ContainerArgs
        {
            StorageAccountName = storageAccount.Name,
            ContainerAccessType = "private",
        });

        return (storageAccount, imagesContainer);
    }

    static AppService CreateAppService(ResourceGroup rg, Plan appServicePlan, Account storage, Container imagesContainer, SqlServer sqlServer, Output<string> connStr)
    {
        var appInsights = new Insights("ai-web-", new InsightsArgs
        {
            ResourceGroupName = rg.Name,
            ApplicationType = "web",
        });

        var app = new AppService("web-", new AppServiceArgs
        {
            ResourceGroupName = rg.Name,
            AppServicePlanId = appServicePlan.Id,
            AppSettings =
            {
                { "APPINSIGHTS_INSTRUMENTATIONKEY", appInsights.InstrumentationKey },
                { "ASPNETCORE_ENVIRONMENT", "Production" },
                { "WEBSITE_RUN_FROM_PACKAGE", "1" },    // Using manual az webapp deploy to take care of this https://github.com/Azure/app-service-announcements/issues/110
                { "Storage:ConnectionString", storage.PrimaryConnectionString },
                { "Storage:Container", imagesContainer.Name },
            },
            ConnectionStrings =
            {
                new AppServiceConnectionStringsArgs
                {
                    Name = "Default",
                    Type = "SQLAzure",
                    Value = connStr,
                }
            },
        });

        app.PossibleOutboundIpAddresses.Apply(ips =>
            ips.Split(",").Select(ip => new FirewallRule($"{ip}-", new FirewallRuleArgs
            {
                ResourceGroupName = rg.Name,
                ServerName = sqlServer.Name,
                StartIpAddress = ip,
                EndIpAddress = ip,
            })).ToList());

        return app;
    }

    static FunctionApp CreateFuncsApp(ResourceGroup rg, Plan appServicePlan, Account storage, Container imagesContainer, SqlServer sqlServer, Output<string> connStr)
    {
        var appInsights = new Insights("ai-funcs-", new InsightsArgs
        {
            ResourceGroupName = rg.Name,
            ApplicationType = "web",
        });

        var func = new FunctionApp("funcs-", new FunctionAppArgs
        {
            ResourceGroupName = rg.Name,
            AppServicePlanId = appServicePlan.Id,
            Version = "~3",
            StorageConnectionString = storage.PrimaryConnectionString,
            AppSettings =
            {
                { "APPINSIGHTS_INSTRUMENTATIONKEY", appInsights.InstrumentationKey },
                { "WEBSITE_RUN_FROM_PACKAGE", "1" },    // Using manual az webapp deploy to take care of this https://github.com/Azure/app-service-announcements/issues/110
                { "Container", imagesContainer.Name },
            },
            ConnectionStrings =
            {
                new FunctionAppConnectionStringsArgs
                {
                    Name = "Default",
                    Type = "SQLAzure",
                    Value = connStr,
                }
            },
        });

        return func;
    }

    static Task<int> Main()
    {
        return Deployment.RunAsync(() =>
        {
            var (username, password) = GetConfig();

            var rg = new ResourceGroup("gh-actions-");
            var (sqlServer, connStr) = CreateSqlServerAndDatabase(rg, username, password);
            var (storage, imagesContainer) = CreateStorageAccount(rg);
            var plan = CreateAppServicePlan(rg);
            var app = CreateAppService(rg, plan, storage, imagesContainer, sqlServer, connStr);
            var funcs = CreateFuncsApp(rg, plan, storage, imagesContainer, sqlServer, connStr);

            return new Dictionary<string, object>
            {
                { "rgName", rg.Name },
                { "appName", app.Name },
                { "funcsName", funcs.Name },
            };
        });
    }
}
