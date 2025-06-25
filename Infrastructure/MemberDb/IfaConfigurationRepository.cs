using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb
{
    public class IfaConfigurationRepository : IIfaConfigurationRepository
    {
        private readonly MemberDbContext _context;

        public IfaConfigurationRepository(MemberDbContext context)
        {
            _context = context;
        }

        public async Task<Option<string>> FindEmail(string businessGroup, string calcType, string ifaName)
        {
            return await _context.IfaConfigurations
                .FirstOrDefaultAsync(x =>
                     x.BusinessGroup == businessGroup &&
                     x.CalculationType == calcType &&
                     x.IfaName == ifaName)
                .Select(x => x.IfaEmail);
        }
    }
}
