using System.Windows;
using System.Windows.Controls;

namespace TheApothecary.Views
{
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && textBox.Text == textBox.Tag?.ToString())
            {
                textBox.Text = "";
                textBox.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(44, 62, 80));
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = textBox.Tag?.ToString();
                textBox.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(102, 102, 102));
            }
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            if (passwordBox != null)
            {
                if (passwordBox.Name == "PasswordBox")
                    PasswordPlaceholder.Visibility = Visibility.Collapsed;
                else if (passwordBox.Name == "ConfirmPasswordBox")
                    ConfirmPasswordPlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            if (passwordBox != null)
            {
              
                if (passwordBox.Name == "PasswordBox")
                {
                    PasswordPlaceholder.Visibility =
                        string.IsNullOrEmpty(passwordBox.Password) ?
                        Visibility.Visible : Visibility.Collapsed;
                }
                else if (passwordBox.Name == "ConfirmPasswordBox")
                {
                    ConfirmPasswordPlaceholder.Visibility =
                        string.IsNullOrEmpty(passwordBox.Password) ?
                        Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
           
            MessageBox.Show("Регистрация выполнена!");
        }
    }
}