using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace pharmacy_healthcare_inventory_billing_system.Models
{
    public class Prescription
    {
        public int PrescriptionId { get; set; }

        [Required]
        [StringLength(100)]
        public string PatientName { get; set; }

        [StringLength(100)]
        public string DoctorName { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public DateTime UploadedAt { get; set; }

        public ICollection<PrescriptionItem> Items { get; set; }
            = new List<PrescriptionItem>();
    }
}
