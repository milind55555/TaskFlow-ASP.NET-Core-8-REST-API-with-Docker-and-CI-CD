using Microsoft.Extensions.Configuration;
using TaskFlow.Api.Dtos;
using TaskFlow.Api.Services;

namespace TaskFlow.Tests;

public class AuthServiceTests
{
    private static IConfiguration Config() =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "unit-test-key-that-is-at-least-32-characters-long!",
            ["Jwt:Issuer"] = "TaskFlow",
            ["Jwt:Audience"] = "TaskFlowClients",
            ["Jwt:ExpiryMinutes"] = "5"
        }).Build();

    [Fact]
    public async Task Register_then_login_returns_token()
    {
        var svc = new AuthService(TestDb.Create(), Config());

        var reg = await svc.RegisterAsync(new RegisterRequest("a@b.com", "password123"));
        var login = await svc.LoginAsync(new LoginRequest("a@b.com", "password123"));

        Assert.NotNull(reg);
        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login!.Token));
    }

    [Fact]
    public async Task Duplicate_email_is_rejected()
    {
        var svc = new AuthService(TestDb.Create(), Config());
        await svc.RegisterAsync(new RegisterRequest("a@b.com", "password123"));

        Assert.Null(await svc.RegisterAsync(new RegisterRequest("A@B.com", "password123")));
    }

    [Fact]
    public async Task Wrong_password_fails_login()
    {
        var svc = new AuthService(TestDb.Create(), Config());
        await svc.RegisterAsync(new RegisterRequest("a@b.com", "password123"));

        Assert.Null(await svc.LoginAsync(new LoginRequest("a@b.com", "nope-nope")));
    }
}
