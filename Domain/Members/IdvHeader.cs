using System;
using WTW.MdpService.Domain.Common;

namespace WTW.MdpService.Domain.Members;

public class IdvHeader
{
    protected IdvHeader() { }

    public string Type = "B";
    public string Status = "Y";

    public IdvHeader(
        string businessGroup,
        string referenceNumber,
        string schemeMember,
        long sequenceNumber,
        DateTimeOffset date,
        string caseNumber,
        Address address,
        string phone,
        string email,
        IdvDetail detail)
    {
        BusinessGroup = businessGroup;
        ReferenceNumber = referenceNumber;
        SchemeMember = schemeMember;
        SequenceNumber = sequenceNumber;
        Date = date;
        CaseNumber = caseNumber;
        AddressLine1 = address.StreetAddress1;
        AddressLine2 = address.StreetAddress2;
        AddressLine3 = address.StreetAddress3;
        AddressLine4 = address.StreetAddress4;
        AddressCity = address.StreetAddress5;
        AddressPostCode = address.PostCode;
        IssuingCountryCode = address.CountryCode;
        Phone = phone;
        Email = email;
        Detail = detail;
    }

    public string BusinessGroup { get; }
    public string ReferenceNumber { get; }
    public string SchemeMember { get; }
    public long SequenceNumber { get; }
    public DateTimeOffset Date { get; }
    public string CaseNumber { get; }
    public string AddressLine1 { get; }
    public string AddressLine2 { get; }
    public string AddressLine3 { get; }
    public string AddressLine4 { get; }
    public string AddressCity { get; }
    public string AddressPostCode { get; }
    public string IssuingCountryCode { get; }
    public string Phone { get; }
    public string Email { get; }
    public virtual IdvDetail Detail { get; }

    public static IdvHeader Create(
        string businessGroup,
        string referenceNumber,
        string schemeMember,
        long sequenceNumber,
        DateTimeOffset date,
        string caseNumber,
        Address address,
        string phone,
        string email,
        int detailId,
        string gbgScanResult,
        string documentType,
        int edmsNumber)
    {
        var detail = IdvDetail.Create(detailId, gbgScanResult, documentType, edmsNumber);
        return new IdvHeader(businessGroup, referenceNumber, schemeMember, sequenceNumber, date, caseNumber, address, phone, email, detail);
    }
}