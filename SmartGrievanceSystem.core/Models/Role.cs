using System;
using System.Collections.Generic;

namespace SmartGrievanceSystem.Core.Models
{
    public class Role
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }

        public ICollection<User> Users { get; set; }
    }
}
