using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Domain.Members.Beneficiaries;
using static Oracle.ManagedDataAccess.Client.OracleDbType;
using static System.Data.ParameterDirection;
using WTW.Web.LanguageExt;
using System.Data;

namespace WTW.MdpService.Infrastructure.MemberDb;

public class MemberRepository : IMemberRepository
{
    private readonly MemberDbContext _context;

    public MemberRepository(MemberDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsMember(string referenceNumber, string businessGroup)
    {
        return await _context.Members.AnyAsync(x => x.ReferenceNumber == referenceNumber && x.BusinessGroup == businessGroup);
    }

    public async Task<Option<Member>> FindMember(string referenceNumber, string businessGroup, string mf2FaStatus = default)
    {
        await _context.Set<ContactReference>()
            .Where(x => x.ReferenceNumber == referenceNumber && x.BusinessGroup == businessGroup)
            .OrderByDescending(x => x.SequenceNumber)
            .FirstOrDefaultAsync();
        await _context.Set<BankAccount>()
           .Where(x => x.ReferenceNumber == referenceNumber && x.BusinessGroup == businessGroup)
           .OrderByDescending(x => x.SequenceNumber)
           .FirstOrDefaultAsync();
        await _context.Set<NotificationSetting>()
          .Where(x => x.ReferenceNumber == referenceNumber && x.BusinessGroup == businessGroup)
          .OrderByDescending(x => x.SequenceNumber)
          .FirstOrDefaultAsync();

        if (mf2FaStatus == "Y")
        {
            await _context.Set<ContactValidation>()
                .Where(x =>
                    x.ReferenceNumber == referenceNumber &&
                    x.BusinessGroup == businessGroup &&
                    (x.ContactType == MemberContactType.EmailAddress || x.ContactType == MemberContactType.MobilePhoneNumber1))
                .ToListAsync();

            await _context.Set<ContactCountry>()
                .Where(x => x.ReferenceNumber == referenceNumber && x.BusinessGroup == businessGroup)
                .FirstOrDefaultAsync();
        }

        return await _context.Members
            .FirstOrDefaultAsync(x => x.ReferenceNumber == referenceNumber && x.BusinessGroup == businessGroup);
    }
    
    public async Task<Option<List<LinkedMember>>> FindLinkedMembers(string referenceNumber, string businessGroup, string linkedReferenceNumber, string linkedBusinessGroup)
    {
        return await _context.Set<LinkedMember>()
            .Where(x => x.ReferenceNumber == referenceNumber && x.BusinessGroup == businessGroup &&
                        (referenceNumber == linkedReferenceNumber && businessGroup == linkedBusinessGroup || 
                         x.LinkedReferenceNumber == linkedReferenceNumber && x.LinkedBusinessGroup == linkedBusinessGroup))
            .ToListAsync();
    }

    public async Task<Option<Member>> FindMemberWithBeneficiaries(string referenceNumber, string businessGroup)
    {
        await _context.Set<Beneficiary>()
          .Where(x => x.ReferenceNumber == referenceNumber && x.BusinessGroup == businessGroup)
          .OrderByDescending(y => y.SequenceNumber)
          .FirstOrDefaultAsync();

        return await _context.Members
            .Include(m => m.Beneficiaries.Where(y => !y.RevokeDate.HasValue).OrderBy(y => y.SequenceNumber))
            .FirstOrDefaultAsync(x => x.ReferenceNumber == referenceNumber && x.BusinessGroup == businessGroup);
    }

    public async Task<Option<Member>> FindMemberWithDependant(string referenceNumber, string businessGroup)
    {
        return await _context.Members
            .Include(m => m.Dependants.Where(y => !y.EndDate.HasValue).OrderBy(y => y.SequenceNumber))
            .FirstOrDefaultAsync(x => x.ReferenceNumber == referenceNumber && x.BusinessGroup == businessGroup);
    }

    public async Task<bool> IsMemberValidForRaCalculation(string referenceNumber, string businessGroup)
    {
        switch (businessGroup)
        {
            case "JSP":
                var recordCount = await RaCalculationValidationQuery(referenceNumber, businessGroup);
                if (recordCount > 0)
                    return false;
                else
                    return true;
            default:
                return true;
        }
    }

    public async Task<bool> IsMemberValidForTransferCalculation(string referenceNumber, string businessGroup)
    {
        switch (businessGroup)
        {
            case "JSP":
                var recordCount = await TaCalculationValidationQuery(referenceNumber, businessGroup);
                if (recordCount > 0)
                    return false;
                else
                    return true;
            default:
                return true;
        }
    }

    private async Task<int> RaCalculationValidationQuery(string referenceNumber, string businessGroup)
    {
        var parameters = new OracleParameter("v_bgroup", Varchar2, 100, businessGroup, Input).ToOption()
            .Concat(new OracleParameter("p_refno", Varchar2, 100, referenceNumber, Input).ToOption())
            .ToArray();

        var recordCount = (await _context.Database.ExecuteQuery<int>(
               @"SELECT count(*) FROM BASIC ba
                 WHERE ba.BGROUP = :v_bgroup AND ba.REFNO = :p_refno
                 AND (EXISTS (SELECT 1 FROM REMARKS_HISTORY
                 WHERE REMARKS_HISTORY.BGROUP = ba.bgroup
                 AND REMARKS_HISTORY.REFNO = ba.refno
                 AND REMARKS_HISTORY.RH03X in ('XAU1'))
                 OR CA26X IN ('0004','0005','1002','1003')
                 OR TRUNC(SYSDATE) > ADD_MONTHS(BD11D,898)
                 OR 1 = (SELECT CASE WHEN (SUM(ppt_tot.PT06P) = SUM(gmp_tot.PT06P)) THEN 1 ELSE 0 END
                 FROM PP_TRANCHE ppt_tot, PP_TRANCHE gmp_tot
                 WHERE ppt_tot.BGROUP = ba.bgroup
                 AND ppt_tot.BGROUP = gmp_tot.BGROUP
                 AND ppt_tot.REFNO = ba.refno
                 AND ppt_tot.REFNO = gmp_tot.REFNO
                 AND ppt_tot.PT02X IN (SELECT ppt_component.ptc03x FROM ppt_component
                              WHERE ppt_component.bgroup = ppt_tot.bgroup
                              AND ppt_component.ptc01x = 'MP')
                              AND gmp_tot.pt02x IN ('PENSGMPO', 'PENSGMPR'))
                 OR  EXISTS (SELECT 1 from AUGMENTATION_BENEFIT where
                      AUGMENTATION_BENEFIT.BGROUP = ba.bgroup
                      AND AUGMENTATION_BENEFIT.SUB = 'BUYB'
                      AND AUGMENTATION_BENEFIT.REFNO = ba.refno)
                 OR  EXISTS (SELECT 1 from TRANSFER_IN where
                      TRANSFER_IN.BGROUP = ba.bgroup
                      AND TRANSFER_IN.SUB in ('TMP1', 'TMP2', 'TMP3')
                      AND TRANSFER_IN.REFNO = ba.refno)
                 OR  EXISTS (SELECT 1 FROM avc_detail
                             WHERE bgroup = ba.bgroup
                             AND refno = ba.refno)      
                 OR EXISTS(SELECT * FROM transition_protection
                          WHERE bgroup = ba.bgroup              
                          AND refno = ba.REFNO
                          AND TPB10D is null))",
               read => Convert.ToInt32(read["COUNT(*)"]),
               parameters)).First();

        return recordCount;
    }

    private async Task<int> TaCalculationValidationQuery(string referenceNumber, string businessGroup)
    {
        var parameters = new OracleParameter("v_bgroup", Varchar2, 100, businessGroup, Input).ToOption()
            .Concat(new OracleParameter("p_refno", Varchar2, 100, referenceNumber, Input).ToOption())
            .ToArray();

        var recordCount = (await _context.Database.ExecuteQuery<int>(
               @"SELECT count(*) FROM BASIC ba
                 WHERE ba.BGROUP = :v_bgroup AND ba.REFNO = :p_refno
                 AND (EXISTS  (SELECT 1 FROM REMARKS_HISTORY
                               WHERE REMARKS_HISTORY.BGROUP = ba.BGROUP
                               AND REMARKS_HISTORY.REFNO = ba.REFNO
                               AND REMARKS_HISTORY.RH03X in ('XAU1'))   
                 OR CA26X = '0003'
                 OR TRUNC(SYSDATE) > ADD_MONTHS(BD11D,898)    
                 OR  EXISTS (SELECT 1 from TRANSFER_IN where
                             TRANSFER_IN.BGROUP = ba.BGROUP
                             AND TRANSFER_IN.SUB in ('TMP1', 'TMP2', 'TMP3')
                             AND TRANSFER_IN.REFNO = ba.REFNO)
                 OR  EXISTS (SELECT 1 FROM avc_detail
                             WHERE bgroup = ba.BGROUP
                             AND refno = ba.refno)              
                 OR EXISTS(SELECT 1 FROM court_order_history CH
                           WHERE ch.bgroup = ba.BGROUP
                           AND ch.refno = ba.REFNO
                           AND ch.cr04a IN ('P','X')))",
               read => Convert.ToInt32(read["COUNT(*)"]),
               parameters)).First();

        return recordCount;
    }


    public async Task PopulateSessionDetails(string bGroup)
    {
        var defaultErrorCode = 0;
        var defaultErrorMessage = string.Empty;

        var parameters = new[] {
            new OracleParameter("i_errorCode", OracleDbType.Int32, 9, defaultErrorCode, ParameterDirection.Output),
            new OracleParameter("i_errorMessage", OracleDbType.Varchar2, 255, defaultErrorMessage, ParameterDirection.Output),
            new OracleParameter("i_bGroup", OracleDbType.Varchar2, 3, bGroup, ParameterDirection.Input)
        };
        var query =
            @"BEGIN
                WW_PMS_WEB_INTERFACE.populate_session_details(:i_errorCode, :i_errorMessage, :i_bGroup, 0,'S_MEMBER','MEMBERSERV');
              END;";

        await _context!.Database.ExecuteSqlRawAsync(query, parameters);

        CheckForErrorOutput(parameters);

    }

    public async Task DisableSysAudit(string bGroup)
    {

        var query =
            @"BEGIN
                PMS_ZZ_AUDIT.set_auditing_off;
              END;";

        await _context!.Database.ExecuteSqlRawAsync(query);

    }

    private static void CheckForErrorOutput(OracleParameter[] parameters)
    {
        var errorCodeIsInt = int.TryParse(parameters[0].Value.ToString(), out int errorCode);
        var errorMessage = parameters[1].Value.ToString();

        if ((errorCodeIsInt && errorCode > 0) ||
            (!string.IsNullOrEmpty(errorMessage) && errorMessage != "null"))
        {
            throw new Exception($"{errorCode} {errorMessage}");
        }
    }
}