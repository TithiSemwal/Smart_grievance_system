using System.ComponentModel.DataAnnotations;

namespace SmartGrievanceSystem.Web.Models
{
    public class GrievanceCreateViewModel
    {
        [Required]
        [StringLength(150, MinimumLength = 5)]
        public string Title { get; set; }

        [Required]
        [StringLength(5000, MinimumLength = 20)]
        public string Description { get; set; }

        public int? SuggestedCategoryID { get; set; }
    }
}
