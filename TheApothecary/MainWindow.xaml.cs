using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using TheApothecary.Models;
using TheApothecary.Views;
using TheApothecary.Data;
using System.IO;
using System;

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

            // 1. ИНИЦИАЛИЗИРУЕМ БАЗУ (самое важное!)
            InitializeDatabase();

            // 2. Загружаем лекарства ИЗ БАЗЫ
            LoadMedicinesFromDatabase();

            // 3. Инициализируем корзину
            cartItems = new List<CartItem>();
            UpdateCartButton();
        }

        // ИНИЦИАЛИЗАЦИЯ БАЗЫ - ВСЕГДА РАБОТАЕТ!
        private void InitializeDatabase()
        {
            try
            {
                // Используем синглтон
                var db = PharmacyDbContext.Instance;
                StatusText.Text = "База данных готова!";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка БД: {ex.Message}";
            }
        }

        // ЗАГРУЗКА ЛЕКАРСТВ ИЗ БАЗЫ
        private void LoadMedicinesFromDatabase()
        {
            try
            {
                // Получаем все лекарства из базы
                var db = PharmacyDbContext.Instance;
                medicines = db.GetAllMedicines();

                // Проверяем что лекарства загрузились
                if (medicines == null || medicines.Count == 0)
                {
                    StatusText.Text = "Нет лекарств в базе";
                    medicines = new List<Medicine>();
                }
                else
                {
                    StatusText.Text = $"Загружено лекарств: {medicines.Count}";
                }

                // ОТОБРАЖАЕМ В ИНТЕРФЕЙСЕ
                var displayMedicines = medicines.Select(med => new
                {
                    Id = med.Id,
                    Name = med.Name,
                    Description = med.Description,
                    Price = med.Price.ToString("F2") + "₽", // Форматируем цену
                    StockQuantity = med.StockQuantity,
                    RequiresPrescription = med.RequiresPrescription,
                    RequiresPrescriptionText = med.RequiresPrescription ? "Да" : "Нет",
                    Category = med.Category,
                    OriginalMedicine = med  // Сохраняем оригинальный объект
                }).ToList();

                MedicinesItemsControl.ItemsSource = displayMedicines;
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка загрузки: {ex.Message}";
                MedicinesItemsControl.ItemsSource = null;
            }
        }

        // ОСТАЛЬНЫЕ МЕТОДЫ (оставляем как были)
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
                // Проверяем логин через базу
                var db = PharmacyDbContext.Instance;
                currentUser = db.Login(loginWindow.Username, loginWindow.Password);

                if (currentUser != null)
                {
                    UpdateUserInterface();
                    StatusText.Text = $"Добро пожаловать, {currentUser.Username}!";
                }
                else
                {
                    StatusText.Text = "Неверный логин или пароль";
                }
            }
        }

        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            var registrationWindow = new RegistrationWindow();
            registrationWindow.Owner = this;
            if (registrationWindow.ShowDialog() == true)
            {
                // Регистрируем через базу
                var newUser = new User
                {
                    Username = registrationWindow.Username,
                    Email = registrationWindow.Email,
                    Password = registrationWindow.Password,
                    Role = registrationWindow.Role
                };

                var db = PharmacyDbContext.Instance;
                if (db.AddUser(newUser))
                {
                    currentUser = newUser;
                    UpdateUserInterface();
                    StatusText.Text = $"Регистрация завершена! Добро пожаловать, {newUser.Username}!";
                }
                else
                {
                    StatusText.Text = "Пользователь с таким именем уже существует";
                }
            }
        }

        private void UpdateUserInterface()
        {
            if (currentUser != null)
            {
                UserInfoText.Text = currentUser.Username;
                LoginBtn.Content = "Выйти";
                RegisterBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                UserInfoText.Text = "Гость";
                LoginBtn.Content = "Войти";
                RegisterBtn.Visibility = Visibility.Visible;
            }
        }

        // КНОПКА ДЛЯ ПРОВЕРКИ БАЗЫ
        private void CheckDbButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var db = PharmacyDbContext.Instance;
                var medicinesCount = db.Medicines.Count();
                var usersCount = db.Users.Count();

                // Получаем путь к базе данных
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pharmacy.db");
                bool dbExists = File.Exists(dbPath);
                long fileSize = dbExists ? new FileInfo(dbPath).Length : 0;

                MessageBox.Show(
                    $"✅ База работает!\n" +
                    $"💊 Лекарств: {medicinesCount}\n" +
                    $"👤 Пользователей: {usersCount}\n" +
                    $"📁 Файл: {(dbExists ? "СУЩЕСТВУЕТ" : "НЕ НАЙДЕН")}\n" +
                    $"📦 Размер: {fileSize} байт\n" +
                    $"👑 Админ: admin/admin123",
                    "Проверка базы");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка");
            }
        }

        // КНОПКА ДЛЯ СБРОСА БАЗЫ - ИСПРАВЛЕННАЯ ВЕРСИЯ
        private void ResetDbButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "Вы уверены, что хотите сбросить базу данных?\n\n" +
                    "Все данные будут удалены, а база будет пересоздана с тестовыми лекарствами и администратором.",
                    "Подтверждение сброса базы данных",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;

                // 1. Сначала очищаем статический экземпляр контекста
                // Используем reflection для сброса синглтона
                var field = typeof(PharmacyDbContext).GetField("_instance",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(null, null);
                }

                // 2. Удаляем файл базы данных
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pharmacy.db");
                if (File.Exists(dbPath))
                {
                    try
                    {
                        // Пытаемся удалить файл несколько раз с паузами
                        for (int i = 0; i < 3; i++)
                        {
                            try
                            {
                                File.Delete(dbPath);
                                break; // Если удалось удалить, выходим из цикла
                            }
                            catch (IOException)
                            {
                                // Ждем немного и пробуем снова
                                System.Threading.Thread.Sleep(100);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось удалить файл базы данных: {ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // 3. Пересоздаем контекст и инициализируем базу
                var db = PharmacyDbContext.Instance;

                // 4. Перезагружаем данные
                LoadMedicinesFromDatabase();

                MessageBox.Show(
                    "✅ База данных успешно сброшена!\n\n" +
                    "• Файл базы пересоздан\n" +
                    "• Добавлены тестовые лекарства\n" +
                    "• Добавлен администратор (admin/admin123)\n\n" +
                    "Теперь вы можете проверить базу через кнопку 'Проверка базы'.",
                    "База данных пересоздана",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                StatusText.Text = "База пересоздана!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка при сбросе базы данных:\n\n{ex.Message}\n\n" +
                               $"Внутренняя ошибка: {ex.InnerException?.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Ошибка сброса базы";
            }
        }
    }
}