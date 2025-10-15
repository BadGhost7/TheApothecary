namespace PharmacyApp.Models
{
    public class Prescription
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public string PrescriptionNumber { get; set; }
        public string DoctorName { get; set; }
        public DateTime IssueDate { get; set; }
        public string PatientName { get; set; }
        public int VerifiedByEmployeeId { get; set; }
    }
}