using System;

namespace SmartGrievanceSystem.Core.Models
{
    public class GrievanceHistory
    {
        public int HistoryID { get; set; }
        public int GrievanceID { get; set; }
        public string ActionTaken { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public int ChangedByUserID { get; set; }
        public DateTime ChangeDate { get; set; } = DateTime.UtcNow;
        public string Comments { get; set; }
        public bool IsInternal { get; set; }

        public Grievance Grievance { get; set; }
        public User ChangedByUser { get; set; }
    }
}
