using System;

namespace SmartGrievanceSystem.Core.Models
{
    public class GrievanceAIRecommendation
    {
        public int RecommendationID { get; set; }
        public int GrievanceID { get; set; }
        public int? PredictedCategoryID { get; set; }
        public string PredictedPriority { get; set; }
        public decimal ConfidenceScore { get; set; }
        public decimal PriorityConfidenceScore { get; set; }
        public string TopCandidatesJson { get; set; }
        public string ModelVersion { get; set; }
        public bool? WasCategoryAccepted { get; set; }
        public bool? WasPriorityAccepted { get; set; }
        public DateTime RecommendationDate { get; set; } = DateTime.UtcNow;

        public Grievance Grievance { get; set; }
        public Category PredictedCategory { get; set; }
    }
}
