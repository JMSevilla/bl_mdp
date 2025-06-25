using System;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.Infrastructure.Calculations;

public interface ICalculationsClient
{
    TryAsync<TypeResponse> TransferEventType(string businessGroup, string referenceNumber, string requestType = "transfer");
    Task<Either<Error, string>> HardQuote(string businessGroup, string referenceNumber);
    Task<Either<Error, string>> GetGuaranteedTransfer(string businessGroup, string referenceNumber);
    Task<Either<Error, (string, int)>> RetirementQuote(string businessGroup, string referenceNumber, DateTime effectiveDate);
    Task<Either<Error, PartialTransferResponse.MdpResponse>> PartialTransferValues(string businessGroup, string referenceNumber, decimal? requestedTransferValue, decimal? requestedResidualPension);
    Task<Either<Error, PartialTransferResponse.MdpResponse>> PensionTranches(string businessGroup, string referenceNumber, decimal requestedTransferValue);
    Task<Either<Error, (RetirementResponse RetirementResponse, string EventType)>> RetirementCalculation(string referenceNumber, string businessGroup, DateTime effectiveDate);
    Task<Either<Error, (RetirementResponse RetirementResponse, string EventType)>> RetirementCalculation(string referenceNumber, string businessGroup, DateTime effectiveDate, decimal requestedLumpSum);
    Task<Either<Error, (RetirementResponseV2 RetirementResponseV2, string EventType)>> RetirementCalculationV2(string referenceNumber, string businessGroup, DateTime effectiveDate, bool engineGuaranteeQuote, bool bypassCache = false);
    Task<Either<Error, (RetirementResponseV2 RetirementResponseV2, string EventType)>> RetirementCalculationV2(string referenceNumber, string businessGroup, DateTime effectiveDate, decimal requestedLumpSum, DateTime? factorDate = null);
    TryAsync<RetirementDatesAgesResponse> RetirementDatesAges(string referenceNumber, string businessGroup);
    Task<Either<Error, TransferResponse>> TransferCalculation(string businessGroup, string referenceNumber, bool lockQuote = false);
    Task<Either<Error, PartialTransferResponse.MdpResponse>> TransferValues(string businessGroup, string referenceNumber, decimal requestedResidualPension);
    Task<Either<Error, RetirementResponseV2>> RetirementCalculationV2WithLock(string referenceNumber, string businessGroup, string eventType, DateTime effectiveDate, decimal? requestedLumpSum);
    Task<Either<Error, RetirementResponseV2>> RateOfReturn(string businessGroup, string referenceNumber, DateTimeOffset startDate, DateTimeOffset effectiveDate);
    Task<Either<Error, GetGuaranteedQuoteResponse>> GetGuaranteedQuotes(GetGuaranteedQuoteClientRequest request);
}