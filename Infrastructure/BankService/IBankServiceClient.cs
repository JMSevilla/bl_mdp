using System.Net;
using System.Threading.Tasks;
using LanguageExt;

namespace WTW.MdpService.Infrastructure.BankService;

public interface IBankServiceClient
{
    Task<HttpStatusCode> AddBankAccount(string bgroup, string refNo, AddBankAccountPayload payload);
    Task<Option<GetBankAccountClientResponse>> GetBankAccount(string bgroup, string refNo);
    Task<ValidateBankAccountResponse> ValidateBankAccount(string bgroup, string refNo, ValidateBankAccountPayload payload);
    Task<VerifySafePaymentResponse> VerifySafePayment(string bgroup, string refNo, VerifySafePaymentPayload payload);
}
