using System;

namespace TheApothecary.Models
{
    public class PrescriptionRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int MedicineId { get; set; }
        public string MedicineName { get; set; }
        public DateTime RequestDate { get; set; }
        public PrescriptionStatus Status { get; set; }
        public string? EmployeeNotes { get; set; }
        public DateTime? ReviewDate { get; set; }
        public int? ReviewedByEmployeeId { get; set; }
    }

    public enum PrescriptionStatus
    {
        Pending, 
        Approved,   
        Rejected    
    }
}