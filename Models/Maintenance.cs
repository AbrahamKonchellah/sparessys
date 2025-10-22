using System;
using System.ComponentModel.DataAnnotations;

namespace SparePartsWeb.Models
{
    public class Maintenance
    {
        public int Id { get; set; }

        [Required]
        public int EquipmentId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ScheduledDate { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Cost { get; set; }

        // Optional: navigation property
        public Equipment? Equipment { get; set; }
    }
}
