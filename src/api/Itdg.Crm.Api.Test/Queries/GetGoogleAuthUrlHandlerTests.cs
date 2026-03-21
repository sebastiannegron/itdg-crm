namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Microsoft.Extensions.Logging;

public class GetGoogleAuthUrlHandlerTests
{
    private readonly IGoogleOAuthService _oAuthService;
    private readonly ILogger<GetGoogleAuthUrlHandler> _logger;
    private readonly GetGoogleAuthUrlHandler _handler;

    public GetGoogleAuthUrlHandlerTests()
    {
        _oAuthService = Substitute.For<IGoogleOAuthService>();
        _logger = Substitute.For<ILogger<GetGoogleAuthUrlHandler>>();
        _handler = new GetGoogleAuthUrlHandler(_oAuthService, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsAuthorizationUrl()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var expectedUrl = "https://accounts.google.com/o/oauth2/v2/auth?client_id=test&scope=drive";

        _oAuthService.GetAuthorizationUrl(correlationId.ToString())
            .Returns(expectedUrl);

        // Act
        var result = await _handler.HandleAsync(new GetGoogleAuthUrl(), correlationId, CancellationToken.None);

        // Assert
        result.Should().Be(expectedUrl);
        _oAuthService.Received(1).GetAuthorizationUrl(correlationId.ToString());
    }
}
