namespace Blazor.Web.Authentication;

public interface ITokenService
{
    Task<bool> RefreshTokenAsync();
    Task<string> GetAccessTokenAsync();
}