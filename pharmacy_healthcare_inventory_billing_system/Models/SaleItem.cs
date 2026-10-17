namespace pharmacy_healthcare_inventory_billing_system.Models
{
    public class SaleItem
    {
        public int SaleItemId { get; set; }

        public int SaleId { get; set; }

        public Sale Sale { get; set; }

        public int MedicineId { get; set; }

        public Medicine Medicine { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Total
        {
            get
            {
                return Quantity * UnitPrice;
            }
        }
    }
}