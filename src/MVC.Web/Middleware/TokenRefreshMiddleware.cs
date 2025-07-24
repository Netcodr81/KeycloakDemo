using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using MVC.Web.Authentication;

namespace MVC.Web.Middleware;

public class TokenRefreshMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenRefreshMiddleware> _logger;

    // Remove ITokenService from the constructor
    public TokenRefreshMiddleware(RequestDelegate next, ILogger<TokenRefreshMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // Add ITokenService as a parameter to InvokeAsync
    public async Task InvokeAsync(HttpContext context, ITokenService tokenService)
    {
        if (context.User.Identity?.IsAuthenticated ?? false)
        {
            var expiresAtString = await context.GetTokenAsync("expires_at");
            if (string.IsNullOrEmpty(expiresAtString))
            {
                await _next(context);
                return;
            }

            var expiresAt = DateTimeOffset.Parse(expiresAtString);
            var timeRemaining = expiresAt.Subtract(DateTimeOffset.UtcNow);

            if (timeRemaining < TimeSpan.FromSeconds(60))
            {
                _logger.LogInformation("Access token is expiring, attempting to refresh.");
                var refreshToken = await context.GetTokenAsync("refresh_token");

                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogWarning("Refresh token not found. Signing out user.");
                    await SignOutAndRedirect(context);
                    return;
                }

                // Pass the resolved tokenService to the method
                var success = await TryRefreshToken(tokenService);
                if (!success)
                {
                    _logger.LogWarning("Failed to refresh token. Signing out user.");
                    await SignOutAndRedirect(context);
                    return;
                }
            }
        }

        await _next(context);
    }

    private async Task<bool> TryRefreshToken(ITokenService tokenService)
    {
        try
        {
            var success = await tokenService.RefreshTokenAsync();

            if (success)
            {
                _logger.LogInformation("Token successfully refreshed by TokenService.");
                return true;
            }

            _logger.LogWarning("TokenService failed to refresh token.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while trying to refresh the token.");
            return false;
        }
    }

    private async Task SignOutAndRedirect(HttpContext context)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        context.Response.Redirect("/Home/Index");
    }
}