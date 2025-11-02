using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SparePartsWeb.Models
{
    public class Vendor
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string ContactEmail { get; set; } = string.Empty;

        [MaxLength(15)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public int AverageLeadTimeDays { get; set; }

        [Range(0, double.MaxValue)]
        public int ReliabilityScore { get; set; } // e.g. 0–100

        public ICollection<SparePart>? SpareParts { get; set; }
    }
}
