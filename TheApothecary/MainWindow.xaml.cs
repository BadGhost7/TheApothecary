using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using TheApothecary.Models;
using TheApothecary.Views;
using TheApothecary.Data;
using System.IO;
using System.Windows.Media;

namespace TheApothecary
{
    public partial class MainWindow : Window
    {
        private List<Medicine> medicines;
        private List<CartItem> cartItems;
        private User currentUser;

      
        public class MedicineDisplay
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Price { get; set; }
            public string StockQuantity { get; set; }
            public bool RequiresPrescription { get; set; }
            public string RequiresPrescriptionText { get; set; }
            public string RequiresPrescriptionColor { get; set; }
            public string Category { get; set; }
            public bool IsEmployee { get; set; }
        }

        public MainWindow()
        {
           

            InitializeComponent();

         
            InitializeDatabase();

           
            LoadMedicinesFromDatabase();

          
            cartItems = new List<CartItem>();

           
            UpdateUserInterface();
            UpdateCartButton();

       

        }

   
        private void InitializeDatabase()
        {
            try
            {
                var db = PharmacyDbContext.Instance;
                StatusText.Text = "База данных готова!";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка БД: {ex.Message}";
            }
        }

  
        private void LoadMedicinesFromDatabase()
        {
            try
            {
                var db = PharmacyDbContext.Instance;
                medicines = db.GetAllMedicines();

                if (medicines == null || medicines.Count == 0)
                {
                    StatusText.Text = "Нет лекарств в базе";
                    medicines = new List<Medicine>();
                }
                else
                {
                    StatusText.Text = $"Загружено лекарств: {medicines.Count}";
                }

                
                bool isEmployee = currentUser != null &&
                                 (currentUser.Role == UserRole.Employee || currentUser.Role == UserRole.Admin);

          
                var displayMedicines = medicines.Select(med => new MedicineDisplay
                {
                    Id = med.Id,
                    Name = med.Name,
                    Description = med.Description,
                    Price = med.Price.ToString("F2") + "₽",
                    StockQuantity = med.StockQuantity.ToString(),
                    RequiresPrescription = med.RequiresPrescription,
                    RequiresPrescriptionText = med.RequiresPrescription ? "Требуется" : "Не требуется",
                    RequiresPrescriptionColor = med.RequiresPrescription ? "#E74C3C" : "#27AE60",
                    Category = med.Category,
                    IsEmployee = isEmployee 
                }).ToList();

                MedicinesItemsControl.ItemsSource = displayMedicines;
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка загрузки: {ex.Message}";
                MedicinesItemsControl.ItemsSource = null;
            }
        }
        private void CheckPrescriptionsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null ||
                (currentUser.Role != UserRole.Employee && currentUser.Role != UserRole.Admin))
            {
                MessageBox.Show("Только сотрудники могут проверять рецепты",
                               "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var requestsWindow = new PrescriptionRequestsWindow(currentUser);
            requestsWindow.Owner = this;
            requestsWindow.ShowDialog();
        }


        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null)
            {
                MessageBox.Show("Пожалуйста, войдите в систему чтобы добавлять товары в корзину",
                    "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var button = (Button)sender;
            if (button.Tag == null) return;

            int medicineId = (int)button.Tag;
            var medicine = medicines.FirstOrDefault(m => m.Id == medicineId);

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
            int cartCount = cartItems.Sum(item => item.Quantity);
            CartBtn.Content = $"🛒 Корзина ({cartCount})";
            CartBtn.IsEnabled = currentUser != null;
        }


        private void CartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null)
            {
                MessageBox.Show("Пожалуйста, войдите в систему для просмотра корзины",
                    "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var cartWindow = new CartWindow(cartItems, currentUser);
            cartWindow.Owner = this;
            cartWindow.ShowDialog();
            UpdateCartButton();
        }

        private void CheckPrescriptionStatusAfterLogin()
        {
            if (currentUser == null) return;

            try
            {
                var db = PharmacyDbContext.Instance;

                // Проверяем только одобренные рецепты за последний день
                var approvedToday = db.PrescriptionRequests
                    .Where(r => r.UserId == currentUser.Id &&
                               r.Status == PrescriptionStatus.Approved &&
                               r.ReviewDate.HasValue &&
                               r.ReviewDate.Value.Date == DateTime.Today)
                    .ToList();

                if (approvedToday.Any())
                {
                    string medicines = string.Join(", ", approvedToday.Select(r => r.MedicineName));
                    MessageBox.Show($"✅ Ваши рецепты проверены!\n\n" +
                                   $"Одобренные лекарства: {medicines}\n\n" +
                                   $"Теперь вы можете оформить заказ в корзине.",
                                   "Рецепты проверены",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                }
            }
            catch
            {
                // Игнорируем ошибки при проверке
            }
        }
        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser != null)
            {
                // Если пользователь уже авторизован - это кнопка выхода
                Logout();
                return;
            }

            var loginWindow = new LoginWindow();
            loginWindow.Owner = this;

            bool? result = loginWindow.ShowDialog();

            if (result == true) // Успешный вход
            {
                var db = PharmacyDbContext.Instance;
                currentUser = db.Login(loginWindow.Username, loginWindow.Password);

                if (currentUser != null)
                {
                    UpdateUserInterface();
                    StatusText.Text = $"Добро пожаловать, {currentUser.Username}! (Роль: {currentUser.Role})";

                    // ПРОВЕРЯЕМ СТАТУС РЕЦЕПТОВ ПОСЛЕ ВХОДА
                    CheckPrescriptionStatusAfterLogin();
                }
                else
                {
                    MessageBox.Show("❌ Неверный логин или пароль\n\n" +
                                  $"Вы ввели: '{loginWindow.Username}'\n" +
                                  $"Попробуйте использовать логин, который был показан при регистрации.\n\n" +
                                  $"Тестовые аккаунты:\n" +
                                  $"• admin/admin123 (Администратор)\n" +
                                  $"• employee/employee123 (Сотрудник)\n" +
                                  $"• customer/customer123 (Покупатель)",
                                  "Ошибка авторизации",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                    StatusText.Text = "Неверный логин или пароль";
                }
            }
            else if (result == false) // Пользователь нажал "Зарегистрироваться"
            {
                OpenRegistrationWindow();
            }
        }



        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenRegistrationWindow();
        }

        private void OpenRegistrationWindow()
        {
            var registrationWindow = new RegistrationWindow();
            registrationWindow.Owner = this;

            bool? result = registrationWindow.ShowDialog();

            if (result == true) 
            {
            
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
                   
                    MessageBox.Show($"Регистрация завершена!\n\n" +
                                   $"Логин: {registrationWindow.Username}\n" +
                                   $"Email: {newUser.Email}\n" +
                                   $"Роль: {newUser.Role}\n\n" +
                                   $"Запомните ваш логин для входа в систему!",
                                   "Регистрация успешна",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);

                    StatusText.Text = $"Пользователь {registrationWindow.Username} зарегистрирован! Теперь войдите в систему.";
                }
                else
                {
                    MessageBox.Show("Пользователь с таким именем уже существует", "Ошибка регистрации",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Пользователь с таким именем уже существует";
                }
            }
        }


        private void Logout()
        {
            var result = MessageBox.Show($"Вы уверены, что хотите выйти из аккаунта {currentUser.Username}?",
                "Подтверждение выхода", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                currentUser = null;
                cartItems.Clear(); // Очищаем корзину при выходе
                UpdateUserInterface();
                UpdateCartButton();
                StatusText.Text = "Вы вышли из системы";
            }
        }


        private void UpdateUserInterface()
        {
            if (currentUser != null)
            {
                UserInfoText.Text = $"{currentUser.Username} ({currentUser.Role})";
                LoginBtn.Content = "Выйти";
                RegisterBtn.Visibility = Visibility.Collapsed;

                // Показываем/скрываем кнопки управления для сотрудников
                ManageMedicinesPanel.Visibility =
                    (currentUser.Role == UserRole.Employee || currentUser.Role == UserRole.Admin)
                    ? Visibility.Visible : Visibility.Collapsed;

                // Показываем кнопку корзины только для покупателей
                CartBtn.Visibility =
                    (currentUser.Role == UserRole.Customer)
                    ? Visibility.Visible : Visibility.Collapsed;
                CartBtn.IsEnabled = true;

                // Показываем кнопку проверки рецептов только для сотрудников
                CheckPrescriptionsBtn.Visibility =
                    (currentUser.Role == UserRole.Employee || currentUser.Role == UserRole.Admin)
                    ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                UserInfoText.Text = "Гость";
                LoginBtn.Content = "Войти";
                RegisterBtn.Visibility = Visibility.Visible;
                ManageMedicinesPanel.Visibility = Visibility.Collapsed;
                CartBtn.Visibility = Visibility.Visible;
                CartBtn.IsEnabled = false;
                CheckPrescriptionsBtn.Visibility = Visibility.Collapsed;
            }
        }


        private void UpdateMedicineControlsVisibility()
        {
          
        
        }

        // КНОПКА ДЛЯ ПРОВЕРКИ БАЗЫ
        private void CheckDbButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var db = PharmacyDbContext.Instance;
                var medicinesCount = db.Medicines.Count();
                var usersCount = db.Users.Count();

                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pharmacy.db");
                bool dbExists = File.Exists(dbPath);
                long fileSize = dbExists ? new FileInfo(dbPath).Length : 0;

                string adminInfo = "admin/admin123 (Админ)";
                string employeeInfo = "employee/employee123 (Сотрудник)";
                string customerInfo = "customer/customer123 (Покупатель)";

                MessageBox.Show(
                    $"✅ База работает!\n" +
                    $"💊 Лекарств: {medicinesCount}\n" +
                    $"👤 Пользователей: {usersCount}\n" +
                    $"📁 Файл: {(dbExists ? "СУЩЕСТВУЕТ" : "НЕ НАЙДЕН")}\n" +
                    $"📦 Размер: {fileSize} байт\n\n" +
                    $"Тестовые пользователи:\n" +
                    $"{adminInfo}\n{employeeInfo}\n{customerInfo}",
                    "Проверка базы");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка");
            }
        }

        // КНОПКА ДЛЯ СБРОСА БАЗЫ
     
        private void ResetDbButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Закрыть приложение и сбросить базу данных?",
                               "Сброс базы",
                               MessageBoxButton.YesNo,
                               MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                string appPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                string appDir = Path.GetDirectoryName(appPath);

                // Создаем простую команду для удаления и перезапуска
                string cmd = $"/C timeout /t 1 & del /f /q \"{appDir}\\pharmacy.db*\" & start \"\" \"{appPath}\"";

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = cmd,
                    CreateNoWindow = true,
                    UseShellExecute = false
                });

                Application.Current.Shutdown();
            }
        }

        // Метод для закрытия всех соединений с базой данных
        private void CloseDatabaseConnections()
        {
            try
            {
                // 1. Закрываем контекст базы данных
                PharmacyDbContext.ResetInstance();

                // 2. Собираем мусор для освобождения ресурсов
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // 3. Если используете Entity Framework, можно попробовать очистить пулы
                // Microsoft.EntityFrameworkCore.Sqlite не имеет прямого метода очистки пулов,
                // но можно попробовать закрыть все соединения через reflection

                Console.WriteLine("Соединения с базой данных закрыты");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при закрытии соединений: {ex.Message}");
            }
        }

        // КНОПКА ДОБАВЛЕНИЯ ЛЕКАРСТВА (для сотрудников)
        private void AddMedicineBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null ||
                (currentUser.Role != UserRole.Employee && currentUser.Role != UserRole.Admin))
            {
                MessageBox.Show("Только сотрудники и администраторы могут добавлять лекарства",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var addMedicineWindow = new AddEditMedicineWindow();
            addMedicineWindow.Owner = this;

            if (addMedicineWindow.ShowDialog() == true && addMedicineWindow.IsSaved)
            {
                try
                {
                    var db = PharmacyDbContext.Instance;
                    var newMedicine = addMedicineWindow.ViewModel.ToMedicine();

                    db.Medicines.Add(newMedicine);
                    db.SaveChanges();

                    LoadMedicinesFromDatabase();
                    StatusText.Text = $"Лекарство '{newMedicine.Name}' добавлено!";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении лекарства: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // КНОПКА РЕДАКТИРОВАНИЯ ЛЕКАРСТВА
        private void EditMedicineBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null ||
                (currentUser.Role != UserRole.Employee && currentUser.Role != UserRole.Admin))
            {
                MessageBox.Show("Только сотрудники и администраторы могут редактировать лекарства",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var button = (Button)sender;
            if (button.Tag == null) return;

            int medicineId = (int)button.Tag;
            var medicine = medicines.FirstOrDefault(m => m.Id == medicineId);

            if (medicine == null) return;

            var editWindow = new AddEditMedicineWindow(medicine);
            editWindow.Owner = this;

            if (editWindow.ShowDialog() == true && editWindow.IsSaved)
            {
                try
                {
                    var db = PharmacyDbContext.Instance;
                    var existingMedicine = db.Medicines.Find(medicineId);

                    if (existingMedicine != null)
                    {
                        var updatedMedicine = editWindow.ViewModel.ToMedicine();

                        existingMedicine.Name = updatedMedicine.Name;
                        existingMedicine.Description = updatedMedicine.Description;
                        existingMedicine.Price = updatedMedicine.Price;
                        existingMedicine.StockQuantity = updatedMedicine.StockQuantity;
                        existingMedicine.RequiresPrescription = updatedMedicine.RequiresPrescription;
                        existingMedicine.Category = updatedMedicine.Category;

                        db.SaveChanges();
                        LoadMedicinesFromDatabase();
                        StatusText.Text = $"Лекарство '{existingMedicine.Name}' обновлено!";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при редактировании лекарства: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // КНОПКА УДАЛЕНИЯ ЛЕКАРСТВА
        private void DeleteMedicineBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null ||
                (currentUser.Role != UserRole.Employee && currentUser.Role != UserRole.Admin))
            {
                MessageBox.Show("Только сотрудники и администраторы могут удалять лекарства",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var button = (Button)sender;
            if (button.Tag == null) return;

            int medicineId = (int)button.Tag;
            var medicine = medicines.FirstOrDefault(m => m.Id == medicineId);

            if (medicine == null) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить лекарство '{medicine.Name}'?\n\n" +
                $"Это действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var db = PharmacyDbContext.Instance;
                    var existingMedicine = db.Medicines.Find(medicineId);

                    if (existingMedicine != null)
                    {
                        db.Medicines.Remove(existingMedicine);
                        db.SaveChanges();
                        LoadMedicinesFromDatabase();
                        StatusText.Text = $"Лекарство '{medicine.Name}' удалено!";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении лекарства: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Обновить видимость кнопок управления лекарствами
        private void MedicinesItemsControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateMedicineControlsInItems();
        }

        private void UpdateMedicineControlsInItems()
        {
            // Получаем все контейнеры элементов
            var itemsControl = MedicinesItemsControl;
            var itemContainerGenerator = itemsControl.ItemContainerGenerator;

            foreach (var item in itemsControl.Items)
            {
                var container = itemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                if (container != null)
                {
                    // Находим панель управления внутри контейнера
                    var controlsPanel = FindVisualChild<StackPanel>(container, "MedicineControlsPanel");
                    if (controlsPanel != null)
                    {
                        // Устанавливаем видимость в зависимости от роли пользователя
                        controlsPanel.Visibility = (currentUser != null &&
                            (currentUser.Role == UserRole.Employee || currentUser.Role == UserRole.Admin))
                            ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
        }

        // Вспомогательный метод для поиска дочерних элементов
        private T FindVisualChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T && (child as FrameworkElement)?.Name == childName)
                    return child as T;

                var result = FindVisualChild<T>(child, childName);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}