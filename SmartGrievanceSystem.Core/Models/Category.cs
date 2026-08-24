using System;
using System.Collections.Generic;

namespace SmartGrievanceSystem.Core.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int DepartmentID { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;

        public Department Department { get; set; }
        public ICollection<Grievance> Grievances { get; set; }
        public ICollection<GrievanceAIRecommendation> AIRecommendations { get; set; }
    }
}
