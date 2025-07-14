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

        // Pass HttpContext to the handler to build the absolute URL
        group.MapGet("/login", (string? returnUrl, HttpContext context) => TypedResults.Challenge(GetAuthProperties(context, returnUrl)))
            .AllowAnonymous();

        // Pass HttpContext for the logout redirect as well
        group.MapPost("/logout", ([FromForm] string? returnUrl, HttpContext context) => TypedResults.SignOut(GetAuthProperties(context, returnUrl),
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

    // Updated GetAuthProperties to accept HttpContext and build an absolute RedirectUri
    private static AuthenticationProperties GetAuthProperties(HttpContext context, string? returnUrl)
    {
        const string pathBase = "/";
        var redirectPath = pathBase;

        if (!string.IsNullOrEmpty(returnUrl))
        {
            // Sanitize the returnUrl to ensure it's a relative path within the app
            redirectPath = Uri.IsWellFormedUriString(returnUrl, UriKind.Absolute)
                ? new Uri(returnUrl, UriKind.Absolute).PathAndQuery
                : returnUrl;
        }

        if (redirectPath[0] != '/')
        {
            redirectPath = $"{pathBase}{redirectPath}";
        }

        // Construct the absolute URI for the OIDC provider
        var absoluteUrl = $"{context.Request.Scheme}://{context.Request.Host}{redirectPath}";
        return new AuthenticationProperties { RedirectUri = absoluteUrl };
    }
}