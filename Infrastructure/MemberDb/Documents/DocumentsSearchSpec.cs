using System;
using System.Linq.Expressions;
using WTW.MdpService.Domain.Members;
using WTW.Web.Specs;

namespace WTW.MdpService.Infrastructure.MemberDb.Documents;

public class DocumentsSearchSpec : Spec<Document>
{
    private readonly string _referenceNumber;
    private readonly string _businessGroup;
    private readonly string _name;
    private readonly DateTimeOffset? _receivedDateFrom;
    private readonly DateTimeOffset? _receivedDateTo;
    private readonly string _type;
    private readonly string _documentReadStatus;

    public DocumentsSearchSpec(string referenceNumber,
        string businessGroup,
        string name,
        string type,
        string documentReadStatus,
        DateTimeOffset? receivedDateFrom,
        DateTimeOffset? receivedDateTo)
    {
        _referenceNumber = referenceNumber;
        _businessGroup = businessGroup;
        _name = name;
        _type = type;
        _documentReadStatus = documentReadStatus;
        _receivedDateFrom = receivedDateFrom;
        _receivedDateTo = receivedDateTo;
    }

    public override Expression<Func<Document, bool>> ToExpression()
    {
        Expression<Func<Document, bool>> expresion = (a) =>
              _referenceNumber == a.ReferenceNumber && _businessGroup == a.BusinessGroup &&
              (_name == null || a.Name.ToLower().Contains(_name.ToLower())) &&
              (_type == null || a.Type == _type) &&
              (_type == null || a.Type == _type) &&
              (_receivedDateFrom == null || a.Date >= _receivedDateFrom) &&
              (_receivedDateTo == null || a.Date <= _receivedDateTo.Value.AddDays(1).AddMilliseconds(-1));

        return (_documentReadStatus) switch
        {
            ("Read") => expresion.And((a) => a.LastReadDate != null),
            ("Unread") => expresion.And((a) => a.LastReadDate == null),
            _ => expresion
        };
    }
}