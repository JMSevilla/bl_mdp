using System;
using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Domain.Mdp.Calculations;

namespace WTW.MdpService.Infrastructure.MdpDb;

public interface ITransferCalculationRepository
{
    Task<Option<TransferCalculation>> Find(string businessGroup, string referenceNumber);
    void Remove(TransferCalculation journey);
    Task Create(TransferCalculation journey);
    Task CreateIfNotExists(TransferCalculation transferCalculation);
}