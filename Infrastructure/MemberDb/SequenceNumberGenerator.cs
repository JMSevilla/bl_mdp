using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Infrastructure.MemberDb;

public class SequenceNumberGenerator<T> : ValueGenerator<int> where T : class
{
    private readonly Expression<Func<T, int>> _propertyAccessor;
    private readonly Func<T, Expression<Func<T, bool>>>  _whereClause;

    public SequenceNumberGenerator(Expression<Func<T, int>> propertyAccessor, Func<T, Expression<Func<T, bool>>>  whereClause)
    {
        _propertyAccessor = propertyAccessor;
        _whereClause = whereClause;
    }

    public override int Next(EntityEntry entry) => throw new NotImplementedException();

    public override async ValueTask<int> NextAsync(EntityEntry entry, CancellationToken cancellationToken = new())
    {
        var where = _whereClause.Invoke(entry.Entity as T);
        
        var entity = await entry.Context
            .Set<T>()
            .AsNoTracking()
            .Where(where)
            .OrderByDescending(_propertyAccessor)
            .FirstOrDefaultAsync(cancellationToken);

        return entity
            .ToOption()
            .Match(arg => _propertyAccessor.Compile().Invoke(arg) + 1, () => 1);
    }

    public override bool GeneratesTemporaryValues => false;
}