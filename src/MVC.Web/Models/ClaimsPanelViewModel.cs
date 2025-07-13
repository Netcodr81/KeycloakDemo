using System.Security.Claims;

namespace MVC.Web.Models
{
    public class ClaimsPanelViewModel
    {
        public List<Claim> Claims { get; set; } = new List<Claim>();
    }
}
