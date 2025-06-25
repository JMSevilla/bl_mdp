namespace WTW.MdpService.Content;

public record ContentAccessKeyResponse
{
    public string ContentAccessKey { get; init; }
    public string SchemeType { get; init; }
    public string SchemeCodeAndCategory { get; init; }

    public static ContentAccessKeyResponse From(string contentAccessKey, string schemeType, string schemeCode, string category)
    {
        return new()
        {
            ContentAccessKey = contentAccessKey,
            SchemeType = schemeType,
            SchemeCodeAndCategory = $"{schemeCode}-{category}"
        };
    }
}