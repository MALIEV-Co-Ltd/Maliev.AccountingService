namespace Maliev.AccountingService.Tests.Unit;

/// <summary>
/// Protects the deployment workflows' cross-repository GitOps boundary.
/// </summary>
public sealed class WorkflowSecurityContractTests
{
    /// <summary>
    /// Enumerates the three environment promotion workflows.
    /// </summary>
    public static TheoryData<string> DeploymentWorkflows => new()
    {
        ".github/workflows/ci-develop.yml",
        ".github/workflows/ci-staging.yml",
        ".github/workflows/ci-main.yml"
    };

    /// <summary>
    /// Workflow-scoped GitHub token permissions must remain read-only.
    /// </summary>
    [Theory]
    [MemberData(nameof(DeploymentWorkflows))]
    public void DeploymentWorkflow_UsesTopLevelReadOnlyContentsPermission(string workflowPath)
    {
        var source = ReadRepositoryFile(workflowPath);
        var permissions = source.IndexOf("\npermissions:\n  contents: read\n", StringComparison.Ordinal);
        var jobs = source.IndexOf("\njobs:\n", StringComparison.Ordinal);

        Assert.True(permissions >= 0, $"{workflowPath} must declare top-level contents: read permissions.");
        Assert.True(jobs > permissions, $"{workflowPath} permissions must be top-level and precede jobs.");
    }

    /// <summary>
    /// Tool installation must precede the credentialed checkout, which must pin the GitOps main branch.
    /// </summary>
    [Theory]
    [MemberData(nameof(DeploymentWorkflows))]
    public void DeploymentWorkflow_PinsGitOpsMainAfterKustomizeSetup(string workflowPath)
    {
        var source = ReadRepositoryFile(workflowPath);
        var setup = source.IndexOf("- name: Install Kustomize", StringComparison.Ordinal);
        var checkout = source.IndexOf("- name: Checkout maliev-gitops repository", StringComparison.Ordinal);
        var update = source.IndexOf("- name: Update image tag", StringComparison.Ordinal);

        Assert.True(setup >= 0, $"{workflowPath} must install Kustomize.");
        Assert.True(checkout > setup, $"{workflowPath} must install Kustomize before the credentialed checkout.");
        Assert.True(update > checkout, $"{workflowPath} must check out GitOps before updating its overlay.");

        var checkoutBlock = source[checkout..update];
        Assert.Contains("repository: MALIEV-Co-Ltd/maliev-gitops", checkoutBlock, StringComparison.Ordinal);
        Assert.Contains("ref: main", checkoutBlock, StringComparison.Ordinal);
        Assert.Contains("token: ${{ secrets.GITOPS_PAT }}", checkoutBlock, StringComparison.Ordinal);
    }

    /// <summary>
    /// Generated GitOps pull requests must target main and remain clearly non-mergeable drafts.
    /// </summary>
    [Theory]
    [MemberData(nameof(DeploymentWorkflows))]
    public void DeploymentWorkflow_CreatesDoNotMergeDraftAgainstMain(string workflowPath)
    {
        var source = ReadRepositoryFile(workflowPath);

        Assert.Contains("--title \"[DO NOT MERGE]", source, StringComparison.Ordinal);
        Assert.Contains("--base main", source, StringComparison.Ordinal);
        Assert.Contains("--draft", source, StringComparison.Ordinal);
    }

    private static string ReadRepositoryFile(string repositoryPath)
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            repositoryPath));
        Assert.True(File.Exists(path), $"Could not find workflow: {path}");
        return File.ReadAllText(path).ReplaceLineEndings("\n");
    }
}
