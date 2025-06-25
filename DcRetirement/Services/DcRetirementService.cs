using System;
using System.Threading.Tasks;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.DcRetirement.Services;

public class DcRetirementService : IDcRetirementService
{
    private readonly IMemberRepository _memberRepository;
    private readonly ICalculationsRepository _calculationsRepository;
    private readonly IMdpUnitOfWork _mdpUnitOfWork;
    private readonly ILogger<DcRetirementService> _logger;

    public DcRetirementService(IMemberRepository memberRepository,
        ICalculationsRepository calculationsRepository,
        IMdpUnitOfWork mdpUnitOfWork,
        ILogger<DcRetirementService> logger)
    {
        _memberRepository = memberRepository;
        _calculationsRepository = calculationsRepository;
        _mdpUnitOfWork = mdpUnitOfWork;
        _logger = logger;
    }

    public async Task<Error?> ResetQuote(string referenceNumber, string businessGroup)
    {
        var utcNow = DateTimeOffset.UtcNow;
        var member = (await _memberRepository.FindMember(referenceNumber, businessGroup)).Value();
        if (!member.IsSchemeDc())
        {
            _logger.LogWarning("Non DC member tried to use this method");
            return Error.New("Method supports only DC members");
        }

        var persistedCalculation = await _calculationsRepository.Find(referenceNumber, businessGroup);
        await persistedCalculation.IfSomeAsync(async c =>
        {
            c.UpdateRetirementV2(string.Empty, string.Empty, DateTime.SpecifyKind(member.DcRetirementDate(utcNow), DateTimeKind.Utc), utcNow);
            await _mdpUnitOfWork.Commit();
        });

        _logger.LogInformation("DC retirement quote reset was successfully completed.");
        return null;
    }
}