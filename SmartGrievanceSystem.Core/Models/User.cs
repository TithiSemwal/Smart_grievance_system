using System;
using System.Collections.Generic;

namespace SmartGrievanceSystem.Core.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public int RoleID { get; set; }
        public int? DepartmentID { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool MustChangePassword { get; set; }

        public Role Role { get; set; }
        public Department Department { get; set; }
        public ICollection<Grievance> SubmittedGrievances { get; set; }
        public ICollection<Grievance> AssignedGrievances { get; set; }
        public ICollection<GrievanceHistory> GrievanceHistories { get; set; }
    }
}
