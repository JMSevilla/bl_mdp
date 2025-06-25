using System.Collections.Generic;
using System.Threading.Tasks;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Infrastructure.MdpDb;

public interface IRetirementPostIndexEventRepository
{
    Task<List<RetirementPostIndexEvent>> List();
    void Add(RetirementPostIndexEvent ev);
    void Delete(RetirementPostIndexEvent ev);
}