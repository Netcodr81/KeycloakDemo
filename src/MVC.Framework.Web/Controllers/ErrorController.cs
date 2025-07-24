using System.Web.Mvc;

namespace MVC.Framework.Web.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult AuthenticationFailed(string message)
        {
            ViewBag.ErrorMessage = message;
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}