using System.Collections.Generic;

namespace SparePartsWeb.Models
{
    public class UserRolesViewModel
    {
        public string UserId { get; set; }=string.Empty;
        public string Email { get; set; }=string.Empty;
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }

    public class ManageUserRolesViewModel
    {
        public string UserId { get; set; }=string.Empty;
        public string Email { get; set; }=string.Empty;
        public List<RoleSelection> Roles { get; set; } = new();
    }

    public class RoleSelection
    {
        public string RoleName { get; set; } = string.Empty;
        public bool Selected { get; set; }
    }
}
