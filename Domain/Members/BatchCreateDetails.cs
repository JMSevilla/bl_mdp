using System;

namespace WTW.MdpService.Domain.Members;

public class BatchCreateDetails
{
    private const string NotesStartsWith = "Case created by an online ";
    private const string NotesEndWith = " application";
    protected BatchCreateDetails() { }

    public BatchCreateDetails(string notes)
    {
        Notes = notes;
    }

    public string Notes { get; }

    public bool IsPaperRetirementApplicationSubmitted()
    {
        return !(Notes is not null && Notes.StartsWith(NotesStartsWith, StringComparison.OrdinalIgnoreCase) && Notes.EndsWith(NotesEndWith, StringComparison.OrdinalIgnoreCase));
    }
}