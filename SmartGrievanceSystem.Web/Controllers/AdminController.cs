using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGrievanceSystem.Infrastructure.Data;

namespace SmartGrievanceSystem.Web.Controllers
{
    [Authorize(Roles = "Administrator,Grievance Officer")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var openCount = await _context.Grievances.CountAsync(g => g.Status != "Closed" && g.Status != "Resolved");
            var resolvedCount = await _context.Grievances.CountAsync(g => g.Status == "Resolved" || g.Status == "Closed");
            var criticalCount = await _context.Grievances.CountAsync(g => g.Priority == "Critical" && g.Status != "Closed");

            ViewBag.OpenCount = openCount;
            ViewBag.ResolvedCount = resolvedCount;
            ViewBag.CriticalCount = criticalCount;

            return View();
        }

        public async Task<IActionResult> ManageGrievances()
        {
            var grievances = await _context.Grievances
                .Include(g => g.Category)
                .Include(g => g.SubmitterUser)
                .Include(g => g.AssignedOfficer)
                .Include(g => g.AIRecommendations)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            return View(grievances);
        }
    }
}
