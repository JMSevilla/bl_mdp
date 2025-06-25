using System.Collections.Generic;
using System.Linq;

namespace WTW.MdpService.Domain.Common.Journeys;

public class CheckboxesList
{
    private readonly List<Checkbox> _checkboxes = new();

    protected CheckboxesList() { }

    public CheckboxesList(string checkboxesListKey, IEnumerable<(string Key, bool Value)> checkboxes)
    {
        CheckboxesListKey = checkboxesListKey;
        _checkboxes = checkboxes.Select(x => new Checkbox(x.Key, x.Value)).ToList();
    }

    private CheckboxesList(string checkboxesListKey, List<Checkbox> checkboxes)
    {
        CheckboxesListKey = checkboxesListKey;
        _checkboxes = checkboxes;
    }

    public string CheckboxesListKey { get; }
    public virtual IReadOnlyList<Checkbox> Checkboxes => _checkboxes;

    public CheckboxesList Duplicate()
    {
        return new(CheckboxesListKey, Checkboxes.Select(x => x.Duplicate()).ToList());
    }
}