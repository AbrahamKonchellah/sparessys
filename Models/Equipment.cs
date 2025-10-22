using System;
using System.ComponentModel.DataAnnotations;

namespace SparePartsWeb.Models
{
    public class Equipment
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Model { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }  

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal BookValue { get; set; }

        [DataType(DataType.Date)]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        public DateTime? LastMaintenanceDate { get; set; }
    }
}
