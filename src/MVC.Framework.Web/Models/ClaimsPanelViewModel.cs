using System.Collections.Generic;
using System.Security.Claims;

namespace MVC.Framework.Web.Models
{
    public class ClaimsPanelViewModel
    {
        public List<Claim> Claims { get; set; } = new List<Claim>();
    }
}
