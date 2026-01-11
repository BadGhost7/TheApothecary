using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheApothecary.Models
{
    public class CartItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Внешний ключ на Medicine
        public int MedicineId { get; set; }

        [ForeignKey("MedicineId")]
        public Medicine Medicine { get; set; }

        public int Quantity { get; set; }

        // Внешний ключ на Prescription (может быть null)
        public int? PrescriptionId { get; set; }

        [ForeignKey("PrescriptionId")]
        public Prescription Prescription { get; set; }

        public bool IsPrescriptionVerified { get; set; }

        public CartItem()
        {
        }

        public CartItem(Medicine medicine, int quantity = 1)
        {
            Medicine = medicine;
            MedicineId = medicine.Id;
            Quantity = quantity;
            IsPrescriptionVerified = !medicine.RequiresPrescription;
        }
    }
}