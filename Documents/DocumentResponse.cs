using System;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Documents;

public record DocumentResponse
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string Type { get; init; }
    public bool IsRead { get; init; }
    public DateTimeOffset DateReceived { get; init; }

    public static DocumentResponse From(Document document)
    {
        return new()
        {
            Id = document.Id,
            Name = document.Name,
            Type = document.Type,
            IsRead = document.LastReadDate.HasValue,
            DateReceived = document.Date
        };
    }
}