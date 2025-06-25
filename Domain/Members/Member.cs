using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members.Beneficiaries;
using WTW.MdpService.Domain.Members.Dependants;
using WTW.MdpService.Infrastructure;
using WTW.MdpService.Retirement;
using WTW.Web.LanguageExt;
using static WTW.MdpService.Retirement.RetirementConstants;
using static WTW.Web.MdpConstants;

namespace WTW.MdpService.Domain.Members;

public class Member
{
    private readonly List<BankAccount> _bankAccounts = new();
    private readonly List<ContactReference> _contactReferences = new();
    private readonly List<ContactValidation> _contactValidations = new();
    private readonly List<EpaEmail> _epaEmails = new();
    private readonly List<NotificationSetting> _notificationSettings = new();
    private readonly List<LinkedMember> _linkedMembers = new();
    private readonly List<PaperRetirementApplication> _paperRetirementApplications = new();
    private readonly List<Beneficiary> _beneficiaries = new();
    private readonly List<Dependant> _dependants = new();

    public Member(PersonalDetails personalDetails, MemberStatus status, string schemeCode, string businessGroup,
        string referenceNumber, string category, CategoryDetail categoryDetail, Scheme scheme, string insuranceNumber,
        string membershipNumber, string payrollNumber, DateTimeOffset dateJoinedScheme, DateTimeOffset? dateLeftScheme,
        string employerCode, string locationCode, EmailView emailView, string statusCode, string complaintInticator,
        DateTimeOffset? datePensionableServiceStarted, DateTimeOffset? dateJoinedCompany,
        List<PaperRetirementApplication> paperRetirementApplications, List<LinkedMember> linkedMembers,
        List<Beneficiary> beneficiaries, List<BankAccount> bankAccounts, List<ContactReference> contactReferences, string recordsIndicator)
    {
        Status = status;
        SchemeCode = schemeCode;
        BusinessGroup = businessGroup;
        ReferenceNumber = referenceNumber;
        Category = category;
        CategoryDetail = categoryDetail;
        Scheme = scheme;
        PersonalDetails = personalDetails;
        InsuranceNumber = insuranceNumber;
        MembershipNumber = membershipNumber;
        PayrollNumber = payrollNumber;
        DateJoinedScheme = dateJoinedScheme;
        DateLeftScheme = dateLeftScheme;
        EmployerCode = employerCode;
        LocationCode = locationCode;
        EmailView = emailView;
        StatusCode = statusCode;
        ComplaintInticator = complaintInticator;
        DatePensionableServiceStarted = datePensionableServiceStarted;
        DateJoinedCompany = dateJoinedCompany;
        _paperRetirementApplications = paperRetirementApplications;
        _linkedMembers = linkedMembers;
        _beneficiaries = beneficiaries;
        _bankAccounts = bankAccounts;
        _contactReferences = contactReferences;
        RecordsIndicator = recordsIndicator;
    }

    protected Member() { }

    public string ReferenceNumber { get; }
    public string BusinessGroup { get; }
    public string SchemeCode { get; }
    public string Category { get; }
    public string RecordsIndicator { get; private set; }
    public MemberStatus Status { get; }
    public string StatusCode { get; }
    public string ComplaintInticator { get; }
    public virtual CategoryDetail CategoryDetail { get; }
    public virtual Scheme Scheme { get; }
    public string InsuranceNumber { get; }
    public string MembershipNumber { get; }
    public string PayrollNumber { get; }
    public string EmployerCode { get; }
    public string LocationCode { get; }
    public string MaritalStatus { get; }
    public DateTimeOffset DateJoinedScheme { get; }
    public DateTimeOffset? DateLeftScheme { get; }
    public DateTimeOffset? DatePensionableServiceStarted { get; }
    public DateTimeOffset? DateJoinedCompany { get; }
    public virtual PersonalDetails PersonalDetails { get; }
    public virtual EmailView EmailView { get; }
    public virtual ContactCountry ContactCountry { get; private set; }
    public virtual IReadOnlyList<BankAccount> BankAccounts => _bankAccounts.ToList();
    public virtual IReadOnlyList<ContactReference> ContactReferences => _contactReferences.ToList();
    public virtual IReadOnlyList<EpaEmail> EpaEmails => _epaEmails.ToList();
    public virtual IReadOnlyList<NotificationSetting> NotificationSettings => _notificationSettings.ToList();
    public virtual IReadOnlyList<LinkedMember> LinkedMembers => _linkedMembers.ToList();
    public virtual IReadOnlyList<PaperRetirementApplication> PaperRetirementApplications => _paperRetirementApplications.ToList();
    public virtual IReadOnlyList<Beneficiary> Beneficiaries => _beneficiaries.ToList();
    public virtual IReadOnlyList<Dependant> Dependants => _dependants.ToList();
    public virtual IReadOnlyList<ContactValidation> ContactValidations => _contactValidations.ToList();

