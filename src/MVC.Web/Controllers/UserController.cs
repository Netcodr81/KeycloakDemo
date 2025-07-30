using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MVC.Web.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using MVC.Web.Authentication;

namespace MVC.Web.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly ITokenService _tokenService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<UserController> _logger;

    public UserController(IHttpContextAccessor httpContextAccessor, ITokenService tokenService, IAuthorizationService authorizationService, ILogger<UserController> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _tokenService = tokenService;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    public IActionResult UserInfo()
    {
        // Initialize empty view models to prevent null reference exceptions
        ViewData["AccessTokenModel"] = new AccessTokenViewModel();
        ViewData["ClaimsPanelModel"] = new ClaimsPanelViewModel();

        // Initialize RefreshTokenViewModel with data from the current access token if available
        var refreshTokenModel = new RefreshTokenViewModel();
        var accessToken = _httpContextAccessor.HttpContext?.GetTokenAsync("access_token").Result;
        if (!string.IsNullOrEmpty(accessToken))
        {
            refreshTokenModel = RefreshTokenViewModel.FromAccessToken(accessToken);
        }
        ViewData["RefreshTokenModel"] = refreshTokenModel;

        return View();
    }

    public async Task<IActionResult> UserRoles()
    {
        var user = _httpContextAccessor.HttpContext.User;

        if (user.Identity.IsAuthenticated)
        {
            var roles =  user.Claims
                .Where(c => c.Type == "role")
                .Select(c => c.Value)
                .ToList();

            var isAdmin = (await _authorizationService.AuthorizeAsync(user, "Admin")).Succeeded;
            var isUser = (await _authorizationService.AuthorizeAsync(user, "User")).Succeeded;

            return View(new UserRoles {Roles = roles, IsInAdminPolicy = isAdmin, IsInUserPolicy = isUser});
        }

        return Unauthorized();
    }

    #region Access Token Methods

    [HttpPost]
    public async Task<IActionResult> ShowAccessToken()
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");
        return Json(new { accessToken });
    }

    [HttpPost]
    public IActionResult DecodeAccessToken(string accessToken)
    {

        if (string.IsNullOrEmpty(accessToken))
        {
            return BadRequest("Access token is required.");
        }

        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(accessToken) as JwtSecurityToken;

        using var jsonDoc = JsonDocument.Parse(jsonToken.Payload.SerializeToJson());
        var formattedJson = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });

        return Json(new { decodedToken = formattedJson });
    }

    [HttpPost]
    public IActionResult ClearAccessToken()
    {
        return Ok();
    }

    #endregion

    #region Claims Methods

    [HttpPost]
    public IActionResult ShowClaims()
    {
        var user = _httpContextAccessor.HttpContext.User;

        if (user.Identity is ClaimsIdentity identity)
        {
            var claims = identity.Claims.Select(c => new { type = c.Type, value = c.Value }).ToList();
            return Json(claims);
        }

        return Json(new List<object>());
    }

    [HttpPost]
    public IActionResult ClearClaims()
    {
        return Ok();
    }

    #endregion

    #region Refresh Token Methods

    [HttpPost]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            var success = await _tokenService.RefreshTokenAsync();
            _logger.LogInformation("Access Token refreshed successfully: {Success}", success);

            if (success)
            {
                // After successful refresh, redirect to force a reload
                TempData["SuccessMessage"] = "Token refreshed successfully";
                return RedirectToAction("UserInfo");
            }
            else
            {

                TempData["ErrorMessage"] = $"Failed to refresh token";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error refreshing token: {ex.Message}";
        }

        var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");

        ViewData["AccessTokenModel"] = new AccessTokenViewModel();
        ViewData["ClaimsPanelModel"] = new ClaimsPanelViewModel();
        ViewData["RefreshTokenModel"] = RefreshTokenViewModel.FromAccessToken(accessToken);

        return View("UserInfo");
    }

    #endregion
}