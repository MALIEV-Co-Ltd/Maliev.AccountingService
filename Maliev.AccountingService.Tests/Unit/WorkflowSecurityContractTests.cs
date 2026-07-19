using System.Text.RegularExpressions;

namespace Maliev.AccountingService.Tests.Unit;

/// <summary>
/// Protects the validation-only CI boundary.
/// </summary>
public sealed class WorkflowSecurityContractTests
{
    private static readonly string[] AllWorkflows =
    [
        "_validate.yml",
        "pr-validation.yml",
        "ci-develop.yml",
        "ci-staging.yml",
        "ci-main.yml"
    ];

    /// <summary>
    /// Branch and tag workflows may validate but may not publish or deploy.
    /// </summary>
    [Theory]
    [InlineData("ci-develop.yml", "develop")]
    [InlineData("ci-main.yml", "main")]
    [InlineData("ci-staging.yml", "release/v*")]
    public void BranchAndTagWorkflows_AreValidationOnly(string file, string trigger)
    {
        var source = ReadWorkflow(file);

        Assert.Contains(trigger, source, StringComparison.Ordinal);
        Assert.Contains("uses: ./.github/workflows/_validate.yml", source, StringComparison.Ordinal);
        Assert.Contains("contents: read", source, StringComparison.Ordinal);
        Assert.DoesNotContain("environment:", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("id-token: write", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("push: true", source, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validation must reconstruct exact public shared dependencies without package credentials.
    /// </summary>
    [Fact]
    public void ReusableValidation_UsesExactPublicSourcesWithoutCredentials()
    {
        var source = ReadWorkflow("_validate.yml");

        Assert.Contains("25a5c3b2d3d6b5ce8ed485d2d44a28f4dc4c9b51", source, StringComparison.Ordinal);
        Assert.Contains("559a00db0c7920a5247fdff60d4476ad23a9a501", source, StringComparison.Ordinal);
        Assert.Contains("prepare-accounting-ci-packages.sh", source, StringComparison.Ordinal);
        Assert.DoesNotContain("GITOPS_PAT", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("NUGET_PASSWORD", source, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Every workflow must avoid deployment mutation and mutable action tags.
    /// </summary>
    [Theory]
    [MemberData(nameof(WorkflowNames))]
    public void EveryWorkflow_ForbidsDeploymentAndPinsActions(string file)
    {
        var source = ReadWorkflow(file);

        foreach (var forbidden in new[]
                 {
                     "gcloud", "maliev-gitops", "kustomize", "kubectl", "argocd",
                     "credentials_json", "GCP_SA_KEY", "packages: write", "contents: write"
                 })
        {
            Assert.DoesNotContain(forbidden, source, StringComparison.OrdinalIgnoreCase);
        }

        var mutableAction = Regex.Match(
            source,
            @"(?m)^\s*-?\s*uses:\s*(?!\./)(?<action>[^\s@]+)@(?<reference>(?![0-9a-f]{40}(?:\s|$))[^\s#]+)");
        Assert.False(mutableAction.Success, $"{file} contains mutable action reference {mutableAction.Value}.");
    }

    /// <summary>
    /// Supplies workflow names for contract validation.
    /// </summary>
    public static TheoryData<string> WorkflowNames => [.. AllWorkflows];

    private static string ReadWorkflow(string file) => ReadRepositoryFile(".github", "workflows", file);

    private static string ReadRepositoryFile(params string[] segments)
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            Path.Combine(segments)));
        Assert.True(File.Exists(path), $"Could not find workflow: {path}");
        return File.ReadAllText(path).ReplaceLineEndings("\n");
    }
}