    public (int Years, int Months) CalculateTermToRetirement(DateTimeOffset now, DateTimeOffset retirementDate)
    {
        retirementDate = retirementDate > now ? retirementDate : now;

        var allMonths = (retirementDate.Year - now.Year) * 12 + retirementDate.Month - now.Month;
        var years = allMonths / 12;
        var yearsToRetirement = years > 0 ? years : 0;
        var months = allMonths - years * 12;
        var monthsToRetirement = months > 0 ? months : 0;

        return (yearsToRetirement, monthsToRetirement);
    }

    public MemberLifeStage GetLifeStage(DateTimeOffset now, int preRetirementAgePeriod,
        int newlyRetiredRange, RetirementDatesAges retirementDatesAges)
    {
        var memberAge = GetAge(now);
        var exactMemberAge = GetExactAge(now);
        var earliestRetirementAge = retirementDatesAges.EarliestRetirement();
        var normalRetirementAge = (int)retirementDatesAges.NormalRetirementAge;
        var earliestRetirementDate = retirementDatesAges.EarliestRetirementDate;
        var latestRetirementAge = retirementDatesAges.LatestRetirementAge;

        if (IsNotEligibleToRetire(preRetirementAgePeriod, memberAge, earliestRetirementAge))
            return MemberLifeStage.NotEligibleToRetire;

        if (IsPreRetiree(preRetirementAgePeriod, memberAge, exactMemberAge, earliestRetirementAge))
            return MemberLifeStage.PreRetiree;

        if (IsEligibleToApplyForRetirement(earliestRetirementAge, exactMemberAge))
            return MemberLifeStage.EligibleToApplyForRetirement;

        if (IsEligibleToRetire(memberAge, earliestRetirementAge, normalRetirementAge))
            return MemberLifeStage.EligibleToRetire;

        if (IsLateRetirement(memberAge, exactMemberAge, normalRetirementAge, latestRetirementAge))
            return MemberLifeStage.LateRetirement;

        if (IsCloseToLatestRetirementAge(memberAge, exactMemberAge, latestRetirementAge))
            return MemberLifeStage.CloseToLatestRetirementAge;

        if (IsOverLatestRetirementAge(memberAge, exactMemberAge, latestRetirementAge))
            return MemberLifeStage.OverLatestRetirementAge;

        if (IsNewlyRetired(now, newlyRetiredRange, earliestRetirementDate))
            return MemberLifeStage.NewlyRetired;

        return MemberLifeStage.EstablishedRetiree;
    }

    public Option<BankAccount> EffectiveBankAccount()
    {
        return BankAccounts.OrderByDescending(x => x.EffectiveDate).FirstOrDefault();
    }

    public Option<Address> Address()
    {
        return CurrentContactReference()?.Contact?.Address;
    }

    public Option<string> Email()
    {
        return EmailView?.Email.Address;
    }

    public Option<string> MobilePhoneNumber()
    {
        return CurrentContactReference()?.Contact?.MobilePhone.Number();
    }

    public Option<string> MobilePhoneCode()
    {
        return CurrentContactReference()?.Contact?.MobilePhone.Code();
    }

    public Option<string> FullMobilePhoneNumber()
    {
        return CurrentContactReference()?.Contact?.MobilePhone.FullNumber;
    }

    public bool HasMobilePhone(Phone phone)
    {
        return CurrentContactReference()?.Contact?.MobilePhone == phone;
    }

