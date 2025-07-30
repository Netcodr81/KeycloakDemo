using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.EnterpriseServices;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.Extensions.Logging;

namespace MVC.Framework.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            MvcApplication.Logger?.LogInformation("Home/Index action executed");

            using(var activity = MvcApplication.ActivitySource.StartActivity("HomeController.Index"))
            {
                if (activity != null)
                {
                    activity.SetTag("operation", "home-index");
                    activity.SetStatus(ActivityStatusCode.Ok);
                }
            }

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

    }
}