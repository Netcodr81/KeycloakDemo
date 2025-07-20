using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin;
using MVC.Framework.Web.Authentication;

namespace MVC.Framework.Web.Middleware
{
    public class TokenRefreshMiddleware : OwinMiddleware
    {
        private readonly ITokenService _tokenService;
        private readonly TimeSpan _refreshThreshold = TimeSpan.FromMinutes(2);

        public TokenRefreshMiddleware(OwinMiddleware next, ITokenService tokenService) : base(next)
        {
            _tokenService = tokenService;
        }

        public override async Task Invoke(IOwinContext context)
        {
            var user = context.Authentication.User as ClaimsPrincipal;
            var accessTokenClaim = user?.Claims.FirstOrDefault(c => c.Type == "access_token");
            if (!string.IsNullOrEmpty(accessTokenClaim?.Value))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(accessTokenClaim.Value);
                var expClaim = jwt.Claims.FirstOrDefault(c => c.Type == "exp");

                if (expClaim != null && long.TryParse(expClaim.Value, out var expUnix))
                {
                    var expDate = DateTimeOffset.FromUnixTimeSeconds(expUnix);
                    var now = DateTimeOffset.UtcNow;

                    if (expDate - now < _refreshThreshold)
                    {
                        // Token is about to expire, refresh it
                        var refreshed = await _tokenService.RefreshTokenAsync();
                    }
                }
            }

            await Next.Invoke(context);
        }
    }
}