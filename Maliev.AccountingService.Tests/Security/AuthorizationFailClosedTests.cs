using System.Net;
using Maliev.AccountingService.Application.Authorization;
using Maliev.AccountingService.Infrastructure.Data;
using Maliev.AccountingService.Tests.Testing;
using Maliev.Aspire.ServiceDefaults.IAM;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Maliev.AccountingService.Tests.Security;

/// <summary>
/// Verifies authorization behavior when the IAM dependency is unavailable.
/// </summary>
public sealed class AuthorizationFailClosedTests
{
    /// <summary>
    /// An authenticated caller without a matching token permission must be denied when IAM throws.
    /// </summary>
    [Fact]
    public async Task PermissionProtectedEndpoint_WhenIamThrows_DeniesAccess()
    {
        await using var factory = new ThrowingIamAccountingServiceFactory();
        await factory.InitializeAsync();
        using var client = factory.CreateAuthenticatedClient(
            userId: "iam-outage-user",
            roles: [],
            permissions: []);

        using var response = await client.GetAsync(
            "/accounting/v1/chart-of-accounts?pageNumber=1&pageSize=1",
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        factory.IamClient.Verify(
            client => client.CheckPermissionAsync(
                "iam-outage-user",
                AccountingPermissions.AccountsRead,
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private sealed class ThrowingIamAccountingServiceFactory
        : BaseIntegrationTestFactory<Program, AccountingDbContext>
    {
        public Mock<IIamServiceClient> IamClient { get; } = new();

        protected override void ConfigureAdditionalServices(IServiceCollection services)
        {
            IamClient
                .Setup(client => client.CheckPermissionAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("IAM unavailable"));
            services.AddScoped<IIamServiceClient>(_ => IamClient.Object);
        }
    }
}
