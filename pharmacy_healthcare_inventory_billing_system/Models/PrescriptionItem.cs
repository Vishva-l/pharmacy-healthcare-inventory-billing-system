namespace pharmacy_healthcare_inventory_billing_system.Models
{
    public class PrescriptionItem
    {
        public int PrescriptionItemId { get; set; }

        public int PrescriptionId { get; set; }

        public Prescription Prescription { get; set; }

        public int MedicineId { get; set; }

        public Medicine Medicine { get; set; }

        public int Quantity { get; set; }
    }
}