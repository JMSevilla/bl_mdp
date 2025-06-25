using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LanguageExt;

namespace WTW.MdpService.Infrastructure.Edms;

public record PostIndexError
{
    //FYI: There are 3 different error response formats for edms post  index endpoint.
    //See task: https://tas-member-digital-platform.atlassian.net/browse/MAB-8741?focusedCommentId=45212

    //on 400 when input validation fails
    public JsonElement? Errors { get; init; }
    public string Message { get; init; }

    //on 400 when documents post index fails
    public List<PostindexDocumentResponse> Documents { get; init; }

    //on 401
    public string Error { get; init; }
    public string Description { get; init; }
    [JsonPropertyName("status_code")]
    public int? StatusCode { get; init; }

    public string GetErrorMessage()
    {
        return (Get401ErrorMessage() + GetValidationErrorMessage() + GetDocumentsPostIndexErrorMessage()).Trim();
    }

    private string GetDocumentsPostIndexErrorMessage()
    {
        if (Documents == null)
            return string.Empty;

        return "Documents post index Errors: " + string.Join(' ', Documents.Select(x => $"\'Message: {x.Message}.\'"));
    }

    private string GetValidationErrorMessage()
    {
        var sb = new StringBuilder();
        if (Message != null)
            sb.Append($" Error message: {Message}.");

        if (Errors.HasValue)
            foreach (var prop in Errors.Value.EnumerateObject())
                if (prop.Value.ValueKind == JsonValueKind.String && prop.Value.GetString() != null)
                    sb.Append($" {prop.Value.GetString()}.");

        return sb.ToString();
    }

    private string Get401ErrorMessage()
    {
        var sb = new StringBuilder();
        if (Error != null)
            sb.Append($" Error: {Error}.");

        if (Description != null)
            sb.Append($" Error description: {Description}.");

        if (StatusCode != null)
            sb.Append($" StatusCode: {StatusCode}.");

        return sb.ToString();
    }
}