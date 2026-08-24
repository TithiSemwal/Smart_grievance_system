using Microsoft.EntityFrameworkCore;
using SmartGrievanceSystem.Core.Models;

namespace SmartGrievanceSystem.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Grievance> Grievances { get; set; }
        public DbSet<GrievanceAIRecommendation> GrievanceAIRecommendations { get; set; }
        public DbSet<GrievanceHistory> GrievanceHistories { get; set; }
        public DbSet<SimilarGrievance> SimilarGrievances { get; set; }
        public DbSet<GrievanceAttachment> GrievanceAttachments { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleID);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentID);

            modelBuilder.Entity<Department>()
                .HasOne(d => d.EscalationOfficer)
                .WithMany()
                .HasForeignKey(d => d.EscalationOfficerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.Department)
                .WithMany(d => d.Categories)
                .HasForeignKey(c => c.DepartmentID);

            modelBuilder.Entity<Grievance>()
                .HasOne(g => g.SubmitterUser)
                .WithMany(u => u.SubmittedGrievances)
                .HasForeignKey(g => g.SubmitterUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Grievance>()
                .HasOne(g => g.AssignedOfficer)
                .WithMany(u => u.AssignedGrievances)
                .HasForeignKey(g => g.AssignedOfficerID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Grievance>()
                .HasOne(g => g.Category)
                .WithMany(c => c.Grievances)
                .HasForeignKey(g => g.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Grievance>()
                .HasOne(g => g.SubmitterDepartment)
                .WithMany()
                .HasForeignKey(g => g.SubmitterDepartmentID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Grievance>()
                .HasOne(g => g.IsDuplicateOfGrievance)
                .WithMany(g => g.Duplicates)
                .HasForeignKey(g => g.IsDuplicateOfGrievanceID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GrievanceAIRecommendation>()
                .HasOne(r => r.Grievance)
                .WithMany(g => g.AIRecommendations)
                .HasForeignKey(r => r.GrievanceID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GrievanceAIRecommendation>()
                .HasOne(r => r.PredictedCategory)
                .WithMany(c => c.AIRecommendations)
                .HasForeignKey(r => r.PredictedCategoryID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<GrievanceHistory>()
                .HasOne(h => h.Grievance)
                .WithMany(g => g.Histories)
                .HasForeignKey(h => h.GrievanceID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GrievanceHistory>()
                .HasOne(h => h.ChangedByUser)
                .WithMany(u => u.GrievanceHistories)
                .HasForeignKey(h => h.ChangedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SimilarGrievance>()
                .HasOne(s => s.PrimaryGrievance)
                .WithMany(g => g.SimilarGrievancesAsPrimary)
                .HasForeignKey(s => s.PrimaryGrievanceID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SimilarGrievance>()
                .HasOne(s => s.SimilarGrievanceRef)
                .WithMany(g => g.SimilarGrievancesAsSimilar)
                .HasForeignKey(s => s.SimilarGrievanceID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GrievanceAttachment>()
                .HasOne(a => a.Grievance)
                .WithMany(g => g.Attachments)
                .HasForeignKey(a => a.GrievanceID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GrievanceAttachment>()
                .HasOne(a => a.UploadedByUser)
                .WithMany()
                .HasForeignKey(a => a.UploadedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Grievance)
                .WithMany(g => g.Notifications)
                .HasForeignKey(n => n.GrievanceID)
                .OnDelete(DeleteBehavior.SetNull);

            // PRD 10.4 Required Indexes
            modelBuilder.Entity<Grievance>().HasIndex(g => new { g.Status, g.Priority });
            modelBuilder.Entity<Grievance>().HasIndex(g => new { g.AssignedOfficerID, g.Status });
            modelBuilder.Entity<Grievance>().HasIndex(g => new { g.SubmitterUserID, g.CreatedAt }).IsDescending(false, true);
            modelBuilder.Entity<Grievance>().HasIndex(g => g.CategoryID);
            modelBuilder.Entity<Grievance>().HasIndex(g => g.SlaDueAt).HasFilter("[Status] NOT IN ('Resolved', 'Closed')"); // Example filter for SQL Server

            modelBuilder.Entity<GrievanceHistory>().HasIndex(h => new { h.GrievanceID, h.ChangeDate });
            modelBuilder.Entity<SimilarGrievance>().HasIndex(s => new { s.PrimaryGrievanceID, s.SimilarityScore }).IsDescending(false, true);

            // Unique constraints
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Grievance>().HasIndex(g => g.GrievanceCode).IsUnique();
        }
    }
}
