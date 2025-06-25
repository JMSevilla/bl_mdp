namespace WTW.MdpService.Infrastructure.Content;

public record TemplateResponse
{
    public string TemplateName { get; init; }
    public string HtmlBody { get; init; }
    public string HtmlHeader { get; init; }
    public string HtmlFooter { get; init; }
    public string EmailSubject { get; init; }
    public string EmailFrom { get; init; }
    public string ContentBlockKeys { get; init; }
    public string DataSummaryKeys { get; init; }
}