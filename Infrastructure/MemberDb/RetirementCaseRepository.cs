using System;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using WTW.Web.LanguageExt;
using static System.Data.ParameterDirection;
using static Oracle.ManagedDataAccess.Client.OracleDbType;
using static System.StringComparison;
using WTW.MdpService.Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace WTW.MdpService.Infrastructure.MemberDb;

public class RetirementCaseRepository
{
    private readonly MemberDbContext _context;

    public RetirementCaseRepository(MemberDbContext context)
    {
        _context = context;
    }

    public async Task<Option<PaperRetirementApplication>> Find(string caseNumber)
    {
        return await _context.Cases.FirstOrDefaultAsync(x => x.CaseNumber == caseNumber);
    }

    public async Task<Either<string, CaseResult>> Create(
        string businessGroup,
        string referenceNumber,
        DateTimeOffset now,
        Option<string> statusCode,
        Option<string> complaint)
    {
        var parameters = new OracleParameter("p_bgroup", Varchar2, 100, businessGroup, Input).ToOption()
            .Concat(new OracleParameter("p_refno", Varchar2, 100, referenceNumber, Input).ToOption())
            .Concat(new OracleParameter("p_casecode", Varchar2, 100, "RTP9", Input).ToOption())
            .Concat(new OracleParameter("p_bsource", Varchar2, 100, "MDP", Input).ToOption())
            .Concat(new OracleParameter("p_batch_desc", Varchar2, 100, "Created by retirement application", Input).ToOption())
            .Concat(new OracleParameter("p_notes", Varchar2, 100, "Case created by an online retirement application", Input).ToOption())
            .Concat(new OracleParameter("p_sticky_notes", Varchar2, 100, "Case created by an online retirement application", Input).ToOption())
            .Concat(new OracleParameter("p_create_case", Varchar2, 100, "Y", Input).ToOption())
            .Concat(new OracleParameter("p_caserecd", Date, 100, now.DateTime, Input).ToOption())
            .Concat(statusCode.Map(s => new OracleParameter("p_status", Varchar2, 1, null, Input)))
            .Concat(complaint.Map(c => new OracleParameter("p_complaint", Varchar2, 1, ToYN(c), Input)))
            .ToList();

        var result = (await _context.Database.ExecuteFunction<OracleString>(
            "ww_pms_create_caseapi.create_new_case",
            parameters,
            new OracleParameter("return_value", Varchar2, 100)))
            .Value;

        if (!result.StartsWith("error", OrdinalIgnoreCase))
            return new CaseResult(
                result.Split(",")[1],
                int.Parse(result.Split(",")[0]));

        return result;
    }

    public async Task Delete(
        string businessGroup,
        string caseNumber)
    {
        var parameters = new OracleParameter("v_bgroup", Varchar2, 100, businessGroup, Input).ToOption()
            .Concat(new OracleParameter("v_caseno", Varchar2, 10, caseNumber, Input).ToOption())
            .ToList();

        await _context.Database.ExecuteSql(@"
            BEGIN
            DELETE FROM ww_event_input_detail WHERE bgroup = :v_bgroup AND caseno = :v_caseno;
            DELETE FROM ww_event_input_head WHERE bgroup = :v_bgroup AND caseno = :v_caseno;
            DELETE FROM cw_case_list WHERE bgroup = :v_bgroup AND caseno = :v_caseno;
            DELETE FROM cw_case_list_ext WHERE bgroup = :v_bgroup AND caseno = :v_caseno;
            DELETE FROM cw_task_list WHERE bgroup = :v_bgroup AND caseno = :v_caseno;
            DELETE FROM cw_task_list_ext WHERE bgroup = :v_bgroup AND caseno = :v_caseno;
            DELETE FROM ww_wf_case_relevant_data WHERE bgroup = :v_bgroup AND caseno = :v_caseno;
            DELETE FROM cw_case_flow WHERE bgroup = :v_bgroup AND caseno = :v_caseno;
            DELETE FROM ww_wf_batch_create WHERE batchno = (SELECT batchno FROM ww_wf_batch_create_detail
                                WHERE bgroup = :v_bgroup
                                AND caseno = :v_caseno);     
            DELETE FROM ww_wf_batch_create_detail WHERE bgroup = :v_bgroup AND caseno = :v_caseno;
            COMMIT;
            END;",
            parameters);
    }

    private string ToYN(string value)
    {
        return !string.IsNullOrEmpty(value) ? "Y" : "N";
    }

    public record CaseResult(string CaseNumber, int BatchNumber);
}