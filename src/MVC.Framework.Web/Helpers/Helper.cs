using System.IdentityModel.Tokens.Jwt;

namespace MVC.Framework.Web.Helpers
{
    public class Helper
    {
        public static JwtSecurityToken DecodeToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token);
            return jsonToken as JwtSecurityToken;
        }
    }
}