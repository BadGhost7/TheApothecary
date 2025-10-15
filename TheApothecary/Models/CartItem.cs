using PharmacyApp.Models;
using System;

namespace TheApothecary.Models
{
    public class CartItem
    {
        public Medicine Medicine { get; set; }
        public int Quantity { get; set; }
        public Prescription Prescription { get; set; }
        public bool IsPrescriptionVerified { get; set; }

        public CartItem()
        {
        }

        public CartItem(Medicine medicine, int quantity = 1)
        {
            Medicine = medicine;
            Quantity = quantity;
            IsPrescriptionVerified = !medicine.RequiresPrescription;
        }
    }
}