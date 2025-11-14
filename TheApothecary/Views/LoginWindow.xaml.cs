using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TheApothecary.Models;

namespace TheApothecary.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void EmailTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (EmailTextBox.Text == "Email")
            {
                EmailTextBox.Text = "";
                EmailTextBox.Foreground = Brushes.Black;
            }
        }

        private void EmailTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                EmailTextBox.Text = "Email";
                EmailTextBox.Foreground = Brushes.Gray;
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;

            if (email == "Email" || string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Введите email");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите пароль");
                return;
            }

            // Простая проверка без UserService
            User authenticatedUser = null;

            if (email == "admin@apothecary.com" && password == "admin123")
            {
                authenticatedUser = new User
                {
                    Id = 1,
                    Username = "Администратор",
                    Email = email,
                    Role = UserRole.Admin
                };
            }
            else if (email == "employee@apothecary.com" && password == "employee123")
            {
                authenticatedUser = new User
                {
                    Id = 2,
                    Username = "Сотрудник",
                    Email = email,
                    Role = UserRole.Employee
                };
            }

            if (authenticatedUser != null)
            {
                OpenMainWindow(authenticatedUser);
            }
            else
            {
                MessageBox.Show("Неверный email или пароль");
            }
        }

        private void OpenMainWindow(User user)
        {
            MainWindow mainWindow = new MainWindow();

          

            mainWindow.Show();
            this.Close();
        }
        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PasswordBox.Password))
            {
                PasswordPlaceholder.Visibility = Visibility.Visible;
            }
        }
        private void RegisterText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RegistrationWindow registrationWindow = new RegistrationWindow();
            registrationWindow.Show();
            this.Close();
        }
    }
}