using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;

namespace Blazor.Web.Authentication;

public class KeycloakTokenService : ITokenService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IOptionsMonitor<OpenIdConnectOptions> _oidcOptionsMonitor;

    public KeycloakTokenService(
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IOptionsMonitor<OpenIdConnectOptions> oidcOptionsMonitor)
    {
        _httpContextAccessor = httpContextAccessor;
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
        _oidcOptionsMonitor = oidcOptionsMonitor;
    }

    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await _httpContextAccessor.HttpContext!.GetTokenAsync("refresh_token");
            if (string.IsNullOrEmpty(refreshToken))
            {
                return false;
            }

            var oidcOptions = _oidcOptionsMonitor.Get(OpenIdConnectDefaults.AuthenticationScheme);
            var configuration = await oidcOptions.ConfigurationManager!.GetConfigurationAsync(default);
            var tokenEndpoint = configuration.TokenEndpoint;

            // Prepare the token refresh request
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = _configuration["Keycloak:resource"],
                ["client_secret"] = _configuration["Keycloak:credentials:secret"]
            });

            var response = await _httpClient.PostAsync(tokenEndpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (tokenResponse == null)
            {
                return false;
            }

            // Update the tokens in the authentication ticket
            var authResult = await _httpContextAccessor.HttpContext!.AuthenticateAsync();
            if (authResult.Succeeded)
            {
                var properties = new AuthenticationProperties(authResult.Properties.Items);
                properties.UpdateTokenValue("access_token", tokenResponse.AccessToken);
                properties.UpdateTokenValue("refresh_token", tokenResponse.RefreshToken);

                if (tokenResponse.ExpiresIn > 0)
                {
                    var expiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                    properties.UpdateTokenValue("expires_at",
                        expiresAt.ToString("o", CultureInfo.InvariantCulture));
                }

                await _httpContextAccessor.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    authResult.Principal,
                    properties);

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<string> GetAccessTokenAsync()
    {
        return await _httpContextAccessor.HttpContext?.GetTokenAsync("access_token");
    }
}

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }
}