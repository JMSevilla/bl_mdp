using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WTW.MdpService.Infrastructure.BankService;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.DeloreanAuthentication;
using WTW.MdpService.Infrastructure.EpaService;
using WTW.MdpService.Infrastructure.IdvService;
using WTW.MdpService.Infrastructure.IpaService;
using WTW.MdpService.Infrastructure.MemberService;
using WTW.MdpService.Infrastructure.MemberWebInteractionService;
using WTW.MdpService.Infrastructure.RetryPolicy;
using WTW.MdpService.Infrastructure.TelephoneNoteService;
using WTW.Web;
using WTW.Web.Attributes;
using WTW.Web.Authentication;
using WTW.Web.Models.Internal;

namespace WTW.MdpService;

public static class OptionsConfigurations
{
    public static void AddOptions(IServiceCollection services, ConfigurationManager configuration)
    {
        BindAndValidateOptions<DeloreanAuthenticationOptions>(services, configuration, MdpConstants.ConfigSection.DeloreanAuthentication);
        BindAndValidateOptions<SingleAuthAuthenticationOptions>(services, configuration, MdpConstants.ConfigSection.SingleAuth);
        BindAndValidateOptions<MemberServiceOptions>(services, configuration, MdpConstants.ConfigSection.MemberService);
        BindAndValidateOptions<MemberWebInteractionServiceOptions>(services, configuration, MdpConstants.ConfigSection.MemberWebInteractionService);
        BindAndValidateOptions<DatePickerConfigOptions>(services, configuration, MdpConstants.ConfigSection.DatePickerConfig);
        BindAndValidateOptions<EpaServiceOptions>(services, configuration, MdpConstants.ConfigSection.EpaService);
        BindAndValidateOptions<BankServiceOptions>(services, configuration, MdpConstants.ConfigSection.BankService);
        BindAndValidateOptions<CalculationServiceOptions>(services, configuration, MdpConstants.ConfigSection.CalculationService);
        BindAndValidateOptions<RetryPolicyOptions>(services, configuration, MdpConstants.ConfigSection.RetryPolicy);
        BindAndValidateOptions<IdvServiceOptions>(services, configuration, MdpConstants.ConfigSection.IdentityVerification);
        BindAndValidateOptions<TelephoneNoteServiceOptions>(services, configuration, MdpConstants.ConfigSection.TelephoneNoteService);
        BindAndValidateOptions<IpaServiceOptions>(services, configuration, MdpConstants.ConfigSection.IpaService);

        services.AddOptions<ResponseHashOptions>()
            .Configure<IConfiguration>((optionModel, configuration) =>
            {
                optionModel.PrivateEncryptionKey = configuration[MdpConstants.ConfigSection.PrivateEncryptionKey];
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    private static void BindAndValidateOptions<TOptions>(IServiceCollection services, IConfiguration configuration, string sectionName)
        where TOptions : class
    {
        services.AddOptions<TOptions>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}
