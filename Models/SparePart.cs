using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SparePartsWeb.Models
{
    public class SparePart
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Spare part name is required.")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Brand is required.")]
        [MaxLength(200)]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;   // ✅ Only one Category (string)

        [Required(ErrorMessage = "Price is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be 0 or more.")]
        public int Quantity { get; set; } = 0;

        // 🔗 Relationship: Each SparePart belongs to one Vendor
        [Display(Name = "Vendor")]
        [ForeignKey(nameof(Vendor))]
        public int? VendorId { get; set; }

        public Vendor? Vendor { get; set; }  // Navigation property
    }
}
