using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using TheApothecary.Models;

namespace TheApothecary.Views
{
    public partial class CartWindow : Window
    {
        private List<CartItem> cartItems;
        private User currentUser;

        public CartWindow(List<CartItem> items, User user)
        {
            InitializeComponent();
            cartItems = items ?? new List<CartItem>();
            currentUser = user;
            LoadCartData();
            UpdateTotal();
            CheckPrescriptionRequirements();
        }

        private void LoadCartData()
        {
          
            var displayItems = cartItems.Select(item => new
            {
                Medicine = item.Medicine,
                Quantity = item.Quantity,
                Price = item.Medicine.Price,
              
                TotalPrice = item.Medicine.Price * item.Quantity, 
                RequiresPrescription = item.Medicine.RequiresPrescription ? "Да" : "Нет",
                RequiresPrescriptionTextColor = item.Medicine.RequiresPrescription ? "#E74C3C" : "#27AE60",
                IsPrescriptionVerified = item.IsPrescriptionVerified ? "Да" : "Нет",
                IsPrescriptionVerifiedTextColor = item.IsPrescriptionVerified ? "#27AE60" : "#E74C3C",
                OriginalItem = item
            }).ToList();

          
            CartItemsControl.ItemsSource = displayItems;
        }

       
        private decimal CalculateTotal()
        {
            decimal total = 0;

            foreach (var item in cartItems)
            {
                if (item.Medicine != null)
                {
                    total += item.Medicine.Price * item.Quantity;
                }
            }

            return total;
        }

        
        private decimal CalculateItemTotal(decimal price, int quantity)
        {
            return price * quantity;
        }

        
        private decimal CalculateTotalAmount()
        {
            return CalculateTotal(); 
        }

        private void CheckPrescriptionRequirements()
        {
            bool hasUnverifiedPrescriptions = cartItems.Any(item =>
                item.Medicine.RequiresPrescription && !item.IsPrescriptionVerified);

            PrescriptionWarningText.Visibility = hasUnverifiedPrescriptions ?
                Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateTotal()
        {
            if (cartItems == null || !cartItems.Any())
            {
                TotalText.Text = "Итого: 0₽ (товаров: 0)";
                return;
            }

            decimal total = CalculateTotalAmount(); 
            int totalItems = cartItems.Sum(item => item.Quantity);
            TotalText.Text = $"Итого: {total:F2}₽ (товаров: {totalItems})";
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                dynamic dataContext = button.DataContext;
                var originalItem = dataContext.OriginalItem as CartItem;

                if (originalItem != null)
                {
                    cartItems.Remove(originalItem);
                    LoadCartData();
                    UpdateTotal();
                    CheckPrescriptionRequirements();
                }
            }
        }

        private void VerifyPrescription_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                dynamic dataContext = button.DataContext;
                var originalItem = dataContext.OriginalItem as CartItem;

                if (originalItem != null)
                {
                    if (currentUser == null || currentUser.Role == UserRole.Customer)
                    {
                        MessageBox.Show("Только сотрудники могут проверять рецепты", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    originalItem.IsPrescriptionVerified = true;
                    LoadCartData();
                    CheckPrescriptionRequirements();

                    MessageBox.Show($"Рецепт для '{originalItem.Medicine.Name}' подтвержден!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (cartItems == null || !cartItems.Any())
            {
                MessageBox.Show("Корзина пуста", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

        
            bool allPrescriptionsVerified = true;
            List<string> unverifiedMeds = new List<string>();

            foreach (var item in cartItems)
            {
                if (item.Medicine.RequiresPrescription && !item.IsPrescriptionVerified)
                {
                    allPrescriptionsVerified = false;
                    unverifiedMeds.Add(item.Medicine.Name);
                }
            }

            if (!allPrescriptionsVerified)
            {
                string medsList = string.Join(", ", unverifiedMeds);
                MessageBox.Show($"Следующие товары требуют проверки рецепта: {medsList}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

        
            bool allInStock = true;
            List<string> outOfStockMeds = new List<string>();

            foreach (var item in cartItems)
            {
                if (item.Quantity > item.Medicine.StockQuantity)
                {
                    allInStock = false;
                    outOfStockMeds.Add($"{item.Medicine.Name} (требуется: {item.Quantity}, в наличии: {item.Medicine.StockQuantity})");
                }
            }

            if (!allInStock)
            {
                string stockList = string.Join("\n", outOfStockMeds);
                MessageBox.Show($"Недостаточно товаров на складе:\n{stockList}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

         
            decimal totalAmount = CalculateTotalAmount(); 

            MessageBox.Show($"Заказ успешно оформлен!\nОбщая сумма: {totalAmount:F2}₽\nСпасибо за покупку!", "Успех",
                          MessageBoxButton.OK, MessageBoxImage.Information);

           
            cartItems.Clear();
            this.DialogResult = true;
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}