using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace WTW.MdpService.Infrastructure.BereavementDb;

public interface IBereavementUnitOfWork
{
    Task Commit();
    Task<IDbContextTransaction> BeginTransactionAsync();
}