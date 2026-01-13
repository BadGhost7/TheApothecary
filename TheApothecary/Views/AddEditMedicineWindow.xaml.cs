using System;
using System.Windows;
using TheApothecary.Models;

namespace TheApothecary.Views
{
    public partial class AddEditMedicineWindow : Window
    {
        public MedicineViewModel ViewModel { get; private set; }
        public bool IsSaved { get; private set; }

        public AddEditMedicineWindow()
        {
            InitializeComponent();
            ViewModel = new MedicineViewModel();
            DataContext = ViewModel;
            ViewModel.WindowTitle = "Добавить лекарство";
        }

        public AddEditMedicineWindow(Medicine medicine)
        {
            InitializeComponent();
            ViewModel = new MedicineViewModel(medicine);
            DataContext = ViewModel;
            ViewModel.WindowTitle = "Редактировать лекарство";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.Validate())
            {
                MessageBox.Show("Пожалуйста, проверьте введенные данные:\n" +
                               "• Название не должно быть пустым\n" +
                               "• Цена должна быть положительным числом\n" +
                               "• Количество должно быть неотрицательным числом\n" +
                               "• Категория не должна быть пустой",
                               "Ошибка валидации",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
                return;
            }

            IsSaved = true;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsSaved = false;
            DialogResult = false;
            Close();
        }
    }

    
    public class MedicineViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public string StockQuantity { get; set; }
        public bool RequiresPrescription { get; set; }
        public string Category { get; set; }
        public string WindowTitle { get; set; }

        public MedicineViewModel()
        {
            Name = "";
            Description = "";
            Price = "0";
            StockQuantity = "0";
            Category = "";
            RequiresPrescription = false;
        }

        public MedicineViewModel(Medicine medicine)
        {
            Id = medicine.Id;
            Name = medicine.Name;
            Description = medicine.Description;
            Price = medicine.Price.ToString("F2");
            StockQuantity = medicine.StockQuantity.ToString();
            RequiresPrescription = medicine.RequiresPrescription;
            Category = medicine.Category;
        }

        public Medicine ToMedicine()
        {
            return new Medicine
            {
                Id = this.Id,
                Name = this.Name.Trim(),
                Description = this.Description.Trim(),
                Price = decimal.Parse(this.Price),
                StockQuantity = int.Parse(this.StockQuantity),
                RequiresPrescription = this.RequiresPrescription,
                Category = this.Category.Trim(),
                ImagePath = "/Images/default_medicine.jpg"
            };
        }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return false;

            if (!decimal.TryParse(Price, out decimal price) || price <= 0)
                return false;

            if (!int.TryParse(StockQuantity, out int quantity) || quantity < 0)
                return false;

            if (string.IsNullOrWhiteSpace(Category))
                return false;

            return true;
        }
    }
}