using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.DeloreanAuthentication;
using WTW.MdpService.Infrastructure.EpaService;
using WTW.MdpService.Infrastructure.MemberService;
using WTW.MessageBroker.Common;
using WTW.MessageBroker.Contracts.Mdp;
using WTW.Web;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.SingleAuth.Services;

public class SingleAuthService : ISingleAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SingleAuthService> _logger;
    private readonly IDeloreanAuthenticationClient _authenticationClient;
    private readonly IMemberServiceClient _memberClient;
    private readonly IWtwPublisher _publisher;
    private readonly IContentService _contentService;
    private readonly IEpaServiceClient _epaClient;

    public SingleAuthService(IHttpContextAccessor httpContextAccessor, ILogger<SingleAuthService> logger, IDeloreanAuthenticationClient authenticationClient,
        IMemberServiceClient memberClient, IWtwPublisher publisher, IContentService contentService, IEpaServiceClient epaClient)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _authenticationClient = authenticationClient;
        _memberClient = memberClient;
        _publisher = publisher;
        _contentService = contentService;
        _epaClient = epaClient;
    }

    public async Task<Either<Error, bool>> RegisterUser(string tenantUrl)
    {
        var correlationId = _httpContextAccessor.HttpContext.Request.Headers[MdpConstants.CorrelationHeaderKey].ToString();

        var idResult = GetSingleAuthClaim(_httpContextAccessor.HttpContext.User, MdpConstants.MemberClaimNames.ExternalId);

        if (idResult.IsLeft)
        {
            return idResult.Left();
        }

        var subResult = GetSingleAuthClaim(_httpContextAccessor.HttpContext.User, MdpConstants.MemberClaimNames.Sub);

        if (subResult.IsLeft)
        {
            return subResult.Left();
        }

        var tenantResult = GetCurrentTenant();

        if (tenantResult.IsLeft)
        {
            return tenantResult.Left();
        }

        using (_logger.BeginScope("Member registration started for {sub} {externalId}", subResult.Right(), idResult.Right()))
        {
            var email = GetUserClaim(_httpContextAccessor.HttpContext.User, MdpConstants.MemberClaimNames.Email);

            await _authenticationClient.UpdateMember(MdpConstants.AppName, idResult.Right(), subResult.Right());

            var sub = subResult.Right(); // member auth guid
            var externalId = idResult.Right(); // member guid
            await QueueEventAndRegisterRelatedMember(sub, externalId, tenantUrl, email, Guid.Parse(correlationId));

            _logger.LogInformation("Member registration successful for sub {sub} and {externalId}", sub, externalId);
            return true;
        }
    }

    public async Task QueueEventAndRegisterRelatedMember(Guid sub, Guid externalId, string tenantUrl, string email, Guid correlationId)
    {
        try
        {
            var memberResult = await GetMemberAccessData(sub);
            var bgroup = string.Empty;
            var refNo = string.Empty;
            foreach (var item in from item in memberResult
                                 where item.MemberGuid.Equals(externalId)
                                 select item)
            {
                bgroup = item.BusinessGroup;
                refNo = item.ReferenceNumber;
            }

            var tenantContent = await _contentService.FindTenant(tenantUrl, bgroup);

            if (tenantContent?.Elements?.BlockSaRelatedMemberDataRegistration?.Value == false)
            {
                await RegisterRelatedMember(bgroup, refNo);
            }

            if (tenantContent?.Elements?.BlockSingleAuthWelcomeEmail?.Value == false)
            {
                await _publisher.Publish(new EmailNotification
                {
                    ContentAccessKey = tenantUrl,
                    TemplateName = MdpConstants.Templates.SingleAuthRegistrationSuccessEmail,
                    To = email,
                    Bgroup = bgroup,
                    EventType = MdpEvent.SingleAuthRegistration,
                    Refno = refNo,
                    MemberAuthGuid = sub
                }, correlationId);

                _logger.LogInformation("Message published to email queue for sub {sub}", sub);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in {methodName}", nameof(QueueEventAndRegisterRelatedMember));
        }
    }

    public async Task RegisterRelatedMember(string bgroup, string refno)
    {
        var records = await GetMatchingRecord(bgroup, refno);
        List<RegisterRelatedMemberDeloreanClientRequest> recordsToRegister = new();
        foreach (var item in records)
        {
            var isEligible = await CheckEligibility(bgroup, item);
            if (isEligible)
            {
                recordsToRegister.Add(new RegisterRelatedMemberDeloreanClientRequest()
                {
                    ReferenceNumber = item,
                    BusinessGroup = bgroup
                });
            }
            else
            {
                _logger.LogWarning("Skipping related member registration for refno - {refno} as not eligible", item);

            }
        }
        if (recordsToRegister.Any())
        {
            await _authenticationClient.RegisterRelatedMember(bgroup, refno, recordsToRegister);
            _logger.LogInformation("Related member registered successfully - {relatedRefno}", String.Join(',', records));
        }
    }

    async Task<List<string>> GetMatchingRecord(string bgroup, string refno)
    {
        var personalDetails = await _memberClient.GetPersonalDetail(bgroup, refno);
        if (personalDetails.IsNone)
        {
            _logger.LogWarning("No personal details found for member matching registration");
            return new List<string>();
        }
        var matchingRecord = await _memberClient.GetMemberMatchingRecords(bgroup, new GetMemberMatchingClientRequest()
        {
            DateOfBirth = DateTime.Parse(personalDetails.Value().DateOfBirth, CultureInfo.InvariantCulture).ToString("dd-MMM-yy", CultureInfo.InvariantCulture),
            NiNumber = personalDetails.Value().NiNumber,
            surname = personalDetails.Value().Surname
        });

        if (matchingRecord.IsSome)
        {
            var matchingData = matchingRecord.Value()?.MemberList;
            var records = matchingData?.Where(x => x.ReferenceNumber != refno).Select(x => x.ReferenceNumber).ToList();
            if (records?.Count > 0)
            {
                _logger.LogInformation("Related member found - {count}", records.Count);
                return records;
            }
        }

        _logger.LogWarning("No matching records found for registration");
        return new List<string>();
    }

    public async Task<Either<Error, List<LinkedRecordServiceResultData>>> GetLoginDetails()
    {
        var tenantResult = GetCurrentTenant();

        if (tenantResult.IsLeft)
        {
            return tenantResult.Left();
        }

        var claimResult = GetSingleAuthClaim(_httpContextAccessor.HttpContext.User, MdpConstants.MemberClaimNames.Sub);

        if (claimResult.IsLeft)
        {
            return claimResult.Left();
        }

        using (_logger.BeginScope("Login called for sub {sub}", claimResult.Right()))
        {

            var resultLinkedRecord = await GetLinkedRecord(claimResult.Right(), tenantResult.Right());

            var data = await PopulateMemberDetails(resultLinkedRecord);

            return data.Filter(x => x.BusinessGroup.Equals(tenantResult.Right(), StringComparison.CurrentCultureIgnoreCase)).ToList();
        }
    }

    public async Task<List<(string bGroup, string RefNo)>> GetLinkedRecord(Guid sub, string TenantBgroup)
    {
        var resultLinkedRecord = await GetMemberRecords(sub);
        return resultLinkedRecord;
    }

    public async Task<Either<Error, LinkedRecordServiceResult>> GetLinkedRecordTableData()
    {
        LinkedRecordServiceResult result = new LinkedRecordServiceResult();
        var tenantResult = GetCurrentTenant();

        if (tenantResult.IsLeft)
        {
            return tenantResult.Left();
        }

        var claimResult = GetSingleAuthClaim(_httpContextAccessor.HttpContext.User, MdpConstants.MemberClaimNames.Sub);

        if (claimResult.IsLeft)
        {
            return claimResult.Left();
        }

        var resultLinkedRecord = await GetMemberRecords(claimResult.Right());

        result.Members.AddRange(await PopulateMemberDetails(resultLinkedRecord, true));

        if (result.Members.Any(x => !x.BusinessGroup.Equals(tenantResult.Right(), StringComparison.CurrentCultureIgnoreCase)))
        {
            result.hasOutsideRecords = true;
            result.Members = result.Members.Filter(x => x.BusinessGroup.Equals(tenantResult.Right(), StringComparison.CurrentCultureIgnoreCase)).ToList();
        }

        return result;
    }

    public async Task<List<(string bGroup, string RefNo)>> GetMemberRecords(Guid sub)
    {

        var relatedRecords = await ListRelatedRecord(sub);

        return await ListEligibleRecords(relatedRecords);
    }

    public Either<Error, Guid> GetSingleAuthClaim(ClaimsPrincipal principal, string claimName)
    {
        if (!Guid.TryParse(principal.FindFirst(c => c.Type == claimName)?.Value, out Guid sub))
        {
            _logger.LogError("Claim {claimName} not found in request token", claimName);

            return Error.New($"Claim {claimName} not found in request token");
        }
        return sub;
    }

    public string GetUserClaim(ClaimsPrincipal principal, string claimName)
    {
        return principal.FindFirst(c => c.Type == claimName)?.Value;
    }

    public Either<Error, string> GetCurrentTenant()
    {

        if (_httpContextAccessor.HttpContext == null ||
         !_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(MdpConstants.BusinessGroupHeaderName,
             out var tenantBgroup))
        {
            _logger.LogError("Required header {tenantHeader} not found", MdpConstants.BusinessGroupHeaderName);
            return Error.New($"Required header {MdpConstants.BusinessGroupHeaderName} not found");
        }
        return tenantBgroup.ToString();
    }

    public async Task<List<(string bGroup, string RefNo)>> ListRelatedRecord(Guid sub)
    {
        var records = new List<(string bGroup, string RefNo)>();

        var memberAccessRecords = await GetMemberAccessData(sub);
        foreach (var memberAccessRecord in memberAccessRecords)
        {
            TryAdd(records, (memberAccessRecord.BusinessGroup, memberAccessRecord.ReferenceNumber));
            _logger.LogInformation("Member access record found -  {bgroup} {refNo} for {sub}", memberAccessRecord.BusinessGroup, memberAccessRecord.ReferenceNumber, sub);

            var linkedRecordResult = await GetBcUkLinkedRecord(memberAccessRecord.BusinessGroup, memberAccessRecord.ReferenceNumber);
            foreach (var linkedRecord in linkedRecordResult)
            {
                TryAdd(records, (linkedRecord.Bgroup, linkedRecord.ReferenceNumber));
                _logger.LogInformation("Linked record found - {bgroup} {refNo}", linkedRecord.Bgroup, linkedRecord.ReferenceNumber);
            }
        }
        return records;
    }

    void TryAdd(List<(string bGroup, string RefNo)> records, (string bGroup, string RefNo) toAdd)
    {
        if (!records.Any(x => x.RefNo == toAdd.RefNo && x.bGroup.Equals(toAdd.bGroup, StringComparison.InvariantCultureIgnoreCase)))
        {
            records.Add(toAdd);
        }
    }
    public async Task<List<(string bGroup, string RefNo)>> ListEligibleRecords(List<(string bGroup, string RefNo)> records)
    {
        var eligibleRecords = new List<(string bGroup, string RefNo)>();
        foreach (var item in records)
        {
            var isEligible = await CheckEligibility(item.bGroup, item.RefNo);
            if (isEligible)
            {
                eligibleRecords.Add(item);
            }
            else
            {
                _logger.LogWarning("Removed linked record - {bgroup} {refno} as eligible status is {status}", item.bGroup, item.RefNo, isEligible);
            }
        }
        return eligibleRecords;
    }

    public async Task<List<LinkedRecordServiceResultData>> PopulateMemberDetails(List<(string bGroup, string RefNo)> records, bool forTable = false)
    {
        var result = new List<LinkedRecordServiceResultData>();
        foreach (var item in records)
        {
            var data = new LinkedRecordServiceResultData()
            {
                BusinessGroup = item.bGroup,
                ReferenceNumber = item.RefNo,
            };

            var summaryRecord = await _memberClient.GetMemberSummary(item.bGroup, item.RefNo);
            if (summaryRecord.IsSome)
            {
                if (summaryRecord.Value().Status.Equals(MdpConstants.MemberLnStatus, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("Since {refno} status is NL , continue with next record", item.RefNo);
                    continue;
                }

                data.MemberStatus = summaryRecord.Value().StatusTranslation;
                data.SchemeCode = summaryRecord.Value().Scheme;
                data.SchemeDescription = summaryRecord.Value().SchemeTranslation;
                data.RecordNumber = summaryRecord.Value().RecordNumber;
                data.RecordType = summaryRecord.Value().RecordType;
            }

            if (forTable)
            {
                var pensionRecord = await _memberClient.GetPensionDetails(item.bGroup, item.RefNo);
                if (pensionRecord.IsSome)
                {
                    data.DateJoinedCompany = pensionRecord.Value().DateJoinedCompany;
                    data.DateJoinedScheme = pensionRecord.Value().DateJoinedScheme;
                    data.DateLeft = pensionRecord.Value().DateCOEnded;
                }
            }

            result.Add(data);
        }
        return result;
    }


    public async Task<Either<Error, BgroupRefnoData>> CheckMemberAccess(Guid sub, string tenantBgroup)
    {
        var headers = GetBgroupRefnoHeaderValue();
        if (headers.IsLeft)
        {
            return headers.Left();
        }

        var bgroupRefnoHeader = headers.Right();
        if (!bgroupRefnoHeader.bgroup.Equals(tenantBgroup, StringComparison.CurrentCultureIgnoreCase))
        {
            _logger.LogError("Bgroup header {bgroupHeader} and tenant bgroup header {tenantBgroup} don't match", bgroupRefnoHeader.bgroup, tenantBgroup);
            return Error.New($"Bgroup header {bgroupRefnoHeader.bgroup} and tenant bgroup header {tenantBgroup} don't match");
        }

        var memberAccessRecords = await GetMemberAccessData(sub);
        var found = GetMemberAccessForBusinessGroup(memberAccessRecords, bgroupRefnoHeader, tenantBgroup);
        if (found)
        {
            _logger.LogInformation("Member access record matching header values found");
            return new BgroupRefnoData(bgroupRefnoHeader.bgroup, bgroupRefnoHeader.refNo, bgroupRefnoHeader.bgroup, bgroupRefnoHeader.refNo);
        }

        _logger.LogInformation("Matching member access record not found in current tenant,checking linked record");
        foreach (var item in memberAccessRecords)
        {
            var linkedRecordResult = await GetBcUkLinkedRecord(item.BusinessGroup, item.ReferenceNumber);
            foreach (var linkedRecord in linkedRecordResult)
            {
                if (bgroupRefnoHeader.bgroup.Equals(linkedRecord.Bgroup, StringComparison.InvariantCultureIgnoreCase) &&
                   bgroupRefnoHeader.refNo.Equals(linkedRecord.ReferenceNumber, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogInformation("Linked record found which has single auth access with refno {refno}", linkedRecord.ReferenceNumber);
                    return new BgroupRefnoData(bgroupRefnoHeader.bgroup, bgroupRefnoHeader.refNo, item.BusinessGroup, item.ReferenceNumber);
                }
            }
        }

        _logger.LogWarning("Member nor its link records have access");
        return Error.New("Member nor its link records have access");
    }

    async Task<List<LinkedMemberClientResponse>> GetBcUkLinkedRecord(string bgroup, string refno)
    {
        var linkedRecordResult = await _memberClient.GetBcUkLinkedRecord(bgroup, refno);

        if (linkedRecordResult.IsNone)
        {
            return new List<LinkedMemberClientResponse>();
        }
        return linkedRecordResult.Value().LinkedRecords;
    }

    async Task<List<GetMemberDataClient>> GetMemberAccessData(Guid sub)
    {
        var memberResult = await _authenticationClient.GetMemberAccess(MdpConstants.AppName, sub);
        if (memberResult.IsSome)
        {
            var activeRecords = memberResult.Value()?.Members?.FindAll(x => x.Status.Equals(MdpConstants.MemberActive, StringComparison.OrdinalIgnoreCase));
            if (activeRecords?.Any() == true)
            {
                return activeRecords;
            }
        }

        _logger.LogWarning("No active memberAccess record found for {sub}", sub);
        return new List<GetMemberDataClient>();
    }

    async Task<bool> CheckEligibility(string bgroup, string refno)
    {
        var eligibleResponse = await _authenticationClient.CheckEligibility(bgroup, refno, MdpConstants.AppName);
        return eligibleResponse.Match(
              response => response.Eligible,
              () => false);
    }

    Either<Error, (string bgroup, string refNo)> GetBgroupRefnoHeaderValue()
    {
        var requiredHeaders = new List<string>
        {
            MdpConstants.BusinessGroupHeaderName,
            MdpConstants.ReferenceNumberHeaderName,
        };

        foreach (var header in requiredHeaders)
        {
            if (_httpContextAccessor.HttpContext == null ||
                !_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(header, out var headerValues) ||
                headerValues.Count != 1)
            {
                _logger.LogError("Header {header} not found in request", header);
                return Error.New($"Header {header} not found in request");
            }
        }

        return (_httpContextAccessor.HttpContext.Request.Headers[MdpConstants.BusinessGroupHeaderName].ToString(),
                _httpContextAccessor.HttpContext.Request.Headers[MdpConstants.ReferenceNumberHeaderName].ToString());
    }

    static bool GetMemberAccessForBusinessGroup(List<GetMemberDataClient> memberAccessList, (string bgroup, string refNo) header, string tenantBgroup)
    {
        return memberAccessList?.Any(x => !string.IsNullOrEmpty(x.BusinessGroup) &&
                           x.BusinessGroup.Equals(header.bgroup, StringComparison.OrdinalIgnoreCase)
                           && !string.IsNullOrEmpty(x.ReferenceNumber) &&
                           x.ReferenceNumber.Equals(header.refNo, StringComparison.OrdinalIgnoreCase)
                           && x.BusinessGroup.Equals(tenantBgroup, StringComparison.CurrentCultureIgnoreCase)) == true;
    }

    public bool IgnoreClaimTransformationCheck()
    {
        return MdpConstants.IgnoreClaimTransformation.Exists(x => x.Equals(_httpContextAccessor.HttpContext.Request.Path,
                                                              StringComparison.CurrentCultureIgnoreCase));
    }

    public async Task<string> GetOutboundToken(int? recordNumber, bool hasMultipleRecords)
    {
        var bgroup = GetUserClaim(_httpContextAccessor.HttpContext.User, MdpConstants.MemberClaimNames.BusinessGroup);
        var refNo = GetUserClaim(_httpContextAccessor.HttpContext.User, MdpConstants.MemberClaimNames.ReferenceNumber);

        if (recordNumber == 1)
        {
            return await GenerateOutboundToken(bgroup, refNo, bgroup, refNo, hasMultipleRecords);
        }
        else
        {
            var resultEpa = await _epaClient.GetEpaUser(bgroup, refNo);
            if (resultEpa.IsSome)
            {
                return await GenerateOutboundToken(bgroup, refNo, bgroup, refNo, hasMultipleRecords);
            }
        }

        var mainBgroup = GetUserClaim(_httpContextAccessor.HttpContext.User, MdpConstants.MemberClaimNames.MainBusinessGroup);
        var mainRefNo = GetUserClaim(_httpContextAccessor.HttpContext.User, MdpConstants.MemberClaimNames.MainReferenceNumber);
        return await GenerateOutboundToken(bgroup, refNo, mainBgroup, mainRefNo, hasMultipleRecords);
    }

    async Task<string> GenerateOutboundToken(string bgroup, string refno, string clientId, string uid, bool hasMultipleRecords)
    {
        var tokenResponse = await _authenticationClient.GenerateToken(new OutboundSsoGenerateTokenClientRequest()
        {
            Claims = new OutboundSsoGenerateTokenClientRequest.OutboundClaims()
            {
                Bgroup = bgroup,
                Refno = refno,
                ClientId = clientId,
                Uid = uid,
                Application = MdpConstants.AppName.ToLower(),
                HasMultipleRecords = hasMultipleRecords.ToString().ToLower()
            },
            Expiry = 3600
        });
        return tokenResponse.AccessToken;
    }

    public bool IsAnonRequest()
    {
        var endpoint = _httpContextAccessor.HttpContext.Features.Get<IEndpointFeature>()?.Endpoint;
        return endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null;
    }
}