    public Member SaveMobilePhone(Phone phone, long authorizationNumber, long addressNumber, string tableShort, DateTimeOffset utcNow)
    {
        if (HasMobilePhone(phone))
            throw new InvalidOperationException(
                $"{nameof(HasMobilePhone)} method must be called to ensure the same mobile phone is not present.");

        var currentContactReference = CurrentContactReference();
        currentContactReference?.Close(utcNow);

        var contact = new Contact(
            currentContactReference?.Contact?.Address.Clone() ?? Common.Address.Empty(),
            currentContactReference?.Contact?.Email.Clone() ?? Domain.Email.Empty(),
            phone,
            currentContactReference?.Contact?.Data.Clone() ?? Domain.Members.Contact.DataToCopy.Empty(),
            BusinessGroup,
            addressNumber);

        AddNewContactReference(contact, authorizationNumber, GetContactReferenceNextSequenceNumber(currentContactReference), utcNow);
        AddRecordsIndicator(tableShort);
        return this;
    }

    public Either<Error, Unit> UpdateMobilePhoneValidationFor2FaBusinessGroup(string userId, long addressNumber, string token, string mf2FaStatus, DateTimeOffset utcNow, string country)
    {
        if (mf2FaStatus != "Y")
            return Unit.Default;

        var phoneValidation = ContactValidations.FirstOrDefault(x => x.ContactType == MemberContactType.MobilePhoneNumber1);
        if (phoneValidation != null)
            _contactValidations.Remove(phoneValidation);

        _contactValidations.Add(new ContactValidation(userId, MemberContactType.MobilePhoneNumber1, utcNow, addressNumber, token));
        var ContactCountryOrError = ContactCountry.Create(addressNumber, country);
        if (ContactCountryOrError.IsLeft)
        {
            return ContactCountryOrError.Left();
        }
        ContactCountry = ContactCountryOrError.Right();
        return Unit.Default;
    }

    public int AgeAtRetirement(int normalRetirementAge, DateTimeOffset utcNow)
    {
        int? floorRoundedAge = GetExactAge(utcNow).HasValue ? Convert.ToInt32(Math.Floor(GetExactAge(utcNow).Value)) : null;

        return (BusinessGroup.Equals("BCL", StringComparison.InvariantCultureIgnoreCase) && floorRoundedAge.HasValue && floorRoundedAge < normalRetirementAge) ||
                (floorRoundedAge.HasValue && floorRoundedAge > normalRetirementAge)
            ? floorRoundedAge.Value
            : normalRetirementAge;
    }

    public Option<PaperRetirementApplication> TransferPaperCase()
    {
        return PaperRetirementApplications.FirstOrDefault(x => x.IsTransferRetirementApplicationSubmitted());
    }

    public bool HasAddress(Address address)
    {
        var result = Address();
        return result.IsSome && result.Value() == address;
    }

    public void SaveAddress(Address address, long authorizationNumber, long addressNumber, string tableShort, DateTimeOffset utcNow)
    {
        if (HasAddress(address))
            throw new InvalidOperationException(
                $"{nameof(HasAddress)} method must be called to ensure same address is not present.");

        var currentContactReference = CurrentContactReference();
        currentContactReference?.Close(utcNow);

        var contact = new Contact(
            address,
            currentContactReference?.Contact?.Email.Clone() ?? Domain.Email.Empty(),
            currentContactReference?.Contact?.MobilePhone.Clone() ?? Phone.Empty(),
            currentContactReference?.Contact?.Data.Clone() ?? Domain.Members.Contact.DataToCopy.Empty(),
            BusinessGroup,
            addressNumber);

        AddNewContactReference(contact, authorizationNumber, GetContactReferenceNextSequenceNumber(currentContactReference), utcNow);
        AddRecordsIndicator(tableShort);
    }

    public bool HasEmail(Email email)
    {
        var result = Email();
        return result.IsSome && result.Value() == email;
    }

