using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace Blazor.Web.Authentication;

internal static class LoginLogoutEndpointRouteBuilderExtensions
{
    internal static IEndpointConventionBuilder MapLoginAndLogout(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("");

        group.MapGet("/login", (string? returnUrl) => TypedResults.Challenge(GetAuthProperties(returnUrl)))
            .AllowAnonymous();

        group.MapPost("/logout", ([FromForm] string? returnUrl) => TypedResults.SignOut(GetAuthProperties(returnUrl),
            [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]));

        // This endpoint will now be reachable for your manual refresh button.
        group.MapPost("/api/refresh-token", async (ITokenService tokenService) =>
            {
                try
                {
                    var success = await tokenService.RefreshTokenAsync();
                    if (success)
                    {
                        return Results.Ok(new {success = true, message = "Token refreshed successfully via manual endpoint."});
                    }

                    return Results.BadRequest(new {success = false, message = "Failed to refresh token via manual endpoint."});
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new {success = false, message = $"An error occurred: {ex.Message}"});
                }
            }).RequireAuthorization()
            .DisableAntiforgery();


        return group;
    }

    private static AuthenticationProperties GetAuthProperties(string? returnUrl)
    {
        const string pathBase = "/";

        if (string.IsNullOrEmpty(returnUrl))
        {
            returnUrl = pathBase;
        }
        else if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
        {
            returnUrl = new Uri(returnUrl, UriKind.Absolute).PathAndQuery;
        }
        else if (returnUrl[0] != '/')
        {
            returnUrl = $"{pathBase}{returnUrl}";
        }

        return new AuthenticationProperties {RedirectUri = returnUrl};
    }
}