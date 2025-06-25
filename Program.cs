using System;
using System.Net.Http;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using ServiceHost.MessageBroker;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.System.Text.Json;
using WTW.MdpService;
using WTW.MdpService.Application;
using WTW.MdpService.BackgroundTasks;
using WTW.MdpService.BankAccounts.Services;
using WTW.MdpService.BereavementJourneys;
using WTW.MdpService.ContactsConfirmation;
using WTW.MdpService.Content.V2;
using WTW.MdpService.DcRetirement.Services;
using WTW.MdpService.Documents;
using WTW.MdpService.Domain.Common.UploadedDocuments;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.IdentityVerification.Services;
using WTW.MdpService.Infrastructure.ApplyFinancials;
using WTW.MdpService.Infrastructure.Aws;
using WTW.MdpService.Infrastructure.BereavementDb;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.CasesApi;
using WTW.MdpService.Infrastructure.Charts;
using WTW.MdpService.Infrastructure.Consumers;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.Db;
using WTW.MdpService.Infrastructure.Edms;
using WTW.MdpService.Infrastructure.EdmsIndexing;
using WTW.MdpService.Infrastructure.EmailConfirmation;
using WTW.MdpService.Infrastructure.Gbg;
using WTW.MdpService.Infrastructure.Geolocation;
using WTW.MdpService.Infrastructure.Investment;
using WTW.MdpService.Infrastructure.Investment.AnnuityBroker;
using WTW.MdpService.Infrastructure.JobScheduler;
using WTW.MdpService.Infrastructure.Journeys;
using WTW.MdpService.Infrastructure.Journeys.Documents;
using WTW.MdpService.Infrastructure.MdpApi;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Infrastructure.MemberDb.Documents;
using WTW.MdpService.Infrastructure.MemberDb.IfaReferrals;
using WTW.MdpService.Infrastructure.PdfGenerator;
using WTW.MdpService.Infrastructure.Redis;
using WTW.MdpService.Infrastructure.RetryPolicy;
using WTW.MdpService.Infrastructure.SmsConfirmation;
using WTW.MdpService.Infrastructure.Templates.Bereavement;
using WTW.MdpService.Infrastructure.Templates.Common;
using WTW.MdpService.Infrastructure.Templates.GenericJourneys;
using WTW.MdpService.Infrastructure.Templates.RetirementApplication.Calculations;
using WTW.MdpService.Infrastructure.Templates.RetirementApplication.Submit;
using WTW.MdpService.Infrastructure.Templates.SingleAuth;
using WTW.MdpService.Infrastructure.Templates.TransferApplication;
using WTW.MdpService.Infrastructure.TokenService;
using WTW.MdpService.Journeys.Submit.Services;
using WTW.MdpService.Journeys.Submit.Services.DocumentsRenderer;
using WTW.MdpService.Retirement;
using WTW.MdpService.RetirementJourneys;
using WTW.MdpService.SingleAuth;
using WTW.MdpService.SingleAuth.Services;
using WTW.MdpService.Templates;
using WTW.MdpService.TransferJourneys;
using WTW.Web;
using WTW.Web.Authentication;
using WTW.Web.Authorization;
using WTW.Web.Caching;
using WTW.Web.Clients;
using WTW.Web.Errors;
using WTW.Web.Extensions;
using WTW.Web.Logging;
using WTW.Web.ModelBinders;
using WTW.Web.OpenAPI;
using WTW.Web.Serialization;
using WTW.Web.Utilities;
using WTW.Web.Validation;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;
builder.RemoveKestrelResponseHeader();
builder.ConfigureLogging(configuration.GetValue<string>("LocalSeqUrl"));

services.AddHealthChecks();
services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter());
    options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ApiError), StatusCodes.Status401Unauthorized));
    options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ApiError), StatusCodes.Status500InternalServerError));
    options.ModelBinderProviders.Insert(0, new EscapeHtmlModelBinderProvider());
}).UseSerialization();

services.UseValidation();
if (builder.Environment.EnvironmentName != "prod")
    services.UseSwaggerGen(forMdpApiOnly: true);

