using System;
using System.Collections.Generic;
using System.Text.Json;
using WTW.MdpService.Infrastructure.Content;

namespace WTW.MdpService.Journeys.Submit.Services.DocumentsRenderer;

public interface ICmsDataParser
{
    IEnumerable<string> GetContentBlockKeys(TemplateResponse template);
    IEnumerable<Uri> GetDataSummaryBlockSourceUris(JsonElement dataSummaryContent);
    IEnumerable<Uri> GetContentBlockSourceUris(IEnumerable<JsonElement> contentBlocksContent);
    IEnumerable<string> GetDataSummaryKeys(TemplateResponse template);
}