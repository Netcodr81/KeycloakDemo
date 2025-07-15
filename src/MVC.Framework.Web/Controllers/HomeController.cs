using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace MVC.Framework.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Claims : ";
            var principle = User as ClaimsPrincipal;
            return View(principle);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        // This is temporary for debugging purposes
        public ActionResult SignInOidc()
        {
            // This is just for diagnosing the 404 issue
            // If this action gets hit, it means ASP.NET MVC is handling the route
            // instead of OWIN middleware
            return Content("The signin-oidc route is being handled by MVC controller, not OWIN middleware");
        }
    }
}