using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Asp.Versioning;
using Maliev.AccountingService.Api.Controllers;
using Maliev.AccountingService.Application.Authorization;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.Aspire.ServiceDefaults.IAM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace Maliev.AccountingService.Tests.Unit;

/// <summary>
/// Tests AccountingService's outbound workload authentication boundary.
/// </summary>
public sealed class ServiceAuthenticationWiringTests
{
    private const string ExpectedToken = "centrally-issued-accounting-token";

    /// <summary>
    /// AccountingService startup should opt into AuthService exchange and retain RabbitMQ IAM registration.
    /// </summary>
    [Fact]
    public void Program_RegistersAccountingExchangeWithoutLegacySigner()
    {
        var source = ReadRepositoryFile("Maliev.AccountingService.Api", "Program.cs");

        Assert.Contains("builder.AddAuthServiceTokenExchange(\"AccountingService\");", source, StringComparison.Ordinal);
        Assert.Contains("builder.AddAuthServiceIAMClient();", source, StringComparison.Ordinal);
        Assert.Contains(
            "builder.Services.AddIAMRegistration<AccountingIAMRegistrationService>(\"accounting\");",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain("AddIAMServiceClient", source, StringComparison.Ordinal);
    }

    /// <summary>
    /// The process identity should be exact and no local-signing services should resolve.
    /// </summary>
    [Fact]
    public void AuthServiceIamClient_RegistersExactIdentityWithoutLegacySigningServices()
    {
        var builder = CreateConfiguredBuilder();

        builder.AddAuthServiceTokenExchange("AccountingService");
        builder.AddAuthServiceIAMClient();

        using var provider = builder.Services.BuildServiceProvider();
        var identity = provider.GetRequiredService<ServiceProcessIdentity>();

        Assert.Equal("AccountingService", identity.ServiceName);
        Assert.Single(provider.GetServices<IIamServiceClient>());
        Assert.Null(provider.GetService<IServiceAccountTokenProvider>());
        Assert.Null(provider.GetService<ServiceAccountAuthenticationHandler>());
    }

    /// <summary>
    /// IAM permission checks should use the AuthService bearer on the exact POST route.
    /// </summary>
    [Fact]
    public async Task IamPermissionCheck_UsesAuthServiceExchangedBearerTokenOnExactPostRoute()
    {
        var builder = CreateConfiguredBuilder();
        var filter = new TrackingPrimaryHandlerFilter();
        builder.Services.AddSingleton<IHttpMessageHandlerBuilderFilter>(filter);

        builder.AddAuthServiceTokenExchange("AccountingService");
        builder.Services.AddSingleton<IAuthServiceTokenProvider>(new StubTokenProvider());
        builder.AddAuthServiceIAMClient();

        await using var provider = builder.Services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var iamClient = scope.ServiceProvider.GetRequiredService<IIamServiceClient>();

        var allowed = await iamClient.CheckPermissionAsync(
            $"accounting-test-{Guid.NewGuid():N}",
            AccountingPermissions.AccountsRead,
            cancellationToken: CancellationToken.None);

        var capture = filter.GetCapture("IAMService");
        Assert.True(allowed);
        Assert.Equal(new AuthenticationHeaderValue("Bearer", ExpectedToken), capture.Authorization);
        Assert.Equal(HttpMethod.Post, capture.Method);
        Assert.Equal(new Uri("https://iam.test/iam/v1/auth/check-permission"), capture.RequestUri);
    }

    /// <summary>
    /// Missing or malformed workload credentials should fail options validation during host startup.
    /// </summary>
    [Theory]
    [InlineData(null, null)]
    [InlineData("service-accounting-service", "short")]
    public async Task AuthServiceExchange_InvalidCredentials_FailsClosedAtHostStartup(
        string? clientId,
        string? clientSecret)
    {
        var builder = CreateConfiguredBuilder(clientId, clientSecret);
        builder.AddAuthServiceTokenExchange("AccountingService");

        using var host = builder.Build();

        await Assert.ThrowsAsync<OptionsValidationException>(() => host.StartAsync());
    }

    /// <summary>
    /// Accounting routes and permission policies are unchanged by the authentication migration.
    /// </summary>
    [Fact]
    public void AccountingEndpoints_RetainVersionedRoutesAndPermissionPolicies()
    {
        AssertControllerRoute<BulkImportController>("accounting/v{version:apiVersion}/bulk-import");
        AssertEndpoint<BulkImportController>(nameof(BulkImportController.ImportChartOfAccounts), "chart-of-accounts", "POST", AccountingPermissions.AccountsCreate);
        AssertEndpoint<BulkImportController>(nameof(BulkImportController.ImportOpeningBalances), "opening-balances", "POST", AccountingPermissions.JournalEntriesCreate);

        AssertControllerRoute<ChartOfAccountsController>("accounting/v{version:apiVersion}/chart-of-accounts");
        AssertEndpoint<ChartOfAccountsController>(nameof(ChartOfAccountsController.GetAccounts), null, "GET", AccountingPermissions.AccountsRead);
        AssertEndpoint<ChartOfAccountsController>(nameof(ChartOfAccountsController.GetAccountHierarchy), "hierarchy", "GET", AccountingPermissions.AccountsRead);
        AssertEndpoint<ChartOfAccountsController>(nameof(ChartOfAccountsController.GetAccountById), "{id:guid}", "GET", AccountingPermissions.AccountsRead);
        AssertEndpoint<ChartOfAccountsController>(nameof(ChartOfAccountsController.GetAccountByNumber), "by-number/{accountNumber}", "GET", AccountingPermissions.AccountsRead);
        AssertEndpoint<ChartOfAccountsController>(nameof(ChartOfAccountsController.CreateAccount), null, "POST", AccountingPermissions.AccountsCreate);
        AssertEndpoint<ChartOfAccountsController>(nameof(ChartOfAccountsController.UpdateAccount), "{id:guid}", "PUT", AccountingPermissions.AccountsUpdate);
        AssertEndpoint<ChartOfAccountsController>(nameof(ChartOfAccountsController.DeactivateAccount), "{id:guid}", "DELETE", AccountingPermissions.AccountsDelete);
        AssertEndpoint<ChartOfAccountsController>(nameof(ChartOfAccountsController.ValidateDeactivation), "{id:guid}/can-deactivate", "GET", AccountingPermissions.AccountsRead);

        AssertControllerRoute<JournalEntriesController>("accounting/v{version:apiVersion}/journal-entries");
        AssertEndpoint<JournalEntriesController>(nameof(JournalEntriesController.GetJournalEntries), null, "GET", AccountingPermissions.JournalEntriesRead);
        AssertEndpoint<JournalEntriesController>(nameof(JournalEntriesController.GetJournalEntry), "{id}", "GET", AccountingPermissions.JournalEntriesRead);
        AssertEndpoint<JournalEntriesController>(nameof(JournalEntriesController.CreateJournalEntry), null, "POST", AccountingPermissions.JournalEntriesCreate);
        AssertEndpoint<JournalEntriesController>(nameof(JournalEntriesController.PostJournalEntry), "{id}/post", "POST", AccountingPermissions.JournalEntriesPost);

        AssertControllerRoute<PeriodsController>("accounting/v{version:apiVersion}/periods");
        AssertEndpoint<PeriodsController>(nameof(PeriodsController.GetPeriods), null, "GET", AccountingPermissions.PeriodsRead);
        AssertEndpoint<PeriodsController>(nameof(PeriodsController.OpenPeriod), "open", "POST", AccountingPermissions.PeriodsOpen);
        AssertEndpoint<PeriodsController>(nameof(PeriodsController.ClosePeriod), "{id}/close", "POST", AccountingPermissions.PeriodsClose);
        AssertEndpoint<PeriodsController>(nameof(PeriodsController.ReopenPeriod), "{id}/reopen", "POST", AccountingPermissions.PeriodsReopen);

        AssertControllerRoute<ReconciliationController>("accounting/v{version:apiVersion}/reconciliation");
        AssertEndpoint<ReconciliationController>(nameof(ReconciliationController.RunReconciliation), "run", "GET", AccountingPermissions.ReconciliationRun);
        AssertEndpoint<ReconciliationController>(nameof(ReconciliationController.RunReconciliationPost), "run", "POST", AccountingPermissions.ReconciliationsRun);

        var legacyReconciliation = typeof(ReconciliationController).GetMethod(
            nameof(ReconciliationController.RunReconciliation),
            BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(legacyReconciliation?.GetCustomAttribute<ObsoleteAttribute>());

        AssertControllerRoute<ReportsController>("accounting/v{version:apiVersion}/reports");
        AssertEndpoint<ReportsController>(nameof(ReportsController.GetBalanceSheet), "balance-sheet", "GET", AccountingPermissions.ReportsBalanceSheet);
        AssertEndpoint<ReportsController>(nameof(ReportsController.GetIncomeStatement), "income-statement", "GET", AccountingPermissions.ReportsIncomeStatement);
        AssertEndpoint<ReportsController>(nameof(ReportsController.GetTrialBalance), "trial-balance", "GET", AccountingPermissions.ReportsTrialBalance);
        AssertEndpoint<ReportsController>(nameof(ReportsController.ExportReports), "export", "GET", AccountingPermissions.ReportsExport);
    }

    /// <summary>
    /// Every public controller action must declare an explicit permission guard.
    /// </summary>
    [Fact]
    public void AccountingControllerActions_AllDeclareExplicitPermissionGuards()
    {
        var actionMethods = typeof(PeriodsController).Assembly
            .GetTypes()
            .Where(type => type is { IsAbstract: false, IsPublic: true } && typeof(ControllerBase).IsAssignableFrom(type))
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(method => method.GetCustomAttributes<HttpMethodAttribute>().Any())
            .ToArray();

        Assert.Equal(24, actionMethods.Length);
        Assert.All(actionMethods, method =>
            Assert.NotNull(method.GetCustomAttribute<RequirePermissionAttribute>()));
    }

    private static HostApplicationBuilder CreateConfiguredBuilder(
        string? clientId = "service-accounting-service",
        string? clientSecret = "accounting-test-secret-with-at-least-32-bytes")
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            EnvironmentName = "Testing"
        });

