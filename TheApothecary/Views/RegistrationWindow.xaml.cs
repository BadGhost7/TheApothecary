using System.Windows;
using System.Windows.Controls;
using TheApothecary.Models;

namespace TheApothecary.Views
{
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                PasswordBox.Password.Length < 6)
            {
                MessageBox.Show("Пожалуйста, заполните все поля корректно. Пароль должен быть не менее 6 символов.",
                              "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка регистрации",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            
            var newUser = new User
            {
                Username = $"{FirstNameTextBox.Text} {LastNameTextBox.Text}",
                Email = EmailTextBox.Text,
                Password = PasswordBox.Password,
                Role = ((ComboBoxItem)RoleComboBox.SelectedItem).Content.ToString() == "Покупатель" ?
                      UserRole.Customer : UserRole.Employee
            };

            MessageBox.Show($"Регистрация успешна! Добро пожаловать в The Apothecary, {newUser.Username}!",
                          "Успешная регистрация", MessageBoxButton.OK, MessageBoxImage.Information);

            this.DialogResult = true;
            this.Close();
        }
    }
}