using System;
using System.Configuration;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using MVC.Framework.Web.Authentication;
using MVC.Framework.Web.Middleware;
using Ninject;
using Owin;

[assembly: OwinStartup(typeof(MVC.Framework.Web.Startup))]

namespace MVC.Framework.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            app.Use(typeof(TokenRefreshMiddleware), new KeycloakTokenService());
        }
    }

}