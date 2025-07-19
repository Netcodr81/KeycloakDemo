using System;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MVC.Framework.Web.Authentication;
using MVC.Framework.Web.Helpers;
using MVC.Framework.Web.Models;

namespace MVC.Framework.Web.Controllers
{
    [Authorize]
    public class UserController : Controller
    {

    private readonly ITokenService _tokenService;

        public UserController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        // GET
        public ActionResult UserInfo()
        {
            ViewData["AccessTokenModel"] = new AccessTokenViewModel();
            ViewData["ClaimsPanelModel"] = new ClaimsPanelViewModel();

            // Initialize RefreshTokenViewModel with data from the current access token if available
            var refreshTokenModel = new RefreshTokenViewModel();
            var accessToken = UserHelper.GetClaim("access_token");
            if (!string.IsNullOrEmpty(accessToken))
            {
                refreshTokenModel = RefreshTokenViewModel.FromAccessToken(accessToken);
            }

            ViewData["RefreshTokenModel"] = refreshTokenModel;

            return View();
        }



    [HttpPost]
    public async Task<ActionResult> ShowAccessToken()
    {
        var accessToken = UserHelper.GetClaim("access_token");
        return Json(new { accessToken });
    }

    [HttpPost]
    public ActionResult DecodeAccessToken(string accessToken)
    {

        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(accessToken) as JwtSecurityToken;

        var jsonDoc = JsonDocument.Parse(jsonToken.Payload.SerializeToJson());
        var formattedJson = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });

        return Json(new { decodedToken = formattedJson });
    }

    [HttpPost]
    public ActionResult ClearAccessToken()
    {
        return Json("");
    }



    [HttpPost]
    public ActionResult ShowClaims()
    {

        var user = User as ClaimsPrincipal;
        var claims = user?.Claims.Select(c => new { type = c.Type, value = c.Value }).ToList();
        var viewModelClaims = user.Claims.ToList();
        return PartialView("Shared/_UserClaimsPanel", new ClaimsPanelViewModel {Claims =viewModelClaims});
    }

    [HttpPost]
    public ActionResult ClearClaims()
    {
        return PartialView("Shared/_UserClaimsPanel", new ClaimsPanelViewModel());
    }



    [HttpPost]
    public async Task<ActionResult> RefreshToken()
    {
        try
        {
            var success = await _tokenService.RefreshTokenAsync();

            if (success)
            {
                // After successful refresh, redirect to force a reload
                TempData["SuccessMessage"] = "Token refreshed successfully";

                var user = HttpContext.GetOwinContext().Authentication.User.Identity;
                var accessToken = UserHelper.GetClaim("access_token");


                return PartialView("Shared/_RefreshTokenPanel", RefreshTokenViewModel.FromAccessToken(accessToken));
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

        var oldAccessToken = UserHelper.GetClaim("access_token");
        return PartialView("Shared/_RefreshTokenPanel", RefreshTokenViewModel.FromAccessToken(oldAccessToken));
    }


    }
}