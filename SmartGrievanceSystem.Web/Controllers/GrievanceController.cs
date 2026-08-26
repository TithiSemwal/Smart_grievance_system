using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGrievanceSystem.Core.Models;
using SmartGrievanceSystem.Infrastructure.Data;
using SmartGrievanceSystem.Web.Models;
using System.Security.Claims;
using System.Text.Json;
using System.Text;

namespace SmartGrievanceSystem.Web.Controllers
{
    [Authorize]
    public class GrievanceController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public GrievanceController(AppDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var grievances = await _context.Grievances
                .Include(g => g.Category)
                .Include(g => g.AssignedOfficer)
                .Where(g => g.SubmitterUserID == userId)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            return View(grievances);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var grievance = await _context.Grievances
                .Include(g => g.Category)
                .Include(g => g.AssignedOfficer)
                .Include(g => g.Histories.Where(h => !h.IsInternal).OrderByDescending(h => h.ChangeDate))
                .FirstOrDefaultAsync(g => g.GrievanceID == id);

            if (grievance == null) return NotFound();
            
            // Only submitter or officers/admins can see
            if (grievance.SubmitterUserID != userId && !User.IsInRole("Grievance Officer") && !User.IsInRole("Administrator"))
            {
                return Forbid();
            }

            return View(grievance);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GrievanceCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                
                var grievance = new Grievance
                {
                    GrievanceCode = $"GRV-{DateTime.UtcNow.Year}-{new Random().Next(100000, 999999)}",
                    SubmitterUserID = userId,
                    Title = model.Title,
                    Description = model.Description,
                    Status = "Submitted",
                    Priority = "Medium", // Default, AI will suggest
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Grievances.Add(grievance);
                
                var history = new GrievanceHistory
                {
                    Grievance = grievance,
                    ActionTaken = "Created",
                    ChangedByUserID = userId
                };
                _context.GrievanceHistories.Add(history);
                
                await _context.SaveChangesAsync();

                // Trigger AI Triage (fire and forget for now or await if fast)
                await TriggerAITriage(grievance);

                return RedirectToAction(nameof(Details), new { id = grievance.GrievanceID });
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Grievance Officer")]
        public async Task<IActionResult> Triage(int id, string newStatus, string resolutionNotes)
        {
            var grievance = await _context.Grievances.FindAsync(id);
            if (grievance == null) return NotFound();

            var oldStatus = grievance.Status;
            grievance.Status = newStatus;
            grievance.UpdatedAt = DateTime.UtcNow;

            if (newStatus == "Resolved" || newStatus == "Closed")
            {
                grievance.ResolvedAt = DateTime.UtcNow;
                if (newStatus == "Closed") grievance.ClosedAt = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(resolutionNotes))
                {
                    grievance.ResolutionNotes = resolutionNotes;
                }
            }

            var history = new GrievanceHistory
            {
                GrievanceID = id,
                ActionTaken = $"Status changed from {oldStatus} to {newStatus}",
                OldValue = oldStatus,
                NewValue = newStatus,
                ChangedByUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                Comments = resolutionNotes
            };

            _context.GrievanceHistories.Add(history);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = grievance.GrievanceID });
        }

        private async Task TriggerAITriage(Grievance grievance)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                
                // Call Category Prediction
                var catReq = new { title = grievance.Title, description = grievance.Description };
                var catContent = new StringContent(JsonSerializer.Serialize(catReq), Encoding.UTF8, "application/json");
                var aiBaseUrl = _configuration["AiService:BaseUrl"] ?? "http://localhost:8000";
                var catRes = await client.PostAsync($"{aiBaseUrl}/predict/category", catContent);
                
                if (catRes.IsSuccessStatusCode)
                {
                    var catData = await catRes.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(catData);
                    var root = doc.RootElement;
                    
                    var candidates = root.GetProperty("top_candidates");
                    var topCandidate = candidates[0];
                    var predictedCategoryId = topCandidate.GetProperty("category_id").GetInt32();
                    var catConfidence = topCandidate.GetProperty("confidence").GetDecimal();
                    
                    // Call Priority Prediction
                    var priReq = new { title = grievance.Title, description = grievance.Description, predicted_category_id = predictedCategoryId };
                    var priContent = new StringContent(JsonSerializer.Serialize(priReq), Encoding.UTF8, "application/json");
                    var priRes = await client.PostAsync($"{aiBaseUrl}/predict/priority", priContent);
                    var priData = await priRes.Content.ReadAsStringAsync();
                    
                    using var pdoc = JsonDocument.Parse(priData);
                    var proot = pdoc.RootElement;
                    var predictedPriority = proot.GetProperty("predicted_priority").GetString();
                    var priConfidence = proot.GetProperty("confidence").GetDecimal();

                    var aiRec = new GrievanceAIRecommendation
                    {
                        GrievanceID = grievance.GrievanceID,
                        PredictedCategoryID = predictedCategoryId,
                        PredictedPriority = predictedPriority,
                        ConfidenceScore = catConfidence,
                        PriorityConfidenceScore = priConfidence,
                        TopCandidatesJson = candidates.ToString(),
                        ModelVersion = root.GetProperty("model_version").GetString()
                    };
                    _context.GrievanceAIRecommendations.Add(aiRec);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                // In a real app, log error. Triage failure shouldn't block submission.
            }
        }
    }
}
