using System.IdentityModel.Tokens.Jwt;

namespace MVC.Web.Models
{
    public class RefreshTokenViewModel
    {
        public DateTimeOffset? TokenExpiration { get; set; }
        public DateTimeOffset? TokenIssuedAt { get; set; }
        public DateTimeOffset? LastAuthenticatedAt { get; set; }

        public static RefreshTokenViewModel FromAccessToken(string accessToken)
        {
            var model = new RefreshTokenViewModel();

            if (!string.IsNullOrEmpty(accessToken))
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(accessToken);

                model.TokenExpiration = GetClaimAsDateTime(token, "exp");
                model.TokenIssuedAt = GetClaimAsDateTime(token, "iat");
                model.LastAuthenticatedAt = GetClaimAsDateTime(token, "auth_time");
            }

            return model;
        }

        private static DateTimeOffset? GetClaimAsDateTime(JwtSecurityToken token, string claimType)
        {
            // Find the claim by its type name
            var claim = token.Claims.FirstOrDefault(c => c.Type == claimType);

            if (claim is null)
            {
                return null;
            }

            // The value is a string representing epoch time. Convert it to a long.
            if (long.TryParse(claim.Value, out long epochSeconds))
            {
                // Convert the epoch seconds to a DateTimeOffset object
                return DateTimeOffset.FromUnixTimeSeconds(epochSeconds);
            }

            return null;
        }
    }
}
