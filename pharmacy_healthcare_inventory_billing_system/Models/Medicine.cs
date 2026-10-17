using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace pharmacy_healthcare_inventory_billing_system.Models
{
    public class Medicine
    {
        public int MedicineId { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        [StringLength(100)]
        public string GenericName { get; set; }

        [StringLength(100)]
        public string Manufacturer { get; set; }

        [StringLength(50)]
        public string BatchNumber { get; set; }

        [DataType(DataType.Date)]
        [Required]
        public DateTime ExpiryDate { get; set; }

        [Range(0, 100000)]
        public int StockQuantity { get; set; }

        [Range(0, 100000)]
        public int ReorderLevel { get; set; }

        [Range(0, 100000)]
        public decimal PurchasePrice { get; set; }

        [Range(0, 100000)]
        public decimal SellingPrice { get; set; }

        [StringLength(50)]
        public string Category { get; set; }

        public bool RequiresPrescription { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}
