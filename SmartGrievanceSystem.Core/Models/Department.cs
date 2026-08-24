using System;
using System.Collections.Generic;

namespace SmartGrievanceSystem.Core.Models
{
    public class Department
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string Description { get; set; }
        public int? EscalationOfficerID { get; set; }
        public bool IsActive { get; set; } = true;

        public User EscalationOfficer { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<Category> Categories { get; set; }
    }
}
