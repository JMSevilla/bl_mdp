using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb;

public interface IBankHolidayRepository
{
    Task<ICollection<BankHoliday>> ListFrom(DateTime fromDate);
}