    public Member SaveEmail(Email email, long authorizationNumber, long addressNumber, string tableShort, DateTimeOffset utcNow)
    {
        if (HasEmail(email))
            throw new InvalidOperationException(
                $"{nameof(HasEmail)} method must be called to ensure same email address is not present.");

        var currentContactReference = CurrentContactReference();
        currentContactReference?.Close(utcNow);

        var contact = new Contact(
            currentContactReference?.Contact?.Address.Clone() ?? Common.Address.Empty(),
            email,
            currentContactReference?.Contact?.MobilePhone.Clone() ?? Phone.Empty(),
            currentContactReference?.Contact?.Data.Clone() ?? Domain.Members.Contact.DataToCopy.Empty(),
            BusinessGroup,
            addressNumber);

        AddNewContactReference(contact, authorizationNumber, GetContactReferenceNextSequenceNumber(currentContactReference), utcNow);
        _epaEmails.Add(new EpaEmail(email, utcNow, EpaEmails.Any() ? EpaEmails.Max(x => x.SequenceNumber) + 1 : 1));
        AddRecordsIndicator(tableShort);
        return this;
    }

    public void UpdateEmailValidationFor2FaBusinessGroup(string userId, long addressNumber, string token, string mf2FaStatus, DateTimeOffset utcNow)
    {
        if (mf2FaStatus != "Y")
            return;

        var emailValidation = ContactValidations.FirstOrDefault(x => x.ContactType == MemberContactType.EmailAddress);
        if (emailValidation != null)
            _contactValidations.Remove(emailValidation);

        _contactValidations.Add(new ContactValidation(userId, MemberContactType.EmailAddress, utcNow, addressNumber, token));
    }

    public IEnumerable<int> GetAgeLines(DateTimeOffset now, int earliestRetirementAge, int normalRetirementAge)
    {
        var line = GetFirstAgeLine(now, earliestRetirementAge, normalRetirementAge);
        var lines = new List<int> { line };

        for (int i = 0; i < 4; i++)
        {
            if (line >= 75)
                break;

            lines.Add(++line);
        }

        return lines;
    }

    public bool HasDateOfBirth()
    {
        return PersonalDetails.DateOfBirth.HasValue;
    }

    public bool CanCalculateRetirement()
    {
        return Status == MemberStatus.Active || Status == MemberStatus.Deferred;
    }

    public Either<Error, bool> TrySubmitUkBankAccount(string accountName, string accountNumber, DateTimeOffset utcNow, Bank bank)
    {
        var bankAccountOrError = BankAccount.CreateUkAccount(GetNewAccountSequenceNumber(), accountName, accountNumber, utcNow, bank);
        if (bankAccountOrError.IsLeft)
            return bankAccountOrError.Left();

        _bankAccounts.Add(bankAccountOrError.Right());
        return true;
    }

    public Either<Error, bool> TrySubmitIbanBankAccount(string accountName, string iban, DateTimeOffset utcNow, Bank bank)
    {
        var bankAccountOrError = BankAccount.CreateIbanAccount(GetNewAccountSequenceNumber(), accountName, iban, utcNow, bank);
        if (bankAccountOrError.IsLeft)
            return bankAccountOrError.Left();

        _bankAccounts.Add(bankAccountOrError.Right());
        return true;
    }

    public TransferApplicationStatus GetTransferApplicationStatus(Option<TransferCalculation> transferCalculation)
    {
        return transferCalculation.Match(x => x.TransferStatus(), () => TransferApplicationStatus.Undefined);
    }

    public bool IsTransferStatusStatedTaOrSubmitStarted(TransferCalculation calc)
    {
        return GetTransferApplicationStatus(calc) == TransferApplicationStatus.StartedTA || GetTransferApplicationStatus(calc) == TransferApplicationStatus.SubmitStarted;
    }

