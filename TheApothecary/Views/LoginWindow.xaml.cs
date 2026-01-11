using System.Windows;
using System.Windows.Controls;

namespace TheApothecary.Views
{
    public partial class LoginWindow : Window
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public bool DialogResult { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();

            // Инициализация плейсхолдеров
            EmailTextBox.GotFocus += EmailTextBox_GotFocus;
            EmailTextBox.LostFocus += EmailTextBox_LostFocus;
            PasswordBox.GotFocus += PasswordBox_GotFocus;
            PasswordBox.LostFocus += PasswordBox_LostFocus;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // EmailTextBox используется как поле для username
            Username = EmailTextBox.Text;
            Password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(Username))
            {
                MessageBox.Show("Введите email/логин", "Ошибка");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Введите пароль", "Ошибка");
                return;
            }

            DialogResult = true;
            Close();
        }

        private void EmailTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (EmailTextBox.Text == "Email")
            {
                EmailTextBox.Text = "";
                EmailTextBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void EmailTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                EmailTextBox.Text = "Email";
                EmailTextBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
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

        private void RegisterText_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}