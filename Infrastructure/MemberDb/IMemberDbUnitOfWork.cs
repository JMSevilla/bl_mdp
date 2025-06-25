using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace WTW.MdpService.Infrastructure.MemberDb;

public interface IMemberDbUnitOfWork
{
    Task Commit();
    Task<IDbContextTransaction> BeginTransactionAsync();
}