using System;
using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb;

public interface ICalculationHistoryRepository
{
    Task<Option<CalculationHistory>> FindByEventTypeAndSeqNumber(string businessGroup, string referenceNumber, string @event, int seqNo);
    Task<Option<CalculationHistory>> FindLatest(string businessGroup, string referenceNumber, Int32 calcSystemHistorySeqno);
}