using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace pharmacy_healthcare_inventory_billing_system.ViewModels
{
    public class PrescriptionUploadViewModel
    {
        [Required]
        public string PatientName { get; set; }

        public string DoctorName { get; set; }

        [Required]
        public IFormFile PrescriptionFile { get; set; }

        public string MedicineNames { get; set; }

        public int Quantity { get; set; } = 1;
    }
}
