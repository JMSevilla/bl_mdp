using System;
using System.Collections.Generic;
using System.Linq;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Journeys.JourneysGenericData;
using WTW.MdpService.JourneysCheckboxes;

namespace WTW.MdpService.TransferJourneys;

public record TransferApplicationResponse
{
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? SubmissionDate { get; set; }
    public DateTimeOffset? PensionWiseDate { get; set; }
    public DateTimeOffset? FinancialAdviseDate { get; set; }
    public IEnumerable<TransferJourneyContactResponse> TransferJourneyContacts { get; set; }
    public IEnumerable<TransferQuestionFormResponse> QuestionForms { get; set; }
    public FlexibleBenefitsResponse FlexibleBenefits { get; private set; }
    public IEnumerable<JourneyCheckboxesResponse> CheckboxesLists { get; private set; }
    public IEnumerable<JourneyGenericDataResponse> JourneyGenericDataList { get; set; }
    public decimal TotalGuaranteedTransferValue { get; set; }
    public decimal TotalNonGuaranteedTransferValue { get; set; }
    public TransferApplicationStatus TransferApplicationStatus { get; set; }

    public TransferApplicationResponse(TransferJourney journey, Domain.Members.TransferApplicationStatus transferApplicationStatus)
    {
        CreateInitialResponse(journey, transferApplicationStatus);
    }

    public TransferApplicationResponse(TransferJourney journey, Domain.Mdp.Calculations.TransferQuote transferQuote, Domain.Members.TransferApplicationStatus transferApplicationStatus)
    {
        CreateInitialResponse(journey, transferApplicationStatus);
        TotalGuaranteedTransferValue = transferQuote.TransferValues.TotalGuaranteedTransferValue;
        TotalNonGuaranteedTransferValue = transferQuote.TransferValues.TotalNonGuaranteedTransferValue;
    }

    private void CreateInitialResponse(TransferJourney journey, TransferApplicationStatus transferApplicationStatus)
    {

        StartDate = journey.StartDate;
        SubmissionDate = journey.SubmissionDate;
        PensionWiseDate = journey.PensionWiseDate;
        FinancialAdviseDate = journey.FinancialAdviseDate;
        TransferApplicationStatus = transferApplicationStatus;
        TransferJourneyContacts = journey.Contacts.Select(c => new TransferJourneyContactResponse
        {
            Type = c.Type,
            Name = c.Name,
            AdvisorName = c.AdvisorName,
            CompanyName = c.CompanyName,
            Email = c.Email,
            PhoneCode = c.Phone?.Code(),
            PhoneNumber = c.Phone?.Number(),
            SchemeName = c.SchemeName,
            Address = new TransferJourneyContactResponse.TransferJourneyContactAddressResponse
            {
                Line1 = c.Address.StreetAddress1,
                Line2 = c.Address.StreetAddress2,
                Line3 = c.Address.StreetAddress3,
                Line4 = c.Address.StreetAddress4,
                Line5 = c.Address.StreetAddress5,
                Country = c.Address.Country,
                CountryCode = c.Address.CountryCode,
                PostCode = c.Address.PostCode
            }
        });
        QuestionForms = journey.QuestionForms(new string[0]).Select(q => new TransferQuestionFormResponse
        {
            QuestionKey = q.QuestionKey,
            AnswerKey = q.AnswerKey,
            AnswerValue = q.AnswerValue,
        });
        FlexibleBenefits = new FlexibleBenefitsResponse(journey.NameOfPlan, journey.TypeOfPayment, journey.DateOfPayment);
        CheckboxesLists = journey.CheckBoxesLists().Select(x => new JourneyCheckboxesResponse(x));
        JourneyGenericDataList = journey.GetJourneyGenericDataList().Select(x => new JourneyGenericDataResponse(x));
    }
}

public record TransferJourneyContactResponse
{
    public string Type { get; set; }
    public string Name { get; set; }
    public string AdvisorName { get; set; }
    public string CompanyName { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string SchemeName { get; set; }
    public TransferJourneyContactAddressResponse Address { get; set; }

    public record TransferJourneyContactAddressResponse
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Line4 { get; set; }
        public string Line5 { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string PostCode { get; set; }
    }
}