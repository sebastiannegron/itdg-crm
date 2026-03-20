namespace Itdg.Crm.Api.Infrastructure.Services;

using System.Text.RegularExpressions;

public partial class TemplateRenderer : ITemplateRenderer
{
    public string Render(string template, IDictionary<string, string> mergeFields)
    {
        return MergeFieldPattern().Replace(template, match =>
        {
            var fieldName = match.Groups[1].Value.Trim();
            return mergeFields.TryGetValue(fieldName, out var value) ? value : match.Value;
        });
    }

    [GeneratedRegex(@"\{\{(.+?)\}\}")]
    private static partial Regex MergeFieldPattern();
}
