namespace Itdg.Crm.Api.Application.Abstractions;

public interface ITemplateRenderer
{
    string Render(string template, IDictionary<string, string> mergeFields);
}
