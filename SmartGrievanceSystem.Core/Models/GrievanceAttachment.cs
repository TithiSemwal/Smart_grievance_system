using System;

namespace SmartGrievanceSystem.Core.Models
{
    public class GrievanceAttachment
    {
        public int AttachmentID { get; set; }
        public int GrievanceID { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long FileSizeBytes { get; set; }
        public int UploadedByUserID { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public Grievance Grievance { get; set; }
        public User UploadedByUser { get; set; }
    }
}
