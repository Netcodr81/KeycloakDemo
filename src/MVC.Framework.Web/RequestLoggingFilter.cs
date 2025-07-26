using System.Web.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MVC.Framework.Web
{
    public class RequestLoggingFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var loggerFactory = (ILoggerFactory)filterContext.HttpContext.Application["LoggerFactory"];
            var logger = loggerFactory?.CreateLogger("RequestLoggingFilter");
            var route = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName + "/" + filterContext.ActionDescriptor.ActionName;
            var parameters = filterContext.ActionParameters;
            var query = filterContext.HttpContext.Request.QueryString.ToString();
            var method = filterContext.HttpContext.Request.HttpMethod;
            var body = "";
            if (method == "POST" || method == "PUT")
            {
                filterContext.HttpContext.Request.InputStream.Position = 0;
                using (var reader = new System.IO.StreamReader(filterContext.HttpContext.Request.InputStream))
                {
                    body = reader.ReadToEnd();
                }
                filterContext.HttpContext.Request.InputStream.Position = 0;
            }
            logger?.LogInformation("Route: {Route}, Method: {Method}, Query: {Query}, Parameters: {Parameters}, Body: {Body}",
                route, method, query, JsonSerializer.Serialize(parameters), body);
            base.OnActionExecuting(filterContext);
        }
    }
}

