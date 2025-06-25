using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Cases;

public record CaseListRequest
{
    public IEnumerable<string> CaseCodes { get; init; }
    public bool? Ascending { get; init; }
    public CaseSortPropertyName? PropertyName { get; init; }

    [Range(0, int.MaxValue)]
    public int PageNumber { get; init; }

    [Range(0, int.MaxValue)]
    public int PageSize { get; init; }
}