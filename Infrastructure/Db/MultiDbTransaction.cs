using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.MemberDb;

namespace WTW.MdpService.Infrastructure.Db;

public class MultiDbTransaction
{
    private readonly IMemberDbUnitOfWork _memberDbUow;
    private readonly IMdpUnitOfWork _mdpDbUow;
    private IDbContextTransaction _memberDbTransaction;
    private IDbContextTransaction _mdpDbTransaction;

    public MultiDbTransaction(IMemberDbUnitOfWork memberDbUow, IMdpUnitOfWork mdpDbUow)
    {
        _memberDbUow = memberDbUow;
        _mdpDbUow = mdpDbUow;
    }

    public async Task<MultiDbTransaction> Begin()
    {
        if (_memberDbTransaction == null)
            _memberDbTransaction = await _memberDbUow.BeginTransactionAsync();
        if (_mdpDbTransaction == null)
            _mdpDbTransaction = await _mdpDbUow.BeginTransactionAsync();
        return this;
    }

    public async ValueTask Rollback()
    {
        ValidateTransactions();
        await _memberDbTransaction.RollbackAsync();
        await _mdpDbTransaction.RollbackAsync();
    }

    public async ValueTask Commit()
    {
        ValidateTransactions();
        await CommitTransactions();
    }

    private async Task CommitTransactions()
    {
        await _memberDbUow.Commit();
        await _mdpDbUow.Commit();

        await _memberDbTransaction.CommitAsync();
        await _mdpDbTransaction.CommitAsync();
        await _memberDbTransaction.DisposeAsync();
        await _mdpDbTransaction.DisposeAsync();
    }

    private void ValidateTransactions()
    {
        if (_memberDbTransaction == null || _mdpDbTransaction == null)
            throw new InvalidOperationException("Transaction not started");
    }
}