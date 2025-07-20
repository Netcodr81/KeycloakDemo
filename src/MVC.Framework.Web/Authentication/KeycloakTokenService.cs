using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using MVC.Framework.Web.Helpers;
using Newtonsoft.Json;

namespace MVC.Framework.Web.Authentication
{
    public class KeycloakTokenService : ITokenService
    {
        private readonly HttpClient _httpClient;

        public KeycloakTokenService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> RefreshTokenAsync()
        {
            try
            {
                // Get the current HttpContext
                var context = HttpContext.Current;
                if (context == null)
                {
                    return false;
                }

                // Get refresh token from authentication properties
                var refreshToken = UserHelper.GetClaim("refresh_token");
                var currentAccessToken = UserHelper.GetClaim("access_token");
                var idToken = UserHelper.GetClaim("id_token");

                if (string.IsNullOrEmpty(refreshToken))
                {
                    return false;
                }

                // Get token endpoint from configuration
                var tokenEndpoint = ConfigurationManager.AppSettings["Keycloak:TokenEndpoint"];


                // Prepare the token refresh request
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = refreshToken,
                    ["client_id"] = ConfigurationManager.AppSettings["Keycloak:ClientId"],
                    ["client_secret"] = ConfigurationManager.AppSettings["Keycloak:ClientSecret"]
                });

                var response = await _httpClient.PostAsync(tokenEndpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                // Read token response
                var responseJson = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseJson);
                if (tokenResponse == null)
                {
                    return false;
                }


                var claimsIdentity = HttpContext.Current.User.Identity as ClaimsIdentity;
                var refreshTokenClaim = claimsIdentity.FindFirst("refresh_token");
                var accessTokenClaim = claimsIdentity.FindFirst("access_token");
                var idTokenClaim = claimsIdentity.FindFirst("id_token");
                var accessTokenExpirationClaim =  claimsIdentity.FindFirst("exp");

                claimsIdentity.RemoveClaim(refreshTokenClaim);
                claimsIdentity.AddClaim(new Claim("refresh_token", tokenResponse.RefreshToken));

                claimsIdentity.RemoveClaim(accessTokenClaim);
                claimsIdentity.AddClaim(new Claim("access_token", tokenResponse.AccessToken));

                claimsIdentity.RemoveClaim(idTokenClaim);
                claimsIdentity.AddClaim(new Claim("id_token", tokenResponse.IdToken));

                claimsIdentity.RemoveClaim(accessTokenExpirationClaim);
                var newAccessTokenExpirationDate = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn);
                claimsIdentity.AddClaim(new Claim("exp", newAccessTokenExpirationDate.ToString("o")));


                // Sign in the user with the updated tokens

                HttpContext.Current.GetOwinContext().Authentication.SignIn(new AuthenticationProperties
                {
                    IsPersistent = true,
                    AllowRefresh = true,
                    ExpiresUtc = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
                }, claimsIdentity);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Task<string> GetAccessTokenAsync()
        {
            var accessToken = UserHelper.GetClaim("access_token");

            return Task.FromResult(accessToken);
        }


    }

    [DataContract]
    public class TokenResponse
    {
        [DataMember(Name = "access_token")]
        public string AccessToken { get; set; }

        [DataMember(Name = "refresh_token")]
        public string RefreshToken { get; set; }

        [DataMember(Name = "id_token")]
        public string IdToken { get; set; }

        [DataMember(Name = "expires_in")]
        public int ExpiresIn { get; set; }

        [DataMember(Name = "token_type")]
        public string TokenType { get; set; }
    }
}