using System.Threading.Tasks;

namespace MVC.Framework.Web.Authentication
{
    public interface ITokenService
    {
        Task<bool> RefreshTokenAsync();
        Task<string> GetAccessTokenAsync();
    }
}