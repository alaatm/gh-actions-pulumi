# Azure Pipelines with Pulumi

### Steps

### Local Steps

1.  Install Pulumi:

    ```
    @"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -InputFormat None -ExecutionPolicy Bypass -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; iex ((New-Object System.Net.WebClient).DownloadString('https://get.pulumi.com/install.ps1'))" && SET "PATH=%PATH%;%USERPROFILE%\.pulumi\bin"
    ```

1.  Bootstrap a new C# pulumi infrastructure program:

    ```
    $ mkdir infra && cd infra
    $ pulumi new csharp
    ```

1.  Create a new stack and give it a name, we use `dev` here:

    ```
    $ pulumi stack init dev
    ```

1.  Configure the location to deploy the resources to:

    ```
    $ pulumi config set azure:location "West Europe"
    ```

1. Define config settings:

    ```
    $ pulumi config set --secret sqlUsername <value>
    $ pulumi config set --secret sqlPassword <value>
    $ pulumi config set <key> <value>
    ...
    ```

1. Build your Azure infrastructure code. See the `/infra` folder for reference.

### Azure DevOps

1. Create a new Azure DevOps project:

    a. Login to your [Azure DevOps account](https://dev.azure.com) and create a new project.

    b. Link it to your GitHub repository, Azure will request you authorize access.

1. Set Pulumi access token:

    In the created pipeline add a new variable called `PULUMI_ACCESS_TOKEN` to set your [pulumi access token](https://app.pulumi.com/alaatm/settings/tokens).

1. Add `azure-pipelines.yml`, see the one in the root of this repo for reference.


### Resources

1. [Pulumi GitHub Actions 1](https://www.pulumi.com/docs/guides/continuous-delivery/github-actions/)
1. [Pulumi Github Actions 2](https://www.pulumi.com/docs/guides/continuous-delivery/azure-devops/)
1. [Azure DevOps](https://www.pulumi.com/docs/guides/continuous-delivery/azure-devops/)
1. [Azure Setup](https://www.pulumi.com/docs/intro/cloud-providers/azure/setup/)
1. [Build, test, and deploy .NET Core apps](https://docs.microsoft.com/en-us/azure/devops/pipelines/ecosystems/dotnet-core?view=azure-devops)
1. [Deploy an Azure Web App](https://docs.microsoft.com/en-us/azure/devops/pipelines/targets/webapp?view=azure-devops&tabs=yaml#endpoint)
