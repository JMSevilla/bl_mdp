using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.Templates.RetirementApplication.Submit;

public class RetirementSummary
{
    public List<SummaryFigure> SummaryFigures { get; set; } = new();
}

public class SummaryFigure
{
    public string Label { get; set; }
    public string Value { get; set; }
    public string Description { get; set; }
}