    public RetirementApplicationStatus GetRetirementApplicationStatus(
        DateTimeOffset now,
        int preRetirementAgePeriod,
        int newlyRetiredRange,
        bool hasRetirementJourneyStarted,
        bool isRetirementJourneySubmitted,
        bool hasRetirementJourneyExpired,
        DateTimeOffset? selectedRetirementDate,
        RetirementDatesAges retirementDatesAges)
    {
        var lifeStage = GetLifeStage(now, preRetirementAgePeriod, newlyRetiredRange, retirementDatesAges);
        var retirementDate = selectedRetirementDate ?? retirementDatesAges.EarliestRetirementDate;

        if (PaperRetirementApplications.Any(x => x.IsPaperRetirementApplicationSubmitted()))
            return RetirementApplicationStatus.RetirementCase;

        if (lifeStage == MemberLifeStage.Undefined)
            return RetirementApplicationStatus.NotEligibleToStart;

        if (hasRetirementJourneyStarted && !isRetirementJourneySubmitted && !hasRetirementJourneyExpired)
            return RetirementApplicationStatus.StartedRA;

        if (hasRetirementJourneyStarted && isRetirementJourneySubmitted)
            return RetirementApplicationStatus.SubmittedRA;

        if (retirementDatesAges.NormalMinimumPensionDate.HasValue && retirementDatesAges.EarliestRetirementDate <= selectedRetirementDate &&
            retirementDatesAges.NormalMinimumPensionDate.Value > selectedRetirementDate)
            return RetirementApplicationStatus.MinimumRetirementDateOutOfRange;

        // For NatWest (RBS), use a fixed 90 days;
        DateTimeOffset eligibleToRetireTo = BusinessGroup == NatwestBgroup
            ? now.AddDays(RetirementApplicationPeriodInDaysRBS)
            : now.AddMonths(RetirementApplicationPeriodInMonths);

        if (Status == MemberStatus.Deferred
            && (lifeStage == MemberLifeStage.EligibleToApplyForRetirement ||
            lifeStage == MemberLifeStage.EligibleToRetire || lifeStage == MemberLifeStage.LateRetirement)
            && now.Date <= retirementDate.Date && retirementDate.Date <= eligibleToRetireTo.Date)
            return RetirementApplicationStatus.EligibleToStart;

        if (Status == MemberStatus.Deferred
            && (lifeStage == MemberLifeStage.EligibleToApplyForRetirement ||
            lifeStage == MemberLifeStage.EligibleToRetire || lifeStage == MemberLifeStage.LateRetirement)
            && retirementDate.Date > eligibleToRetireTo.Date)
            return RetirementApplicationStatus.RetirementDateOutOfRange;

        return RetirementApplicationStatus.NotEligibleToStart;
    }

    public Either<Error, long> UpdateNotificationsSettings(string typeToUpdate, bool email, bool sms, bool post, DateTimeOffset utcNow)
    {
        var sequenceNr = NotificationSettings.Any() ? NotificationSettings.Max(x => x.SequenceNumber) + 1 : 1;
        var currentSetting = NotificationSettings.SingleOrDefault(x => x.EndDate == null && x.Scheme == "M").ToOption();
        var newSetting = currentSetting
            .Match(
            x => NotificationSetting.Create(email, sms, post, sequenceNr, typeToUpdate, utcNow),
            () => NotificationSetting.Create(email, sms, post, sequenceNr, typeToUpdate, utcNow));

        if (newSetting.IsLeft)
            return newSetting.Left();

        _notificationSettings.Add(newSetting.Right());
        currentSetting.IfSome(x => x.Close(utcNow));

        return sequenceNr;
    }

    public (bool Email, bool Sms, bool Post) NotificationsSettings()
    {
        if (!NotificationSettings.Any())
            return (false, false, true);

        return NotificationSettings.Single(x => x.EndDate == null && x.Scheme == "M").NotificationSettings();
    }

    public DateTime LatestRetirementDate(DateTimeOffset? latestRetirementDate, int? latestRetirementAgeInYears, string businessGroup, DateTimeOffset now)
    {
        var normalLatestRetirementDate = PersonalDetails.DateOfBirth.Value.AddYears(latestRetirementAgeInYears ?? RetirementConstants.LatestRetirementAgeInYears).Date;
        var barclaysLatestRetirementDate = now.AddMonths(6).AddDays(-1).Date;
        if (businessGroup == "BCL" && latestRetirementDate.HasValue)
            return barclaysLatestRetirementDate > latestRetirementDate ? latestRetirementDate.Value.Date : barclaysLatestRetirementDate;

        if (latestRetirementDate != null)
            return latestRetirementDate.Value.Date;

        return normalLatestRetirementDate;
    }

