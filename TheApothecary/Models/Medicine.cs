namespace TheApothecary.Models
{
    public class Medicine
    { //dobavit new fichi
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Price { get; set; } 
        public int StockQuantity { get; set; }
        public bool RequiresPrescription { get; set; }
        public string Category { get; set; }
        public string ImagePath { get; set; }
    }
}