var singleAuthOptions = new SingleAuthAuthenticationOptions();
configuration.GetSection(MdpConstants.ConfigSection.SingleAuth).Bind(singleAuthOptions);
services.AddMpdServiceAuthentication(configuration, singleAuthOptions);
services.AddMpdServiceAuthorization(singleAuthOptions);
// Configure options
OptionsConfigurations.AddOptions(builder.Services, builder.Configuration);
// Register HttpClients
HttpClientRegistrations.AddHttpClients(builder.Services, builder.Configuration);
builder.Services.AddMessageBroker(builder.Configuration, typeof(SendEmailConsumer));
builder.Services.AddHttpContextAccessor();
builder.Services.AddHeaderPropagation();


RegisterDependencies(services, configuration);
RegisterHostedServices(services);

var app = builder.Build();
RuntimeMetrics.Configure();
IronPdf.License.LicenseKey = configuration.GetValue<string>("IRON_PDF_LICENCE_KEY");
IronPdf.Installation.LinuxAndDockerDependenciesAutoConfig = false;
IronPdf.Installation.ChromeGpuMode = IronPdf.Engines.Chrome.ChromeGpuModes.Disabled;
IronPdf.Installation.Initialize();

#warning TODO: need to do migration outside of the process
using (var scope = app.Services.CreateScope())
{
    var isMigrationEnabled = configuration.GetValue<bool>("IsMigrationEnabled");
    if (isMigrationEnabled)
    {
        var mdpContext = scope.ServiceProvider.GetRequiredService<MdpDbContext>();
        var bereavmentContext = scope.ServiceProvider.GetRequiredService<BereavementDbContext>();
        mdpContext.Database.Migrate();
        bereavmentContext.Database.Migrate();
    }
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.MapHealthChecks("/health").WithMetadata(new AllowAnonymousAttribute());
app.UseErrorHandling();
app.UseSwagger();
app.UseHeaderPropagation();
app.UseHttpsRedirection();
if (app.Environment.EnvironmentName != "prod")
{
    app.UseSwaggerUI(config =>
    {
        config.SwaggerEndpoint("/swagger/v1/swagger.json", "MdpService v1");
        config.DisplayRequestDuration();
    });
}

app.UseMiddleware<MdpAuthenticationMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.Run();

void RegisterDependencies(IServiceCollection services, IConfiguration configuration)
{
    builder.Services.AddTransient<IClaimsTransformation, SingleAuthClaimsTransformation>();
    services.AddTransient<IMdpAuthenticationService, MdpAuthenticationService>();
    builder.Services.AddScoped<ISingleAuthService, SingleAuthService>();
    builder.Services.AddScoped<IRegistrationEmailTemplate, RegistrationEmailTemplate>();
    builder.Services.AddScoped<IIdvService, IdvService>();
    builder.Services.AddScoped<IBankService, BankService>();

    var retryPolicySettings = new RetryPolicyOptions();
    configuration.GetSection(MdpConstants.ConfigSection.RetryPolicy).Bind(retryPolicySettings);

    services.AddDbContext<MemberDbContext>(options =>
        options
        .UseLazyLoadingProxies()
        .UseOracle(new OracleConnection { ConnectionString = configuration.GetConnectionString("MemberDb-PMSPAD"), KeepAlive = true })
        .AddInterceptors(new AuditingInterceptor()));
    services.AddDbContext<MdpDbContext>(options =>
        options
        .UseLazyLoadingProxies()
        .UseNpgsql(configuration.GetConnectionString("Mdp")));
    services.AddDbContext<BereavementDbContext>(options =>
       options
       .UseLazyLoadingProxies()
       .UseNpgsql(configuration.GetConnectionString("Bereavement")));

    services.AddScoped<ICache>(s => new SafeCache(
        new RedisCache(
            s.GetService<IRedisDatabase>(),
            s.GetService<IRedisClient>(),
            s.GetService<ILogger<RedisCache>>()),
        s.GetService<ILogger<SafeCache>>()));

    services.AddScoped(s => new CalculationsRedisCache(
        s.GetService<ICache>(),
        s.GetService<ILogger<CalculationsRedisCache>>()));
    services.AddScoped<ICalculationsRedisCache, CalculationsRedisCache>();

    services.AddScoped<MemberRepository>();
    services.AddScoped<IMemberRepository>(sp => sp.GetRequiredService<MemberRepository>());
    services.AddScoped<IDocumentsRepository, DocumentsRepository>();
    services.AddScoped<ISystemRepository, SystemRepository>();
    services.AddScoped<IObjectStatusRepository, ObjectStatusRepository>();
    services.AddScoped<RetirementCaseRepository>();
    services.AddScoped<IdvHeaderRepository>();
    services.AddScoped<IdvDetailRepository>();
    services.AddScoped<IIfaReferralHistoryRepository, IfaReferralHistoryRepository>();
    services.AddScoped<IIfaReferralRepository, IfaReferralRepository>();
    services.AddScoped<IQuoteSelectionJourneyRepository, QuoteSelectionJourneyRepository>();
    services.AddScoped<TransferCalculationRepository>();
    services.AddScoped<ITransferCalculationRepository>(sp => sp.GetRequiredService<TransferCalculationRepository>());
    services.AddSingleton(configuration.GetSection("AuthenticationToken").Get<AuthenticationSettings>());
    services.AddSingleton<IPdfGenerator, PdfGenerator>();

    services.AddHttpClient(
        "CalculationAPI",
        o =>
        {
            o.BaseAddress = new Uri(configuration["CalculationApi:BaseUrl"]);
        })
        .AddPolicyHandler(GenericApiPolicies.RetryPolicy(retryPolicySettings))
        .AddPolicyHandler(CalculationApiPolicies.RetryPolicy(retryPolicySettings))
        .AddPolicyHandler(GenericApiPolicies.TimeoutPolicy(TimeSpan.FromSeconds(int.Parse(configuration["CalculationApi:TimeOutInSeconds"]))))
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
#warning Install cert
            UseProxy = false,
            UseCookies = false,
            ServerCertificateCustomValidationCallback = (sender, cert, chaun, ssPolicyError) => true,
        });

    services.AddHttpClient(
       "TransferCalculationApi",
       o =>
       {
           o.BaseAddress = new Uri(configuration["CalculationApi:BaseUrl"]);
           o.Timeout = TimeSpan.FromSeconds(120);
       })
       .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
       {
#warning Install cert
           UseProxy = false,
           UseCookies = false,
           ServerCertificateCustomValidationCallback = (sender, cert, chaun, ssPolicyError) => true,
       });

    services.AddScoped(s => new CalculationsClient(
        s.GetService<IHttpClientFactory>().CreateClient("CalculationAPI"),
        s.GetService<IHttpClientFactory>().CreateClient("TransferCalculationApi"),
        configuration,
        s.GetService<IHostEnvironment>(),
        s.GetService<ILogger<CalculationsClient>>(),
        s.GetService<IOptionsSnapshot<CalculationServiceOptions>>()
        ));
    services.AddScoped<ICalculationsClient>(s => new CachedCalculationsClient(
        new CalculationsClient(
        s.GetService<IHttpClientFactory>().CreateClient("CalculationAPI"),
        s.GetService<IHttpClientFactory>().CreateClient("TransferCalculationApi"),
        configuration,
        s.GetService<IHostEnvironment>(),
        s.GetService<ILogger<CalculationsClient>>(),
        s.GetService<IOptionsSnapshot<CalculationServiceOptions>>()),
        s.GetService<ICache>(),
        int.Parse(configuration["CalculationApi:CacheExpiresInMs"]),
        s.GetService<ILogger<CachedCalculationsClient>>()));

    services.AddHttpClient("ApplyFinancials", o => o.BaseAddress = new Uri(configuration["ApplyFinancials:BaseUrl"]));
    services.AddScoped<IApplyFinancialsClient>(s => new ApplyFinancialsClient(
        s.GetService<IHttpClientFactory>().CreateClient("ApplyFinancials"),
        configuration["ApplyFinancials:UserName"],
        configuration["ApplyFinancials:Password"]));

    services.AddHttpClient("CasesApi", o => o.BaseAddress = new Uri(configuration["CasesApi:BaseUrl"]));
    services.AddScoped<ICasesClient>(s => new CasesClient(
        s.GetService<IHttpClientFactory>().CreateClient("CasesApi"),
        s.GetService<ILogger<CasesClient>>()));

    services.AddHttpClient("LoqateApi", client => client.BaseAddress = new Uri(configuration["LoqateApi:BaseUrl"]));
    services.AddScoped<ILoqateApiClient>(s =>
        new LoqateApiClient(s.GetService<IHttpClientFactory>().CreateClient("LoqateApi"),
            new LoqateApiConfiguration { ApiKey = configuration["LoqateApi:ApiKey"] },
            s.GetService<ILogger<LoqateApiClient>>()));

    services.AddHttpClient("Edms", o => o.BaseAddress = new Uri(configuration["Edms:BaseUrl"]))
        .AddPolicyHandler(GenericApiPolicies.RetryPolicy(retryPolicySettings));
    services.AddScoped(s => new EdmsClient(
        s.GetService<IHttpClientFactory>().CreateClient("Edms"),
        configuration["Edms:UserName"],
        configuration["Edms:Password"],
        s.GetService<ILogger<EdmsClient>>()));
    services.AddScoped<IEdmsClient>(sp => sp.GetRequiredService<EdmsClient>());

    services.AddHttpClient("Gbg", o => o.BaseAddress = new Uri(configuration["Gbg:BaseUrl"]))
        .AddPolicyHandler(GenericApiPolicies.RetryPolicy(retryPolicySettings));
    services.AddHttpClient("GbgScan", o => o.BaseAddress = new Uri(configuration["GbgScan:BaseUrl"]))
        .AddPolicyHandler(GenericApiPolicies.RetryPolicy(retryPolicySettings));
    services.AddHttpClient("GbgAdmin", o => o.BaseAddress = new Uri(configuration["GbgAdmin:BaseUrl"]))
        .AddPolicyHandler(GenericApiPolicies.RetryPolicy(retryPolicySettings));
    services.AddScoped(s => new GbgClient(
        s.GetService<IHttpClientFactory>().CreateClient("Gbg"),
        configuration["Gbg:UserName"],
        configuration["Gbg:Password"]));
    services.AddScoped(s => new GbgScanClient(
        s.GetService<IHttpClientFactory>().CreateClient("GbgScan"),
        configuration["GbgScan:UserName"],
        configuration["GbgScan:Password"]));
    services.AddScoped(s => new GbgAdminClient(
        s.GetService<IHttpClientFactory>().CreateClient("GbgAdmin"),
        configuration["GbgAdmin:UserName"],
        configuration["GbgAdmin:Password"]));
    services.AddScoped<ICachedGbgClient>(s => new CachedGbgClient(
        s.GetService<GbgClient>(),
        s.GetService<ICache>(),
        int.Parse(configuration["Gbg:CacheExpiresInMs"])));
    services.AddScoped<ICachedGbgScanClient>(s => new CachedGbgScanClient(
        s.GetService<GbgScanClient>(),
        s.GetService<ICache>(),
        int.Parse(configuration["GbgScan:CacheExpiresInMs"])));
    services.AddScoped<ICachedGbgAdminClient>(s => new CachedGbgAdminClient(
        s.GetService<GbgAdminClient>(),
        s.GetService<ICache>(),
        int.Parse(configuration["GbgAdmin:CacheExpiresInMs"])));

    services.AddHttpClient("JobScheduler", o => o.BaseAddress = new Uri(configuration["JobScheduler:BaseUrl"]));
    services.AddScoped(s => new JobSchedulerClient(
        s.GetService<IHttpClientFactory>().CreateClient("JobScheduler"),
        configuration["JobScheduler:UserName"],
        configuration["JobScheduler:Password"]));
    services.AddScoped<IJobSchedulerClient>(sp => sp.GetRequiredService<JobSchedulerClient>());
    services.AddSingleton(new JobSchedulerConfiguration(configuration["JobScheduler:JobChainEnv"]));
    services.AddScoped<IAccessTokenHelper, AccessTokenHelper>();
    services.AddScoped<IMdpClient>(s => new MdpClient(
            new HttpClient(),
            s.GetService<ILogger<MdpClient>>(),
            s.GetService<IHttpContextAccessor>()
        ));

    services.AddHttpClient("Content", o => o.BaseAddress = new Uri(configuration["Content:BaseUrl"]))
        .AddPolicyHandler(GenericApiPolicies.RetryPolicy(retryPolicySettings));
    services.AddScoped(s => new ContentClient(
        s.GetService<IHttpClientFactory>().CreateClient("Content"),
        s.GetService<ILogger<ContentClient>>()));
    services.AddScoped<IContentClient>(sp => sp.GetRequiredService<ContentClient>());
    services.AddScoped<IChartsTemporaryClient, ChartsTemporaryClient>();

    services.AddHttpClient("Investment", o => o.BaseAddress = new Uri(configuration["InvestmentService:BaseUrl"]));
    services.AddScoped<IInvestmentServiceClient>(s =>
        new InvestmentServiceClient(s.GetService<IHttpClientFactory>().CreateClient("Investment"),
        s.GetService<ICachedTokenServiceClient>(),
        s.GetService<ILogger<InvestmentServiceClient>>()));

    services.AddHttpClient("TokenService", o => o.BaseAddress = new Uri(configuration["TokenService:BaseUrl"]));
    services.AddSingleton(new TokenServiceClientConfiguration(
        configuration["TokenService:GrantType"],
        configuration["TokenService:ClientId"],
        configuration["TokenService:ClientSecret"],
        configuration.GetSection("TokenService:Scopes").Get<string[]>()));
    services.AddScoped<ICachedTokenServiceClient>(s => new CachedTokenServiceClient(
        new TokenServiceClient(
            s.GetService<IHttpClientFactory>().CreateClient("TokenService"),
            s.GetService<TokenServiceClientConfiguration>(),
            s.GetService<ILogger<TokenServiceClient>>()),
        s.GetService<ICache>()));

    services.AddScoped<RetirementJourneyRepository>();
    services.AddScoped<IRetirementJourneyRepository>(sp => sp.GetRequiredService<RetirementJourneyRepository>());
    services.AddScoped<TransferJourneyRepository>();
    services.AddScoped<ITransferJourneyRepository>(sp => sp.GetRequiredService<TransferJourneyRepository>());
    services.AddScoped<JourneyDocumentsRepository>();
    services.AddScoped<IJourneyDocumentsRepository>(sp => sp.GetRequiredService<JourneyDocumentsRepository>());
    services.AddScoped<ContactConfirmationRepository>();
    services.AddScoped<UserQueryPromptRepository>();
    services.AddScoped<EventRtiRepository>();
    services.AddScoped<CalculationsRepository>();
    services.AddScoped<ICalculationsRepository>(sp => sp.GetRequiredService<CalculationsRepository>());
    services.AddScoped<ITenantRetirementTimelineRepository, TenantRetirementTimelineRepository>();
    services.AddScoped<IBankHolidayRepository, BankHolidayRepository>();
    services.AddScoped<PensionWiseRepository>();
    services.AddScoped<ITenantRepository, TenantRepository>();
    services.AddScoped<CalculationHistoryRepository>();
    services.AddScoped<ICalculationHistoryRepository>(sp => sp.GetRequiredService<CalculationHistoryRepository>());
    services.AddScoped<BereavementUnitOfWork>();
    services.AddScoped<IBereavementUnitOfWork>(sp => sp.GetRequiredService<BereavementUnitOfWork>());
    services.AddScoped<MdpUnitOfWork>();
    services.AddScoped<IMdpUnitOfWork>(sp => sp.GetRequiredService<MdpUnitOfWork>());
    services.AddScoped<MemberDbUnitOfWork>();
    services.AddScoped<IMemberDbUnitOfWork>(sp => sp.GetRequiredService<MemberDbUnitOfWork>());
    services.AddScoped<MemberIfaReferral>();
    services.AddScoped<IMemberIfaReferral>(sp => sp.GetRequiredService<MemberIfaReferral>());
    services.AddScoped<IIfaConfigurationRepository, IfaConfigurationRepository>();
    services.AddScoped<BereavementJourneyRepository>();
    services.AddScoped<IBereavementJourneyRepository>(sp => sp.GetRequiredService<BereavementJourneyRepository>());
    services.AddScoped<IBereavementContactConfirmationRepository, BereavementContactConfirmationRepository>();
    services.AddScoped<IJourneysRepository, JourneysRepository>();
    services.AddScoped<IJourneyService, JourneyService>();
    services.AddScoped<IRetirementDatesService, RetirementDatesService>();
    services.AddScoped<IWebChatFlagRepository, WebChatFlagRepository>();
    services.AddScoped<IInvestmentQuoteService, InvestmentQuoteService>();

    var outboxSection = configuration.GetSection("Outbox");
    if (outboxSection == null)
        throw new InvalidOperationException("'Outbox' section is not specified in settings");

    services.AddScoped<MailKit.Net.Smtp.ISmtpClient, MailKit.Net.Smtp.SmtpClient>();
    services.Configure<OutboxSettings>(outboxSection);
    services.AddScoped<IEmailClient, SmtpEmailClient>();
    services.AddScoped<EmailConfirmationSmtpClient>();
    services.AddScoped<IEmailConfirmationSmtpClient>(sp => sp.GetRequiredService<EmailConfirmationSmtpClient>());
    services.AddSingleton(new ContactsConfirmationConfiguration(
        int.Parse(configuration["ContactsConfirmation:EmailTokenExpiresInMin"]),
        int.Parse(configuration["ContactsConfirmation:MobilePhoneTokenExpiresInMin"]),
        int.Parse(configuration["ContactsConfirmation:MaxMobileConfirmationAttemptCount"]),
        int.Parse(configuration["ContactsConfirmation:MaxEmailConfirmationAttemptCount"])));
    services.AddSingleton(new RetirementJourneyConfiguration(int.Parse(configuration["RetirementJourneyDaysToExpire"])));
    services.AddSingleton(new BereavementJourneyConfiguration(
        int.Parse(configuration["BereavementJourney:ValidityPeriodInMin"]),
        int.Parse(configuration["BereavementJourney:ExpiredJourneysRemovalPeriodInMin"]),
        int.Parse(configuration["BereavementJourney:EmailTokenExpiresInMin"]),
        int.Parse(configuration["BereavementJourney:MaxEmailConfirmationAttemptCount"]),
        int.Parse(configuration["BereavementJourney:EmailLockPeriodInMin"]),
        int.Parse(configuration["BereavementJourney:FailedJourneyValidityPeriodInMin"])));
    services.AddScoped<IMessageClient>(_ => new MessageBirdClient(configuration["MessageBirdAccessKey"]));
    services.AddScoped<IBackgroundTask, RetirementJourneyPostIndexTask>();
    services.AddScoped<IBackgroundTask, ExpiredBerevementJourneysRemovalTask>();
    services.AddScoped<EdmsDocumentsIndexing>();
    services.AddScoped<IEdmsDocumentsIndexing>(sp => sp.GetRequiredService<EdmsDocumentsIndexing>());
    services.AddScoped<BereavementCase>();
    services.AddScoped<ITransferCase, TransferCase>();
    services.AddScoped<IBereavementCase, BereavementCase>();
    services.AddScoped<TransferJourneyContactFactory>();
    services.AddScoped<ITransferJourneyContactFactory>(sp => sp.GetRequiredService<TransferJourneyContactFactory>());
    services.AddScoped<ITemplateService, TemplateService>();
    services.AddScoped<ITemplateProvider, TemplateProvider>();
    services.AddScoped<IGenericTemplateContent, GenericTemplateContent>();
    services.AddScoped<RetirementPostIndexEventRepository>();
    services.AddScoped<IRetirementPostIndexEventRepository>(sp => sp.GetRequiredService<RetirementPostIndexEventRepository>());
    services.AddScoped<ITransferOutsideAssure, TransferOutsideAssure>();
    services.AddScoped<IApplicationInitialization, ApplicationInitialization>();
    services.AddAWSService<IAmazonS3>();
    services.AddTransient<IAwsClient, AwsClient>();
    services.AddSingleton(new OtpSettings(configuration["SuppressOTPCheck"] == null ? false : bool.Parse(configuration["SuppressOTPCheck"])));
    services.AddSingleton(new PublicApiSetting(new Uri(configuration["PublicApi:BaseUrl"])));
    services.AddScoped<ITransferV2Template, TransferV2Template>();
    services.AddSingleton<IBereavementTemplate, BereavementTemplate>();
    services.AddSingleton<ITransferJourneySubmitEmailTemplate, TransferJourneySubmitEmailTemplate>();
    services.AddScoped<ICaseService, CaseService>();
    services.AddScoped<IGenericJourneysTemplate, GenericJourneysTemplate>();
    services.AddScoped<ICaseRequestFactory, CaseRequestFactory>();
    services.AddScoped<IDocumentRenderer, DocumentRenderer>();
    services.AddScoped<IRetirementCalculationsPdf, RetirementCalculationsPdf>();
    services.AddScoped<IDocumentsUploaderService, DocumentsUploaderService>();
    services.AddScoped<IGbgDocumentService, GbgDocumentService>();
    services.AddScoped<IJsonConversionService, JsonConversionService>();
    services.AddScoped<ICalculationsParser, CalculationsParser>();
    services.AddScoped<IRetirementApplicationQuotesV2, RetirementApplicationQuotesV2>();
    services.AddScoped<IRetirementCalculationQuotesV2, RetirementCalculationQuotesV2>();
    services.AddScoped<IRetirementApplicationSubmissionTemplate, RetirementApplicationSubmissionTemplate>();
    services.AddScoped<IRetirementApplicationCalculationTemplate, RetirementCalculationTemplate>();
    services.AddScoped<IGenericJourneyService, GenericJourneyService>();
    services.AddScoped<IDocumentsRendererDataFactory, DocumentsRendererDataFactory>();
    services.AddScoped<IUploadedDocumentFactory, UploadedDocumentFactory>();
    services.AddScoped<IDcRetirementService, DcRetirementService>();
    services.AddScoped<IRetirementService, RetirementService>();
    services.AddScoped<IAccessKeyService, AccessKeyService>();
    services.AddScoped<IAccessKeyWordingFlagsService, AccessKeyWordingFlagsService>();
    services.AddScoped<IWebChatFlagService, WebChatFlagService>();
    services.AddScoped<IRetirementAccessKeyDataService, RetirementAccessKeyDataService>();
    services.AddScoped<IContentService, ContentService>();
    services.AddScoped<ICaseDocumentsService, CaseDocumentsService>();
    services.AddSingleton<ICmsDataParser, CmsDataParser>();
    services.AddScoped<ITemplateDataService, TemplateDataService>();
    services.AddScoped<IGenericJourneyDetails, GenericJourneyDetails>();
    services.AddScoped<IMdpHashHelper, MdpHashHelper>();
    services.AddScoped<IRateOfReturnService, RateOfReturnService>();
    services.AddScoped<IJourneyDocumentsHandlerService, JourneyDocumentsHandlerService>();

    var username = configuration["Redis:Username"];
    var password = configuration["Redis:Password"];
    var keyPrefix = configuration["Redis:KeyPrefix"];
    services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(new RedisConfiguration()
    {
        Hosts = new RedisHost[]
        {
                new RedisHost
                {
                    Host = configuration["Redis:Host"],
                    Port = int.Parse(configuration["Redis:Port"])
                }
        },
        Ssl = bool.Parse(configuration["Redis:UseSsl"]),
        User = !string.IsNullOrEmpty(username) ? username : null,
        Password = !string.IsNullOrEmpty(password) ? password : null,
        KeyPrefix = !string.IsNullOrEmpty(keyPrefix) ? $"{keyPrefix}:" : string.Empty,
        SyncTimeout = int.Parse(configuration["Redis:SyncTimeout"])
    });

    services.AddScoped<LoggingHandler>();
    builder.Services.AddSingleton<IDocumentFactory, RetirementDocumentFactory>();
    builder.Services.AddSingleton<IDocumentFactory, TransferDocumentFactory>();
    builder.Services.AddSingleton<IDocumentFactory, TransferV2DocumentFactory>();
    builder.Services.AddSingleton<IDocumentFactory, TransferV2OutsideAssureQuoteLockDocumentFactory>();
    builder.Services.AddSingleton<IDocumentFactory, RetirementQuoteRequestFactory>();
    builder.Services.AddSingleton<IDocumentFactory, TransferQuoteRequestFactory>();
    builder.Services.AddSingleton<IDocumentFactory, DcRetirementDocumentFactory>();
    builder.Services.AddSingleton<IDocumentFactory, RetirementQuoteWithoutCaseDocumantFactory>();
    services.AddSingleton<IDocumentFactoryProvider, DocumentFactoryProvider>();
    services.AddSingleton<IDatabaseConnectionParser>(serviceProvider =>
        new DatabaseConnectionParser(
            configuration.GetConnectionString("MemberDb-PMSPAD"),
            serviceProvider.GetRequiredService<ILogger<DatabaseConnectionParser>>()));
    services.ConfigureAll<HttpClientFactoryOptions>(options =>
    {
        options.HttpMessageHandlerBuilderActions.Add(builder =>
        {
            builder.AdditionalHandlers.Add(builder.Services.GetRequiredService<LoggingHandler>());
        });
    });
}

void RegisterHostedServices(IServiceCollection services)
{
    services.AddHostedService<BackgroundTasksService>();
}
