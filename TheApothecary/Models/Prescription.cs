using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheApothecary.Models
{
    public class Prescription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int MedicineId { get; set; }
        public string PrescriptionNumber { get; set; }
        public string DoctorName { get; set; }
        public DateTime IssueDate { get; set; }
        public string PatientName { get; set; }
        public int? VerifiedByEmployeeId { get; set; }

        public Medicine Medicine { get; set; }
    }
}