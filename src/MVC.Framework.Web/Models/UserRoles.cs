using System.Collections.Generic;

namespace MVC.Framework.Web.Models
{
    public class UserRoles
    {
        public List<string> Roles { get; set; } = new List<string>();
        public bool IsInAdminPolicy { get; set; }
        public bool IsInUserPolicy { get; set; }
    }
}