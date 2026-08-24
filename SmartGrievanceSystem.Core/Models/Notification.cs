using System;

namespace SmartGrievanceSystem.Core.Models
{
    public class Notification
    {
        public int NotificationID { get; set; }
        public int UserID { get; set; }
        public int? GrievanceID { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; }
        public Grievance Grievance { get; set; }
    }
}
