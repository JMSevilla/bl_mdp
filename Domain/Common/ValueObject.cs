using System.Collections.Generic;
using System.Linq;

namespace WTW.MdpService.Domain;

public abstract class ValueObject
{
    protected ValueObject() { }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (GetType() != obj.GetType())
            return false;

        return Parts().SequenceEqual(((ValueObject)obj).Parts());
    }

    public override int GetHashCode()
    {
        return Parts()
            .Aggregate(1, (current, obj) =>
            {
                unchecked
                {
                    return current * 23 + (obj?.GetHashCode() ?? 0);
                }
            });
    }

    protected abstract IEnumerable<object> Parts();

    public static bool operator ==(ValueObject a, ValueObject b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(ValueObject a, ValueObject b)
    {
        return !(a == b);
    }
}