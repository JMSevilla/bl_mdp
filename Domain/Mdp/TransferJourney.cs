using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt.Common;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Common.Journeys;

namespace WTW.MdpService.Domain.Mdp;

public class TransferJourney : Journey
{
    private readonly List<TransferJourneyContact> _contacts = new();

    protected TransferJourney() { }

    private TransferJourney(string businessGroup, string referenceNumber, DateTimeOffset utcNow, string currentPageKey, string nextPageKey, int transferImageId)
        : base(businessGroup, referenceNumber, utcNow, currentPageKey, nextPageKey)
    {
        TransferImageId = transferImageId;
        TransferVersion = "transfer3";
    }

    public int TransferImageId { get; }
    public int? TransferSummaryImageId { get; private set; }
    public string CalculationType { get; private set; }
    public Guid? GbgId { get; private set; }
    public string CaseNumber { get; private set; }
    public string NameOfPlan { get; private set; }
    public string TypeOfPayment { get; private set; }
    public DateTime? DateOfPayment { get; private set; }
    public DateTimeOffset? PensionWiseDate { get; private set; }
    public DateTimeOffset? FinancialAdviseDate { get; private set; }
    public string TransferVersion { get; }
    public virtual IReadOnlyList<TransferJourneyContact> Contacts => _contacts;

    public static TransferJourney CreateEpa(string businessGroup, string referenceNumber, string caseNumber, DateTimeOffset submitDate, DateTimeOffset utcNow)
    {
        var journey = new TransferJourney(businessGroup, referenceNumber, utcNow, "OutsideTa1", "OutsideTa2", default);
        journey.Submit(caseNumber, submitDate);

        return journey;
    }

    public static TransferJourney Create(string businessGroup, string referenceNumber, DateTimeOffset utcNow, string currentPageKey, string nextPageKey, int transferImageId)
    {
        return new(businessGroup, referenceNumber, utcNow, currentPageKey, nextPageKey, transferImageId);
    }

    public static TransferJourney Create(string businessGroup, string referenceNumber, DateTimeOffset utcNow, int transferImageId)
    {
        return new(businessGroup, referenceNumber, utcNow, string.Empty, string.Empty, transferImageId);
    }

    public void SetCalculationType(string type)
    {
        CalculationType = type;
    }

    public void SubmitContact(TransferJourneyContact transferJourneyContact)
    {
        var contact = Contacts.SingleOrDefault(c => c.Type == transferJourneyContact.Type);

        if (contact == null)
        {
            _contacts.Add(transferJourneyContact);
            return;
        }

        transferJourneyContact.SubmitAddress(contact.Address.Clone());
        _contacts.Add(transferJourneyContact);
        _contacts.Remove(contact);
    }

    public Error? SubmitContactAddress(Address address, string type)
    {
        var contact = Contacts.SingleOrDefault(c => c.Type == type);
        if (contact == null)
            return Error.New("Contact details must be submitted, before submitting its address details.");

        contact.SubmitAddress(address.Clone());

        return null;
    }

    public void RemoveAllContacts()
    {
        var contacts = Contacts.ToArray();
        _contacts.RemoveAll(x => contacts.Contains(x));
    }

    public void SaveGbgId(Guid id)
    {
        GbgId = id;
    }

    public void SaveTransferSummaryImageId(int? id)
    {
        TransferSummaryImageId = id;
    }

    public void RemoveStepsAndUpdateLastStepCurrentPageKeyToHub()
    {
        var activeJourneyBranch = JourneyBranches.Single(b => b.IsActive);
        var lastStep = activeJourneyBranch.GetLastStep();
        lastStep.UpdateCurrentPageKey("hub");
        activeJourneyBranch.RemoveStepsExcept(lastStep);
    }

    public void Submit(string caseNumber, DateTimeOffset utcNow)
    {
        CaseNumber = caseNumber;
        SubmissionDate = utcNow;
    }

    public bool IsGbgStepOlderThan30Days(DateTimeOffset utcNow)
    {
        return ActiveBranch().JourneySteps.SingleOrDefault(s => s.CurrentPageKey == "t2_submit_upload")?.SubmitDate.AddDays(30) < utcNow;
    }

    public Error? SaveFlexibleBenefits(string nameOfPlan, string typeOfPayment, DateTime? dateOfPayment, DateTimeOffset now)
    {
        if (nameOfPlan != null && nameOfPlan.Length > 50)
            return Error.New("\'Name of Plan\' must be less or equal 50 characters length.");

        if (typeOfPayment != null && typeOfPayment.Length > 50)
            return Error.New("\'Type of Payment\' must be less or equal 50 characters length.");

        if (dateOfPayment.HasValue && dateOfPayment.Value.Date > now.Date)
            return Error.New("\'Date of Payment\' must be less or equal to today's date");

        NameOfPlan = nameOfPlan;
        TypeOfPayment = typeOfPayment;
        DateOfPayment = dateOfPayment;

        return null;
    }

    public Error? SetPensionWiseDate(DateTimeOffset? pensionWiseDate)
    {
        if (pensionWiseDate is not null && !DateIsLessThanOrEqualToTodayValidation(pensionWiseDate.Value))
            return Error.New("Pension wise date should be less than or equal to today.");

        PensionWiseDate = pensionWiseDate;
        return null;
    }

    public Error? SetFinancialAdviseDate(DateTimeOffset? financialAdviseDate)
    {
        if (financialAdviseDate is not null && !DateIsLessThanOrEqualToTodayValidation(financialAdviseDate.Value))
            return Error.New("Financial advise date should be less than or equal to today.");

        FinancialAdviseDate = financialAdviseDate;
        return null;
    }

    public void ClearPensionWiseDate()
    {
        PensionWiseDate = null;
    }

    public void ClearFlexibleBenefitsData()
    {
        NameOfPlan = null;
        TypeOfPayment = null;
        DateOfPayment = null;
    }

    public void ClearFinancialAdviseDate()
    {
        FinancialAdviseDate = null;
    }

    private readonly Func<DateTimeOffset, bool> DateIsLessThanOrEqualToTodayValidation = date =>
    {
        return date.Date <= DateTimeOffset.UtcNow.Date;
    };

    private JourneyBranch ActiveBranch()
    {
        return JourneyBranches.Single(x => x.IsActive);
    }
}