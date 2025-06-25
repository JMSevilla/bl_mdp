using System;
using System.Collections.Generic;
using System.Text.Json;
using WTW.Web.Serialization;

namespace WTW.MdpService.Infrastructure.Templates.GenericJourneys;

public class DataSummaryBlockValues
{
    public Dictionary<string, object> Create(string json)
    {
        return ConvertJsonToDictionary(json);
    }

    Dictionary<string, object> ConvertJsonToDictionary(string json)
    {
        var result = new Dictionary<string, object>();
        var jsonObject = JsonSerializer.Deserialize<JsonElement>(json, SerialiationBuilder.Options());

        foreach (var prop in jsonObject.EnumerateObject())
        {
            AddToDictionary(result, prop.Name, prop.Value);
        }

        return result;
    }

    void AddToDictionary(Dictionary<string, object> dictionary, string prefix, JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in value.EnumerateObject())
                {
                    AddToDictionary(dictionary, $"{prefix}.{prop.Name}", prop.Value);
                }
                break;
            case JsonValueKind.Array:
                var lines = new List<string>();
                foreach (var line in value.EnumerateArray())
                {
                    lines.Add(line.ToString() + "<br>");
                }
                var addres = string.Join("\n", lines);
                dictionary[prefix] = addres;
                break;
            default:
                dictionary[prefix] = value.ToString();
                break;
        }
    }
}
