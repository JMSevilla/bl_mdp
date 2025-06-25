using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore.Storage;

namespace WTW.MdpService.Infrastructure.MdpDb;

public interface IMdpUnitOfWork
{
    MdpDbContext Context { get; }
    Task Commit();
    void Remove(object obj);
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<Either<string, bool>> TryCommit();
}