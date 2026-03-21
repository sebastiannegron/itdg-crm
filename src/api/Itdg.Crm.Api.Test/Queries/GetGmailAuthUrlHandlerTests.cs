namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Microsoft.Extensions.Logging;

public class GetGmailAuthUrlHandlerTests
{
    private readonly IGoogleOAuthService _oAuthService;
    private readonly ILogger<GetGmailAuthUrlHandler> _logger;
    private readonly GetGmailAuthUrlHandler _handler;

    public GetGmailAuthUrlHandlerTests()
    {
        _oAuthService = Substitute.For<IGoogleOAuthService>();
        _logger = Substitute.For<ILogger<GetGmailAuthUrlHandler>>();
        _handler = new GetGmailAuthUrlHandler(_oAuthService, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsAuthorizationUrl()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        _oAuthService.GetAuthorizationUrl(correlationId.ToString())
            .Returns("https://accounts.google.com/o/oauth2/auth?state=" + correlationId);

        // Act
        var result = await _handler.HandleAsync(new GetGmailAuthUrl(), correlationId, CancellationToken.None);

        // Assert
        result.Should().Contain("accounts.google.com");
        _oAuthService.Received(1).GetAuthorizationUrl(correlationId.ToString());
    }
}
