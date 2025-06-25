using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Migrations;

namespace WTW.MdpService.Infrastructure.MdpDb;

public class ContactConfirmationRepository
{
    private readonly MdpDbContext _context;

    public ContactConfirmationRepository(MdpDbContext context)
    {
        _context = context;
    }

    public async Task<Option<ContactConfirmation>> FindLastEmailConfirmation(string businessGroup, string referenceNumber)
    {
        return await _context.ContactConfirmations
            .Where(t => t.BusinessGroup == businessGroup &&
                t.ReferenceNumber == referenceNumber &&
                t.ContactType == ContactType.EmailAddress)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task Create(ContactConfirmation token)
    {
        await _context.ContactConfirmations.AddAsync(token);
    }

    public async Task<Option<ContactConfirmation>> FindLastMobilePhoneConfirmation(string businessGroup, string referenceNumber)
    {
        return await _context.ContactConfirmations
            .Where(t => t.BusinessGroup == businessGroup &&
                t.ReferenceNumber == referenceNumber &&
                t.ContactType == ContactType.MobilePhoneNumber)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();
    }
}