    public DateTime DcRetirementDate(DateTimeOffset now)
    {
        var memberAgeYear = GetAge(now);
        var memberAgeMonth = GetAgeMonth(now);
        if (memberAgeYear == RetirementConstants.LatestRetirementAgeInYears - 1 && memberAgeMonth >= 6)
        {
            var maxRetirementDate = PersonalDetails.DateOfBirth.Value.AddYears(RetirementConstants.LatestRetirementAgeInYears).Date;
            return maxRetirementDate;
        }

        return now.Date.AddMonths(RetirementConstants.DcRetirementDateAdditionalPeriodInMonth);
    }


    public string GetAgeAndMonth(DateTimeOffset now)
    {
        var memberAgeYear = GetAge(now);
        var memberAgeMonth = GetAgeMonth(now);
        if (memberAgeYear.HasValue && memberAgeMonth.HasValue)
            return memberAgeYear + "Y" + memberAgeMonth + "M";

        return null;
    }

    public Error? UpdateBeneficiaries(List<(int? SequenceNumber, BeneficiaryDetails Details, BeneficiaryAddress Address)> beneficiaries, DateTimeOffset utcNow)
    {
        var pensionBeneficiariesCount = beneficiaries.Count(x => x.Details.PensionPercentage == 100);
        if (pensionBeneficiariesCount > 1)
            return Error.New("Only one beneficiary is eligible for pension.");

        if (beneficiaries.Any() && beneficiaries.Sum(x => x.Details.LumpSumPercentage) != 100)
            return Error.New("Sum of beneficiaries lump sum percentages must be 100.");

        var beneficiariesIdsToBeUpdated = beneficiaries.Where(x => x.SequenceNumber.HasValue).Select(x => x.SequenceNumber.Value);
        var nonExistingIds = beneficiariesIdsToBeUpdated.Except(Beneficiaries.Select(x => x.SequenceNumber));

        if (nonExistingIds.Any())
            return Error.New($"Beneficiaries to be updated do not exist. Ids: {(string.Join(", ", nonExistingIds)).Trim()}.");

        Beneficiaries.Where(x => x.RevokeDate == null).ToList().ForEach(b => b.Revoke(utcNow));

        var nextSequenceNumber = Beneficiaries.Select(x => x.SequenceNumber).DefaultIfEmpty().Max() + 1;
        beneficiaries.ForEach(b =>
        {
            _beneficiaries.Add(new Beneficiary(nextSequenceNumber, b.Address, b.Details, utcNow));
            nextSequenceNumber++;
        });

        beneficiaries.ToOption().Match(_ => AddRecordsIndicator("ND"), () => RemoveRecordsIndicator("ND"));

        return null;
    }

    public IEnumerable<Beneficiary> ActiveBeneficiaries()
    {
        return Beneficiaries.Where(x => x.RevokeDate == null).ToList();
    }

    public bool HasLinkedMembers()
    {
        return LinkedMembers.Any();
    }

    public double? GetExactAge(DateTimeOffset now)
    {
        if (!PersonalDetails.DateOfBirth.HasValue)
            return null;

        var years = now.Year - PersonalDetails.DateOfBirth.Value.Year;
        var last = PersonalDetails.DateOfBirth.Value.AddYears(years);
        if (last > now)
        {
            last = last.AddYears(-1);
            years--;
        }

        var months = (last.Date, now.Date) switch
        {
            (DateTime s, DateTime e) when (e.Month >= s.Month && e.Day >= s.Day) => e.Month - s.Month,
            (DateTime s, DateTime e) when (e.Month > s.Month && e.Day < s.Day) => e.Month - 1 - s.Month,
            (DateTime s, DateTime e) when (e.Month == s.Month && e.Day < s.Day) => 11,
            (DateTime s, DateTime e) when (e.Month < s.Month && e.Day >= s.Day) => e.Month + 12 - s.Month,
            (DateTime s, DateTime e) when (e.Month < s.Month && e.Day < s.Day) => e.Month + 12 - 1 - s.Month,
            _ => throw new InvalidOperationException()
        };

        var days = (int)(now.Date - last.Date.AddMonths(months)).TotalDays;

        var next = last.AddYears(1);
        double yearDays = (next - last).Days;

        return (double)years + (double)months / 12 + (double)days / yearDays;
    }

