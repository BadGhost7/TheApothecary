using System.Windows;
using System.Windows.Input;
using TheApothecary.Models;

namespace TheApothecary.Views
{
    public partial class LoginWindow : Window
    {
        public User LoggedInUser { get; private set; }

        public LoginWindow()
        {
            InitializeComponent(); 
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка входа",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

          //dodelat
            if (EmailTextBox.Text == "admin@apothecary.com" && PasswordBox.Password == "admin123")
            {
                LoggedInUser = new User
                {
                    Id = 1,
                    Username = "Администратор",
                    Email = EmailTextBox.Text,
                    Role = UserRole.Admin
                };
                this.DialogResult = true;
                this.Close();
            }
            else if (EmailTextBox.Text == "employee@apothecary.com" && PasswordBox.Password == "employee123")
            {
                LoggedInUser = new User
                {
                    Id = 2,
                    Username = "Сотрудник",
                    Email = EmailTextBox.Text,
                    Role = UserRole.Employee
                };
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный email или пароль", "Ошибка входа",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var registrationWindow = new RegistrationWindow();
            registrationWindow.Owner = this;
            registrationWindow.ShowDialog();
        }
    }
}