using System;

namespace SmartGrievanceSystem.Core.Models
{
    public class SimilarGrievance
    {
        public int SimilarityID { get; set; }
        public int PrimaryGrievanceID { get; set; }
        public int SimilarGrievanceID { get; set; }
        public decimal SimilarityScore { get; set; }
        public string OfficerAction { get; set; } // Confirmed / Linked / Dismissed / Pending
        public DateTime IdentifiedDate { get; set; } = DateTime.UtcNow;

        public Grievance PrimaryGrievance { get; set; }
        public Grievance SimilarGrievanceRef { get; set; }
    }
}
