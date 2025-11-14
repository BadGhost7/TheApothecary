using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using TheApothecary.Models;
using TheApothecary.Views;
using PharmacyApp.Models;

namespace TheApothecary
{
    public partial class MainWindow : Window
    {
        private List<Medicine> medicines;
        private List<CartItem> cartItems;
        private User currentUser;

        public MainWindow()
        {
            InitializeComponent();
            cartItems = new List<CartItem>();
            LoadMedicines();
            UpdateCartButton();
        }

        private void LoadMedicines()
        {
            medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Парацетамол", Description = "Обезболивающее и жаропонижающее средство",
                    Price = "150₽", StockQuantity = 50, RequiresPrescription = false },
                new Medicine { Id = 2, Name = "Амоксициллин", Description = "Антибиотик широкого спектра действия",
                    Price = "450₽", StockQuantity = 20, RequiresPrescription = true },
                new Medicine { Id = 3, Name = "Ибупрофен", Description = "Противовоспалительное средство",
                    Price = "200₽", StockQuantity = 35, RequiresPrescription = false },
                new Medicine { Id = 4, Name = "Лоратадин", Description = "Противоаллергическое средство",
                    Price = "180₽", StockQuantity = 40, RequiresPrescription = false },
                new Medicine { Id = 5, Name = "Трамадол", Description = "Сильное обезболивающее",
                    Price = "650₽", StockQuantity = 15, RequiresPrescription = true }
            };

           
            var displayMedicines = medicines.Select(med => new
            {
                Id = med.Id,
                Name = med.Name,
                Description = med.Description,
                Price = med.Price,
                StockQuantity = med.StockQuantity,
                RequiresPrescription = med.RequiresPrescription,
                RequiresPrescriptionText = med.RequiresPrescription ? "Да" : "Нет",
                OriginalMedicine = med
            }).ToList();

            MedicinesItemsControl.ItemsSource = displayMedicines;
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            dynamic dataContext = button.DataContext;
            var medicine = dataContext.OriginalMedicine as Medicine;

            if (medicine == null) return;

            var existingItem = cartItems.FirstOrDefault(item => item.Medicine.Id == medicine.Id);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cartItems.Add(new CartItem
                {
                    Medicine = medicine,
                    Quantity = 1,
                    IsPrescriptionVerified = !medicine.RequiresPrescription
                });
            }

            UpdateCartButton();
            StatusText.Text = $"Добавлено: {medicine.Name}";
        }

        private void UpdateCartButton()
        {
            CartBtn.Content = $"🛒 Корзина ({cartItems.Sum(item => item.Quantity)})";
        }

        private void CartBtn_Click(object sender, RoutedEventArgs e)
        {
            var cartWindow = new CartWindow(cartItems, currentUser);
            cartWindow.Owner = this;
            cartWindow.ShowDialog();
            UpdateCartButton();
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Owner = this;
            if (loginWindow.ShowDialog() == true)
            {
               
                UpdateUserInterface();
            }
        }

        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            var registrationWindow = new RegistrationWindow();
            registrationWindow.Owner = this;
            if (registrationWindow.ShowDialog() == true)
            {
                StatusText.Text = "Регистрация завершена! Теперь войдите в систему.";
            }
        }

        private void UpdateUserInterface()
        {
            if (currentUser != null)
            {
                UserInfoText.Text = currentUser.Username;
                LoginBtn.Content = "Выйти";
                RegisterBtn.Visibility = Visibility.Collapsed;
                StatusText.Text = $"Добро пожаловать, {currentUser.Username}!";
            }
            else
            {
                UserInfoText.Text = "Гость";
                LoginBtn.Content = "Войти";
                RegisterBtn.Visibility = Visibility.Visible;
            }
        }
    }
}