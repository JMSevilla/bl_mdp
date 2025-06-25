using System;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Infrastructure.MemberService;
using WTW.Web;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Infrastructure.Investment.AnnuityBroker;

public class InvestmentQuoteService : IInvestmentQuoteService
{
    private readonly IMemberServiceClient _memberServiceClient;

    public InvestmentQuoteService(IMemberServiceClient memberServiceClient)
    {
        _memberServiceClient = memberServiceClient;
    }

    public async Task<Either<Error, InvestmentQuoteRequest>> CreateAnnuityQuoteRequest(string businessGroup, string referenceNumber, RetirementV2 retirementV2)
    {
        var memberSummaryOption = await _memberServiceClient.GetMemberSummary(businessGroup, referenceNumber);
        if (memberSummaryOption.IsNone)
            return Error.New($"Member summary not found for {businessGroup} with reference number {referenceNumber}");

        var memberPersonalDetailsOption = await _memberServiceClient.GetPersonalDetail(businessGroup, referenceNumber);
        if (memberPersonalDetailsOption.IsNone)
            return Error.New($"Member personal details not found for {businessGroup} with reference number {referenceNumber}");

        var memberPensionDetailsOption = await _memberServiceClient.GetPensionDetails(businessGroup, referenceNumber);
        if (memberPersonalDetailsOption.IsNone)
            return Error.New($"Member pension details not found for {businessGroup} with reference number {referenceNumber}");

        var memberContactDetailsOption = await _memberServiceClient.GetContactDetails(businessGroup, referenceNumber);
        if (memberContactDetailsOption.IsNone)
            return Error.New($"Member contact details not found for {businessGroup} with reference number {referenceNumber}");

        var memberSummary = memberSummaryOption.Value();
        var memberPersonalDetails = memberPersonalDetailsOption.Value();
        var memberPensionDetails = memberPensionDetailsOption.Value();
        var memberContactDetails = memberContactDetailsOption.Value();

        var quoteMemberSummary = CreateMemberSummary(memberSummary, memberPersonalDetails, memberPensionDetails, memberContactDetails);
        var investmentQuoteDetails = CreateQuoteDetails(retirementV2);

        var request = new InvestmentQuoteRequest
        {
            EventType = MdpConstants.AnnuityProvider.EventType,
            AutomatedInd = MdpConstants.AnnuityProvider.AutomatedInd,
            FullDataSet = MdpConstants.AnnuityProvider.FullDataSet,
            MemberRequestedInd = MdpConstants.AnnuityProvider.MemberRequestedInd,
            Quote = investmentQuoteDetails,
            Member = quoteMemberSummary,
        };

        return request;
    }

    private InvestmentQuoteMemberSummary CreateMemberSummary(
        GetMemberSummaryClientResponse memberSummary,
        GetMemberPersonalDetailClientResponse memberPersonalDetails,
        GetPensionDetailsClientResponse memberPensionDetails,
        MemberContactDetailsClientResponse memberContactDetails)
    {
        return new InvestmentQuoteMemberSummary
        {
            NiNumber = memberPersonalDetails.NiNumber,
            PayrollNo = memberPensionDetails.PayrollNo,
            Title = memberPersonalDetails.Title,
            Surname = memberPersonalDetails.Surname,
            Forenames = memberPersonalDetails.Forenames,
            DateOfBirth = memberPersonalDetails.DateOfBirth,
            Sex = memberPersonalDetails.Sex,
            Address = new InvestmentQuoteMemberAddress
            {
                Line1 = memberContactDetails.Address.Line1,
                Line2 = memberContactDetails.Address.Line2,
                Line3 = memberContactDetails.Address.Line3,
                Line4 = memberContactDetails.Address.Line4,
                Line5 = memberContactDetails.Address.Line5,
                Country = memberContactDetails.Address.Country,
                Postcode = memberContactDetails.Address.PostCode
            },
            Telephone = memberContactDetails.Telephone,
            Email = memberContactDetails.Email,

            MaritalStatus = memberPersonalDetails.MaritalStatus,
            SchemeName = memberSummary.SchemeTranslation,
            SchemeType = memberSummary.SchemeType,
            NonStandardCommsType = memberContactDetails.NonStandardCommsType,
            MemberStatus = memberSummary.Status,
            SchemeCode = memberSummary.Scheme,
            BusinessGroupTitle = memberSummary.BgroupTranslation,
        };
    }

    private InvestmentQuoteDetails CreateQuoteDetails(RetirementV2 retirementV2)
    {
        return new InvestmentQuoteDetails
        {
            QuoteType = MdpConstants.AnnuityProvider.QuoteType,
            PclsAmount = Convert.ToInt32(retirementV2.MaximumPermittedTotalLumpSum),
            FundValue = retirementV2.TotalFundValue.ToString(),
            ResidualFundValue = retirementV2.ResidualFundValue,
            ValueDate = DateTime.Now.ToString("yyyy-MM-dd"),
            CalculationCode = MdpConstants.AnnuityProvider.CalculationCode,
        };
    }
}