    public int? AgeOnSelectedDate(DateTime retirementDate)
    {
        return GetExactAge(retirementDate).HasValue ? Convert.ToInt32(Math.Floor(GetExactAge(retirementDate).Value)) : null;
    }

    public bool IsLastRTP9ClosedOrAbandoned()
    {
        var lastRTP9Case = PaperRetirementApplications
            .Where(x => x.IsRTP9())
            .OrderByDescending(x => x.CaseNumber)
            .FirstOrDefault();

        return lastRTP9Case != null && lastRTP9Case.IsClosedOrAbandoned();
    }

    public bool IsRTP9CaseAbandoned(string caseNumber)
    {
        var lastRTP9Case = PaperRetirementApplications
            .Where(x => x.IsRTP9())
            .OrderByDescending(x => x.CaseReceivedDate)
            .FirstOrDefault(x => x.CaseNumber == caseNumber);

        return lastRTP9Case != null && lastRTP9Case.IsAbandoned();
    }

    public string CurrentAgeIso(DateTimeOffset utcNow)
    {
        var dateOfBirth = (PersonalDetails.DateOfBirth ?? utcNow).Date;
        var timeToRetirement = TimePeriodCalculator.Calculate(dateOfBirth, utcNow.Date);
        return $"{timeToRetirement.Years}Y{timeToRetirement.month}M{timeToRetirement.Weeks}W{timeToRetirement.Days}D";
    }

    public string AgeAtSelectedRetirementDateIso(DateTime? retirementDate)
    {
        var dateOfBirth = (PersonalDetails.DateOfBirth ?? DateTime.UtcNow).Date;
        var timeToRetirement = TimePeriodCalculator.Calculate(dateOfBirth, retirementDate ?? DateTime.UtcNow);
        return $"{timeToRetirement.Years}Y{timeToRetirement.month}M{timeToRetirement.Weeks}W{timeToRetirement.Days}D";
    }

    public bool IsSchemeDc()
    {
        return "DC".Equals(Scheme?.Type, StringComparison.InvariantCultureIgnoreCase);
    }

    public int? TenureInYears()
    {
        if (!DatePensionableServiceStarted.HasValue || !DateJoinedCompany.HasValue)
        {
            return null;
        }

        var startDate = DateJoinedCompany.Value;
        var endDate = DatePensionableServiceStarted.Value;

        if (endDate < startDate)
        {
            return null;
        }

        int years = endDate.Year - startDate.Year;

        if (endDate.Date < startDate.AddYears(years))
        {
            years--;
        }

        return years;
    }

    public bool IsDeathCasesLogged() => PaperRetirementApplications.Any(x => x.IsDeathCasesLogged());

    private int GetContactReferenceNextSequenceNumber(ContactReference currentContactReference)
    {
        return currentContactReference?.SequenceNumber + 1 ?? 1;
    }

    private ContactReference CurrentContactReference()
    {
        return ContactReferences.OrderByDescending(x => x.SequenceNumber).FirstOrDefault();
    }

    private int GetNewAccountSequenceNumber()
    {
        var currentTopSequenceNumber = BankAccounts.Any() ? BankAccounts.Max(x => x.SequenceNumber) : 0;
        return ++currentTopSequenceNumber;
    }

    private int GetFirstAgeLine(DateTimeOffset now, int earliestRetirementAge, int normalRetirementAge)
    {
        var memberAge = GetAge(now);

        if (earliestRetirementAge > memberAge.Value)
            return earliestRetirementAge;

        if (memberAge.Value >= earliestRetirementAge && memberAge.Value < normalRetirementAge)
            return normalRetirementAge;

        return memberAge.Value + 1;
    }

    private int? GetAge(DateTimeOffset now)
    {
        if (!PersonalDetails.DateOfBirth.HasValue)
            return null;

        var age = now.Year - PersonalDetails.DateOfBirth.Value.Year;
        if (PersonalDetails.DateOfBirth > now.AddYears(-age))
            return --age;

        return age;
    }

    private int? GetAgeMonth(DateTimeOffset now)
    {
        if (!PersonalDetails.DateOfBirth.HasValue)
            return null;

        var months = now.Month - PersonalDetails.DateOfBirth.Value.Month;
        if (now.Day < PersonalDetails.DateOfBirth.Value.Day)
            --months;

        if (months < 0)
            return months += 12;

        return months;
    }

