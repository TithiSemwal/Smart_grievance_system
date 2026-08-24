using System;
using System.Collections.Generic;

namespace SmartGrievanceSystem.Core.Models
{
    public class Grievance
    {
        public int GrievanceID { get; set; }
        public string GrievanceCode { get; set; } // GRV-YYYY-NNNNNN
        public int SubmitterUserID { get; set; }
        public int? SubmitterDepartmentID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int? CategoryID { get; set; }
        public string Priority { get; set; }
        public int? AssignedOfficerID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime? SlaDueAt { get; set; }
        public string ResolutionNotes { get; set; }
        public int? IsDuplicateOfGrievanceID { get; set; }
        public int ReopenCount { get; set; }

        public User SubmitterUser { get; set; }
        public Department SubmitterDepartment { get; set; }
        public Category Category { get; set; }
        public User AssignedOfficer { get; set; }
        public Grievance IsDuplicateOfGrievance { get; set; }
        
        public ICollection<Grievance> Duplicates { get; set; }
        public ICollection<GrievanceHistory> Histories { get; set; }
        public ICollection<GrievanceAIRecommendation> AIRecommendations { get; set; }
        public ICollection<SimilarGrievance> SimilarGrievancesAsPrimary { get; set; }
        public ICollection<SimilarGrievance> SimilarGrievancesAsSimilar { get; set; }
        public ICollection<GrievanceAttachment> Attachments { get; set; }
        public ICollection<Notification> Notifications { get; set; }
    }
}
