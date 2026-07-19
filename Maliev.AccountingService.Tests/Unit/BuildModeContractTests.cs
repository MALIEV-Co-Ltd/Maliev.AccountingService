namespace Maliev.AccountingService.Tests.Unit;

/// <summary>
/// Tests deterministic package-mode and container build inputs.
/// </summary>
public sealed class BuildModeContractTests
{
    /// <summary>
    /// Package mode should consume the exact published ServiceDefaults version containing exchange support.
    /// </summary>
    [Fact]
    public void ServiceDefaultsDependency_PinsPublishedCentralExchangeVersion()
    {
        var source = ReadRepositoryFile("Directory.Build.props");

        Assert.Contains(
            "<ServiceDefaultsVersion Condition=\"'$(ServiceDefaultsVersion)' == ''\">1.0.89-alpha</ServiceDefaultsVersion>",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "<ServiceDefaultsVersion Condition=\"'$(ServiceDefaultsVersion)' == ''\">1.0.*",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "<SharedLibraryVersion Condition=\"'$(SharedLibraryVersion)' == ''\">1.0.96-alpha</SharedLibraryVersion>",
            source,
            StringComparison.Ordinal);

        foreach (var project in new[]
                 {
                     "Maliev.AccountingService.Api/Maliev.AccountingService.Api.csproj",
                     "Maliev.AccountingService.Application/Maliev.AccountingService.Application.csproj",
                     "Maliev.AccountingService.Infrastructure/Maliev.AccountingService.Infrastructure.csproj",
                     "Maliev.AccountingService.Tests/Maliev.AccountingService.Tests.csproj"
                 })
        {
            var projectSource = ReadRepositoryFile(project.Split('/'));
            Assert.Contains(
                "<PackageReference Include=\"Maliev.Aspire.ServiceDefaults\" Version=\"$(ServiceDefaultsVersion)\" />",
                projectSource,
                StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Docker package restore must be deterministic and must not advertise an unavailable curl health check.
    /// </summary>
    [Fact]
    public void Dockerfile_UsesOneDeterministicPackageRestoreBeforeNoRestoreBuildAndPublish()
    {
        var source = ReadRepositoryFile("Maliev.AccountingService.Api", "Dockerfile");
        var propertiesCopy = source.IndexOf("COPY [\"Directory.Build.props\", \".\"]", StringComparison.Ordinal);
        var restore = source.IndexOf(
            "dotnet restore \"./Maliev.AccountingService.Api/Maliev.AccountingService.Api.csproj\"",
            StringComparison.Ordinal);

        Assert.True(propertiesCopy >= 0, "Dockerfile must copy Directory.Build.props into the restore layer.");
        Assert.True(restore > propertiesCopy, "Directory.Build.props must be available before dotnet restore.");
        Assert.Equal(1, CountOccurrences(source, "dotnet restore"));
        Assert.Contains("ARG GITHUB_ACTIONS=true", source, StringComparison.Ordinal);
        Assert.Contains("ARG SERVICE_DEFAULTS_VERSION=1.0.89-alpha", source, StringComparison.Ordinal);
        Assert.Contains("-p:ServiceDefaultsVersion=$SERVICE_DEFAULTS_VERSION", source, StringComparison.Ordinal);
        Assert.Contains("dotnet build", source, StringComparison.Ordinal);
        Assert.Contains("--no-restore", source, StringComparison.Ordinal);
        Assert.Contains("dotnet publish", source, StringComparison.Ordinal);
        Assert.DoesNotContain("HEALTHCHECK", source, StringComparison.Ordinal);
        Assert.DoesNotContain("curl", source, StringComparison.Ordinal);
    }

    /// <summary>
    /// Host build assets must not overwrite the Linux restore performed inside the image build.
    /// </summary>
    [Fact]
    public void Dockerignore_ExcludesNestedBuildOutputs()
    {
        var source = ReadRepositoryFile(".dockerignore");

        Assert.Contains("**/bin/", source, StringComparison.Ordinal);
        Assert.Contains("**/obj/", source, StringComparison.Ordinal);
    }

    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

    private static string ReadRepositoryFile(params string[] segments)
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            Path.Combine(segments)));
        Assert.True(File.Exists(path), $"Could not find source file: {path}");
        return File.ReadAllText(path);
    }
}
