using System.ComponentModel.DataAnnotations;

namespace pharmacy_healthcare_inventory_billing_system.ViewModels
{
    public class CheckoutViewModel
    {
        [Required, StringLength(100)]
        public string CustomerName { get; set; }

        [Required]
        public string PaymentMethod { get; set; }
    }
}
