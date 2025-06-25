using System.Collections.Generic;
using System.Linq;
using WTW.MdpService.Domain.Common.Journeys;

namespace WTW.MdpService.JourneysCheckboxes;

public record JourneyCheckboxesResponse
{
    public JourneyCheckboxesResponse(CheckboxesList checkboxesList)
    {
        CheckboxesListKey = checkboxesList.CheckboxesListKey;
        Checkboxes = checkboxesList.Checkboxes.Select(x => new CheckboxResponse(x.Key, x.AnswerValue));
    }

    public string CheckboxesListKey { get; }
    public IEnumerable<CheckboxResponse> Checkboxes { get; }
}