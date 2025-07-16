using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace MVC.Framework.Web.Models
{
    public class AccessTokenViewModel
    {
        public string AccessToken { get; set; } = string.Empty;
        public string DecodedToken { get; set; } = string.Empty;

        public void DecodeAccessToken()
        {
            try
            {
                if (string.IsNullOrEmpty(AccessToken))
                    return;

                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(AccessToken);

                var payload = token.Payload;

                // Format payload as indented JSON
                var decodedToken = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                DecodedToken = decodedToken;
            }
            catch (Exception ex)
            {
                DecodedToken = $"Error decoding token: {ex.Message}";
            }
        }
    }
}
