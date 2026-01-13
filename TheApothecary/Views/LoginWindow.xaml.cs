using System.Windows;
using System.Windows.Controls;

namespace TheApothecary.Views
{
    public partial class LoginWindow : Window
    {
        public string Username { get; private set; }
        public string Password { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();

           
            LoginTextBox.GotFocus += LoginTextBox_GotFocus;
            LoginTextBox.LostFocus += LoginTextBox_LostFocus; 
            PasswordBox.GotFocus += PasswordBox_GotFocus;
            PasswordBox.LostFocus += PasswordBox_LostFocus;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            
            string loginInput = LoginTextBox.Text; 
            string password = PasswordBox.Password;

          
            if (string.IsNullOrWhiteSpace(loginInput) || loginInput == "Логин")
            {
                MessageBox.Show("Введите логин или email", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

          
            Username = loginInput;
            Password = password;

           
            this.DialogResult = true;
            this.Close();
        }

        private void LoginTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            if (textBox.Text == "Логин") 
            {
                textBox.Text = "";
                textBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void LoginTextBox_LostFocus(object sender, RoutedEventArgs e) 
        {
            var textBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Логин"; 
                textBox.Foreground = System.Windows.Media.Brushes.Gray;
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
           
            this.DialogResult = false;
            this.Close();
        }
    }
}