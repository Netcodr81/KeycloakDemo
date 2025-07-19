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

                // Get the authentication manager from OWIN context
                var authManager = context.GetOwinContext().Authentication;
                var authResult = authManager.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationType).Result;
                var user = context.GetOwinContext().Authentication.User.Identity;

                if (authResult == null || !authResult.Identity.IsAuthenticated)
                {
                    return false;
                }

                // Get refresh token from authentication properties
                var refreshToken = UserHelper.GetClaim("refresh_token");

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

                // Update the tokens in the authentication ticket
                var properties = authResult.Properties;

                // Update token values
                if (properties.Dictionary.ContainsKey(".Token.access_token"))
                    properties.Dictionary[".Token.access_token"] = tokenResponse.AccessToken;
                else
                    properties.Dictionary.Add(".Token.access_token", tokenResponse.AccessToken);

                if (properties.Dictionary.ContainsKey(".Token.refresh_token"))
                    properties.Dictionary[".Token.refresh_token"] = tokenResponse.RefreshToken;
                else
                    properties.Dictionary.Add(".Token.refresh_token", tokenResponse.RefreshToken);

                if (tokenResponse.ExpiresIn > 0)
                {
                    var expiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                    var expiresAtString = expiresAt.ToString("o", CultureInfo.InvariantCulture);

                    if (properties.Dictionary.ContainsKey(".Token.expires_at"))
                        properties.Dictionary[".Token.expires_at"] = expiresAtString;
                    else
                        properties.Dictionary.Add(".Token.expires_at", expiresAtString);
                }

                // Sign in the user with the updated tokens

                //authManager.SignIn(properties, authResult.Identity);
                HttpContext.Current.GetOwinContext().Authentication.SignIn(new AuthenticationProperties
                {
                    IsPersistent = true,
                    AllowRefresh = true,
                    ExpiresUtc = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
                }, authResult.Identity);


                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Task<string> GetAccessTokenAsync()
        {
            var context = HttpContext.Current;
            if (context == null)
            {
                return Task.FromResult<string>(null);
            }

            var authManager = context.GetOwinContext().Authentication;
            var authResult = authManager.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationType).Result;

            if (authResult == null || !authResult.Identity.IsAuthenticated)
            {
                return Task.FromResult<string>(null);
            }

            var accessToken = authResult.Properties.Dictionary.ContainsKey(".Token.access_token")
                ? authResult.Properties.Dictionary[".Token.access_token"]
                : null;

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

        [DataMember(Name = "expires_in")]
        public int ExpiresIn { get; set; }

        [DataMember(Name = "token_type")]
        public string TokenType { get; set; }
    }
}