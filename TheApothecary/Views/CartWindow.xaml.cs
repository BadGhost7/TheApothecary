using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using TheApothecary.Models;
using TheApothecary.Data;

namespace TheApothecary.Views
{
    public partial class CartWindow : Window
    {
        private List<CartItem> cartItems;
        private User currentUser;
        private PharmacyDbContext db;

        // Класс для отображения товара в корзине
        public class CartDisplayItem
        {
            public CartItem CartItem { get; set; }
            public Medicine Medicine { get; set; }
            public int Quantity { get; set; }
            public string Price { get; set; }
            public string TotalPrice { get; set; }
            public bool RequiresPrescription { get; set; }
            public string PrescriptionStatusText { get; set; }
            public string PrescriptionStatusColor { get; set; }
            public bool ShowRequestButton { get; set; }
        }

        // Конвертер для Boolean в текст
        public class BoolToTextConverter : System.Windows.Data.IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is bool boolValue)
                {
                    return boolValue ? "Требуется" : "Не требуется";
                }
                return "Не требуется";
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        // Конвертер для Boolean в цвет
        public class BoolToColorConverter : System.Windows.Data.IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is bool boolValue)
                {
                    return boolValue ? "#E74C3C" : "#27AE60";
                }
                return "#95A5A6";
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public CartWindow(List<CartItem> items, User user)
        {
            InitializeComponent();
            cartItems = items;
            currentUser = user;
            db = PharmacyDbContext.Instance;

            // Добавляем конвертеры в ресурсы
            this.Resources.Add("BoolToVisText", new BoolToTextConverter());
            this.Resources.Add("BoolToColor", new BoolToColorConverter());

            LoadCartItems();
            UpdateTotal();
        }

        private void LoadCartItems()
        {
            var displayItems = new List<CartDisplayItem>();
            bool hasPrescriptionItems = false;
            bool hasUnverifiedItems = false;

            foreach (var item in cartItems)
            {
                // Проверяем статус рецепта в базе данных
                bool hasApprovedPrescription = false;
                if (item.Medicine.RequiresPrescription && currentUser != null)
                {
                    hasApprovedPrescription = db.HasApprovedPrescription(currentUser.Id, item.Medicine.Id);
                    item.IsPrescriptionVerified = hasApprovedPrescription;
                }

                var displayItem = new CartDisplayItem
                {
                    CartItem = item,
                    Medicine = item.Medicine,
                    Quantity = item.Quantity,
                    Price = item.Medicine.Price.ToString("F2") + "₽",
                    TotalPrice = (item.Medicine.Price * item.Quantity).ToString("F2") + "₽",
                    RequiresPrescription = item.Medicine.RequiresPrescription,
                    PrescriptionStatusText = item.Medicine.RequiresPrescription
                        ? (hasApprovedPrescription ? "✅ Проверен" : "⏳ Ожидает проверки")
                        : "✅ Не требуется",
                    PrescriptionStatusColor = item.Medicine.RequiresPrescription
                        ? (hasApprovedPrescription ? "#27AE60" : "#F39C12")
                        : "#95A5A6",
                    ShowRequestButton = item.Medicine.RequiresPrescription &&
                                       !hasApprovedPrescription &&
                                       currentUser != null
                };

                displayItems.Add(displayItem);

                if (item.Medicine.RequiresPrescription)
                {
                    hasPrescriptionItems = true;
                    if (!hasApprovedPrescription)
                    {
                        hasUnverifiedItems = true;
                    }
                }
            }

            CartItemsControl.ItemsSource = displayItems;

            // Показываем/скрываем предупреждение
            if (hasPrescriptionItems)
            {
                PrescriptionWarningBorder.Visibility = Visibility.Visible;
                if (hasUnverifiedItems)
                {
                    PrescriptionWarningText.Text = "⚠️ Некоторые товары требуют проверки рецепта. Нажмите 'Запросить проверку рецепта' для отправки запроса сотруднику.";
                }
                else
                {
                    PrescriptionWarningText.Text = "✅ Все рецепты проверены. Можете оформлять заказ.";
                }
            }
            else
            {
                PrescriptionWarningBorder.Visibility = Visibility.Collapsed;
            }
        }

        // КНОПКА ДЛЯ ЗАПРОСА ПРОВЕРКИ РЕЦЕПТА
        private void RequestPrescriptionCheck_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null)
            {
                MessageBox.Show("Для запроса проверки рецепта необходимо войти в систему",
                               "Требуется авторизация",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            var button = (Button)sender;
            int medicineId = (int)button.Tag;

            // Находим лекарство в корзине
            var cartItem = cartItems.FirstOrDefault(item => item.Medicine.Id == medicineId);
            if (cartItem == null) return;

            // Проверяем, не отправлен ли уже запрос
            if (cartItem.IsPrescriptionVerified)
            {
                MessageBox.Show("Рецепт для этого лекарства уже проверен!",
                               "Информация",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
                return;
            }

            // Создаем запрос на проверку
            bool success = db.CreatePrescriptionRequest(
                currentUser.Id,
                currentUser.Username,
                medicineId,
                cartItem.Medicine.Name
            );

            if (success)
            {
                MessageBox.Show("✅ Запрос на проверку рецепта отправлен!\n\n" +
                               "Сотрудник проверит ваш рецепт и уведомит вас.\n" +
                               "Вы получите уведомление, когда рецепт будет проверен.",
                               "Запрос отправлен",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);

                // Обновляем интерфейс
                LoadCartItems();
            }
            else
            {
                MessageBox.Show("❌ Не удалось отправить запрос. Возможно, запрос уже существует.",
                               "Ошибка",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }

        // Остальные методы оставляем как есть...
        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int medicineId = (int)button.Tag;

            var itemToRemove = cartItems.FirstOrDefault(item => item.Medicine.Id == medicineId);
            if (itemToRemove != null)
            {
                cartItems.Remove(itemToRemove);
                LoadCartItems();
                UpdateTotal();
            }
        }

        private void UpdateTotal()
        {
            int totalItems = cartItems.Sum(item => item.Quantity);
            decimal totalPrice = cartItems.Sum(item => item.Medicine.Price * item.Quantity);

            TotalText.Text = $"Итого: {totalPrice:F2}₽ (товаров: {totalItems})";

            // Проверяем можно ли оформить заказ
            bool canCheckout = true;
            foreach (var item in cartItems)
            {
                if (item.Medicine.RequiresPrescription && !item.IsPrescriptionVerified)
                {
                    canCheckout = false;
                    break;
                }
            }

            CheckoutButton.IsEnabled = canCheckout && cartItems.Count > 0;
            CheckoutButton.Content = canCheckout ? "✅ Оформить заказ" : "❌ Требуется проверка рецептов";
            CheckoutButton.Background = canCheckout ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Gray;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (!cartItems.Any())
            {
                MessageBox.Show("Корзина пуста", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Проверяем все ли рецепты проверены
            foreach (var item in cartItems)
            {
                if (item.Medicine.RequiresPrescription && !item.IsPrescriptionVerified)
                {
                    MessageBox.Show("Некоторые лекарства требуют проверки рецепта перед покупкой",
                                   "Требуется проверка",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
                    return;
                }
            }

            MessageBox.Show("✅ Заказ успешно оформлен!\n\n" +
                           $"Количество товаров: {cartItems.Sum(i => i.Quantity)}\n" +
                           $"Общая сумма: {cartItems.Sum(i => i.Medicine.Price * i.Quantity):F2}₽\n\n" +
                           "Спасибо за покупку!",
                           "Заказ оформлен",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);

            // Очищаем корзину после успешного заказа
            cartItems.Clear();
            DialogResult = true;
            Close();
        }
    }
}