    private int GetMinimumPensionAge()
    {
        return CategoryDetail.MinimumPensionAge ?? 55;
    }

    private void AddRecordsIndicator(string indicator)
    {
        if (string.IsNullOrWhiteSpace(RecordsIndicator) || !RecordsIndicator.Contains(indicator))
            RecordsIndicator = $"{RecordsIndicator} {indicator}".Trim();
    }

    private void RemoveRecordsIndicator(string indicator)
    {
        if (!string.IsNullOrWhiteSpace(RecordsIndicator))
            RecordsIndicator = RecordsIndicator.Replace(indicator, string.Empty).Replace("  ", " ").Trim();
    }

    private void AddNewContactReference(Contact contact, long authorizationNumber, int sequenceNumber, DateTimeOffset utcNow)
    {
        var authorization = new Authorization(BusinessGroup, ReferenceNumber, authorizationNumber, utcNow);
        _contactReferences.Add(new ContactReference(contact,
            authorization,
            BusinessGroup,
            ReferenceNumber,
            sequenceNumber,
            utcNow));
    }

    private bool IsNotEligibleToRetire(int preRetirementAgePeriod, int? memberAge, int? earliestRetirementAge)
    {
        return Status != MemberStatus.Pensioner && memberAge < earliestRetirementAge - preRetirementAgePeriod;
    }

    private bool IsPreRetiree(int preRetirementAgePeriod, int? memberAge, double? exactMemberAge, int? earliestRetirementAge)
    {
        return Status != MemberStatus.Pensioner &&
            memberAge >= earliestRetirementAge - preRetirementAgePeriod &&
            exactMemberAge < earliestRetirementAge - (double)RetirementApplicationPeriodInMonths / 12;
    }

    private bool IsEligibleToApplyForRetirement(int? earliestRetirementAge, double? exactMemberAge)
    {
        return earliestRetirementAge.HasValue &&
            Status != MemberStatus.Pensioner
            && exactMemberAge >= earliestRetirementAge - (double)RetirementApplicationPeriodInMonths / 12
            && exactMemberAge < earliestRetirementAge;
    }

    private bool IsEligibleToRetire(int? memberAge, int? earliestRetirementAge, int? normalRetirementAge)
    {
        return Status != MemberStatus.Pensioner &&
            memberAge >= earliestRetirementAge &&
            memberAge < normalRetirementAge;
    }

    private bool IsLateRetirement(int? memberAge, double? exactMemberAge, int? normalRetirementAge, decimal? latestRetirementAge)
    {
        var latestRetirementAgeValue = (double?)latestRetirementAge ?? (double)LatestRetirementAgeInYears;

        return Status != MemberStatus.Pensioner &&
            memberAge >= normalRetirementAge &&
            exactMemberAge < latestRetirementAgeValue - (double)RetirementApplicationPeriodInMonths / 12;
    }

    private bool IsCloseToLatestRetirementAge(int? memberAge, double? exactMemberAge, decimal? latestRetirementAge)
    {
        var latestRetirementAgeValue = (double?)latestRetirementAge ?? (double)LatestRetirementAgeInYears;

        return Status != MemberStatus.Pensioner &&
            exactMemberAge >= latestRetirementAgeValue - (double)RetirementApplicationPeriodInMonths / 12 &&
            exactMemberAge < latestRetirementAgeValue - OverLatestRetirementAgePeriodInMonth / 12;
    }

    private bool IsOverLatestRetirementAge(int? memberAge, double? exactMemberAge, decimal? latestRetirementAge)
    {
        var latestRetirementAgeValue = (double?)latestRetirementAge ?? (double)LatestRetirementAgeInYears;

        return Status != MemberStatus.Pensioner &&
            exactMemberAge >= latestRetirementAgeValue - OverLatestRetirementAgePeriodInMonth / 12;
    }

    private bool IsNewlyRetired(DateTimeOffset now, int newlyRetiredRange, DateTimeOffset retirementDate)
    {
        return Status == MemberStatus.Pensioner &&
            now >= retirementDate &&
            now < retirementDate.AddMonths(newlyRetiredRange);
    }
}