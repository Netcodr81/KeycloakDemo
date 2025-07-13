using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MVC.Web.Controllers;

public class AuthorizationController : Controller
{
    private readonly ILogger<AuthorizationController> logger;

    public AuthorizationController(ILogger<AuthorizationController> logger) => this.logger = logger;

    [AllowAnonymous]
    public IActionResult SignIn()
    {
        if (!this.User.Identity!.IsAuthenticated)
        {
            return this.Challenge(OpenIdConnectDefaults.AuthenticationScheme);
        }

        return this.RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public async Task<IActionResult> SignOutAsync()
    {
        if (!this.User.Identity!.IsAuthenticated)
        {
            return this.Challenge(OpenIdConnectDefaults.AuthenticationScheme);
        }

        var idToken = await this.HttpContext.GetTokenAsync("id_token");

        var authResult = this
            .HttpContext.Features.Get<IAuthenticateResultFeature>()
            ?.AuthenticateResult;

        var tokens = authResult!.Properties!.GetTokens();

        var tokenNames = tokens.Select(token => token.Name).ToArray();

        this.logger.LogInformation("Token Names: {TokenNames}", string.Join(", ", tokenNames));

        return this.SignOut(
            new AuthenticationProperties
            {
                RedirectUri = "/",
                Items = {{"id_token_hint", idToken}}
            },
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme
        );
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => this.RedirectToAction("AccessDenied", "Home");
}