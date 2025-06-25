using System;
using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Documents;

public record DocumentsRequest
{
    [MaxLength(60)]
    public string Name { get; init; }

    [MaxLength(60)]
    public string Type { get; init; }
    public DateTimeOffset? ReceivedDateFrom { get; init; }
    public DateTimeOffset? ReceivedDateTo { get; init; }
    public ReadStatus? DocumentReadStatus { get; init; }

    [Required]
    public bool Ascending { get; init; }

    [Required]
    public SortPropertyName PropertyName { get; init; }

    [Required]
    [Range(1, int.MaxValue)]
    public int PageNumber { get; init; }

    [Required]
    [Range(1, int.MaxValue)]
    public int PageSize { get; init; }

    public enum SortPropertyName
    {
        Name,
        DateReceived,
        Type
    }

    public enum ReadStatus
    {
        Read,
        Unread
    }
}