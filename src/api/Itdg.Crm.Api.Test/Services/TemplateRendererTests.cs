namespace Itdg.Crm.Api.Test.Services;

using Itdg.Crm.Api.Infrastructure.Services;

public class TemplateRendererTests
{
    private readonly TemplateRenderer _renderer = new();

    [Fact]
    public void Render_ReplacesAllMergeFields()
    {
        // Arrange
        var template = "Dear {{client_name}}, your payment of {{amount}} is due on {{due_date}}.";
        var mergeFields = new Dictionary<string, string>
        {
            { "client_name", "John Doe" },
            { "amount", "$500.00" },
            { "due_date", "2026-04-01" }
        };

        // Act
        var result = _renderer.Render(template, mergeFields);

        // Assert
        result.Should().Be("Dear John Doe, your payment of $500.00 is due on 2026-04-01.");
    }

    [Fact]
    public void Render_LeavesUnmatchedFieldsIntact()
    {
        // Arrange
        var template = "Hello {{client_name}}, your account {{account_id}} is active.";
        var mergeFields = new Dictionary<string, string>
        {
            { "client_name", "Jane Smith" }
        };

        // Act
        var result = _renderer.Render(template, mergeFields);

        // Assert
        result.Should().Be("Hello Jane Smith, your account {{account_id}} is active.");
    }

    [Fact]
    public void Render_HandlesEmptyMergeFields()
    {
        // Arrange
        var template = "Hello {{client_name}}, welcome!";
        var mergeFields = new Dictionary<string, string>();

        // Act
        var result = _renderer.Render(template, mergeFields);

        // Assert
        result.Should().Be("Hello {{client_name}}, welcome!");
    }

    [Fact]
    public void Render_HandlesTemplateWithNoMergeFields()
    {
        // Arrange
        var template = "This is a plain text template with no merge fields.";
        var mergeFields = new Dictionary<string, string>
        {
            { "client_name", "John Doe" }
        };

        // Act
        var result = _renderer.Render(template, mergeFields);

        // Assert
        result.Should().Be("This is a plain text template with no merge fields.");
    }

    [Fact]
    public void Render_HandlesDottedFieldNames()
    {
        // Arrange
        var template = "Dear {{client.name}}, your tier is {{client.tier}}.";
        var mergeFields = new Dictionary<string, string>
        {
            { "client.name", "María García" },
            { "client.tier", "Gold" }
        };

        // Act
        var result = _renderer.Render(template, mergeFields);

        // Assert
        result.Should().Be("Dear María García, your tier is Gold.");
    }

    [Fact]
    public void Render_HandlesFieldsWithSpacesAroundName()
    {
        // Arrange
        var template = "Hello {{ client_name }}, welcome!";
        var mergeFields = new Dictionary<string, string>
        {
            { "client_name", "John Doe" }
        };

        // Act
        var result = _renderer.Render(template, mergeFields);

        // Assert
        result.Should().Be("Hello John Doe, welcome!");
    }

    [Fact]
    public void Render_HandlesSpanishTemplate()
    {
        // Arrange
        var template = "Estimado/a {{client_name}}, su pago de {{amount}} vence el {{due_date}}.";
        var mergeFields = new Dictionary<string, string>
        {
            { "client_name", "Carlos López" },
            { "amount", "$1,200.00" },
            { "due_date", "1 de abril de 2026" }
        };

        // Act
        var result = _renderer.Render(template, mergeFields);

        // Assert
        result.Should().Be("Estimado/a Carlos López, su pago de $1,200.00 vence el 1 de abril de 2026.");
    }

    [Fact]
    public void Render_HandlesMultipleOccurrencesOfSameField()
    {
        // Arrange
        var template = "{{client_name}} - Dear {{client_name}}, thank you {{client_name}}.";
        var mergeFields = new Dictionary<string, string>
        {
            { "client_name", "John Doe" }
        };

        // Act
        var result = _renderer.Render(template, mergeFields);

        // Assert
        result.Should().Be("John Doe - Dear John Doe, thank you John Doe.");
    }

    [Fact]
    public void Render_HandlesEmptyStringValues()
    {
        // Arrange
        var template = "Hello {{client_name}}, notes: {{notes}}.";
        var mergeFields = new Dictionary<string, string>
        {
            { "client_name", "John" },
            { "notes", "" }
        };

        // Act
        var result = _renderer.Render(template, mergeFields);

        // Assert
        result.Should().Be("Hello John, notes: .");
    }
}
