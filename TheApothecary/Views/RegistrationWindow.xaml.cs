using System.Windows;
using System.Windows.Controls;
using TheApothecary.Models;

namespace TheApothecary.Views
{
    public partial class RegistrationWindow : Window
    {
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; }
        public UserRole Role { get; private set; }

        public RegistrationWindow()
        {
            InitializeComponent();

            // Подписываемся на события
            EmailTextBox.GotFocus += TextBox_GotFocus;
            EmailTextBox.LostFocus += TextBox_LostFocus;
            FirstNameTextBox.GotFocus += TextBox_GotFocus;
            FirstNameTextBox.LostFocus += TextBox_LostFocus;
            LastNameTextBox.GotFocus += TextBox_GotFocus;
            LastNameTextBox.LostFocus += TextBox_LostFocus;
            PasswordBox.GotFocus += PasswordBox_GotFocus;
            PasswordBox.LostFocus += PasswordBox_LostFocus;
            ConfirmPasswordBox.GotFocus += PasswordBox_GotFocus;
            ConfirmPasswordBox.LostFocus += PasswordBox_LostFocus;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка данных
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text) || EmailTextBox.Text == "Введите ваш email")
            {
                MessageBox.Show("Введите email", "Ошибка");
                return;
            }

            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) || FirstNameTextBox.Text == "Имя")
            {
                MessageBox.Show("Введите имя", "Ошибка");
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Введите пароль", "Ошибка");
                return;
            }

            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка");
                return;
            }

            // Устанавливаем значения
            Username = FirstNameTextBox.Text; // Имя как username
            Email = EmailTextBox.Text;
            Password = PasswordBox.Password;

            // Определяем роль
            if (RoleComboBox.SelectedIndex == 0) // Покупатель
                Role = UserRole.Customer;
            else // Сотрудник
                Role = UserRole.Employee;

            DialogResult = true;
            Close();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            if (textBox.Tag != null && textBox.Text == textBox.Tag.ToString())
            {
                textBox.Text = "";
                textBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(textBox.Text) && textBox.Tag != null)
            {
                textBox.Text = textBox.Tag.ToString();
                textBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            if (passwordBox == PasswordBox)
                PasswordPlaceholder.Visibility = Visibility.Collapsed;
            else if (passwordBox == ConfirmPasswordBox)
                ConfirmPasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            if (passwordBox == PasswordBox && string.IsNullOrEmpty(passwordBox.Password))
                PasswordPlaceholder.Visibility = Visibility.Visible;
            else if (passwordBox == ConfirmPasswordBox && string.IsNullOrEmpty(passwordBox.Password))
                ConfirmPasswordPlaceholder.Visibility = Visibility.Visible;
        }
    }
}