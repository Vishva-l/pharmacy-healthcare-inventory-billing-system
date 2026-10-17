using System;

namespace pharmacy_healthcare_inventory_billing_system.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public int SaleId { get; set; }

        public Sale Sale { get; set; }

        public string PaymentMethod { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public string TransactionId { get; set; }
    }
}