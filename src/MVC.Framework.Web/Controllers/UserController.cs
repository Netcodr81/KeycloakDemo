using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;

namespace MVC.Framework.Web.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        // GET
        public ActionResult UserInfo()
        {
            var identity = User.Identity as ClaimsIdentity;

            if (identity == null)
            {
                return new HttpUnauthorizedResult();
            }

            // Get all claims for the current user
            var claims = identity.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();

            ViewBag.Claims = claims;
            ViewBag.Name = identity.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
            ViewBag.Email = identity.FindFirst(ClaimTypes.Email)?.Value;
            ViewBag.Roles = identity.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            return View();
        }
    }
}