        using var rsa = RSA.Create(2048);
        builder.Configuration["ServiceAuthentication:ClientId"] = clientId;
        builder.Configuration["ServiceAuthentication:ClientSecret"] = clientSecret;
        builder.Configuration["Services:AuthService:BaseUrl"] = "https://auth.test";
        builder.Configuration["Services:IAMService:BaseUrl"] = "https://iam.test";
        builder.Configuration["Jwt:PublicKey"] = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(rsa.ExportSubjectPublicKeyInfoPem()));
        builder.Configuration["Jwt:Issuer"] = "https://api.maliev.com";
        builder.Configuration["Jwt:Audience"] = "https://api.maliev.com";

        return builder;
    }

    private static void AssertControllerRoute<TController>(string expectedTemplate)
    {
        var controller = typeof(TController);
        Assert.NotNull(controller.GetCustomAttribute<ApiVersionAttribute>());
        Assert.Equal(expectedTemplate, controller.GetCustomAttribute<RouteAttribute>()?.Template);
    }

    private static void AssertEndpoint<TController>(
        string methodName,
        string? expectedTemplate,
        string expectedVerb,
        string expectedPermission)
    {
        var method = typeof(TController).GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);
        var route = method.GetCustomAttributes<HttpMethodAttribute>().Single();
        Assert.Equal(expectedTemplate, route.Template);
        Assert.Contains(expectedVerb, route.HttpMethods);
        Assert.Equal(expectedPermission, method.GetCustomAttribute<RequirePermissionAttribute>()?.Permission);
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

    private sealed class StubTokenProvider : IAuthServiceTokenProvider
    {
        public Task<string> GetTokenAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(ExpectedToken);
    }

    private sealed class AuthorizationCaptureHandler : HttpMessageHandler
    {
        public AuthenticationHeaderValue? Authorization { get; private set; }
        public HttpMethod? Method { get; private set; }
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Authorization = request.Headers.Authorization;
            Method = request.Method;
            RequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"allowed\":true}", Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class TrackingPrimaryHandlerFilter : IHttpMessageHandlerBuilderFilter
    {
        private readonly Dictionary<string, AuthorizationCaptureHandler> _captures = new(StringComparer.Ordinal);

        public AuthorizationCaptureHandler GetCapture(string clientName) => _captures[clientName];

        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next) => builder =>
        {
            next(builder);
            var clientName = builder.Name
                ?? throw new InvalidOperationException("Every HttpClientFactory handler must have a client name.");
            for (var index = builder.AdditionalHandlers.Count - 1; index >= 0; index--)
            {
                if (builder.AdditionalHandlers[index].GetType().FullName?.Contains(
                        "ServiceDiscovery",
                        StringComparison.Ordinal) == true ||
                    builder.AdditionalHandlers[index].GetType().FullName?.Contains(
                        "ResolvingHttpDelegatingHandler",
                        StringComparison.Ordinal) == true)
                {
                    builder.AdditionalHandlers.RemoveAt(index);
                }
            }

            var capture = new AuthorizationCaptureHandler();
            _captures[clientName] = capture;
            builder.PrimaryHandler = capture;
        };
    }
}
