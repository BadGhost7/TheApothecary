using System;
using System.Windows;
using System.Windows.Controls;
using TheApothecary.Data;
using TheApothecary.Models;
using System.Linq;

namespace TheApothecary.Views
{
    public partial class PrescriptionRequestsWindow : Window
    {
        private PharmacyDbContext db;
        private User currentEmployee;
        private PrescriptionRequest selectedRequest;

        public PrescriptionRequestsWindow(User employee)
        {
            InitializeComponent();
            db = PharmacyDbContext.Instance;
            currentEmployee = employee;

            LoadRequests();

            // Подписываемся на выбор элемента
            RequestsListView.SelectionChanged += RequestsListView_SelectionChanged;
        }

        private void LoadRequests()
        {
            try
            {
                var requests = db.GetPendingRequests();
                RequestsListView.ItemsSource = requests;

                StatusText.Text = $"Найдено запросов: {requests.Count}";

                if (requests.Count == 0)
                {
                    StatusText.Text = "Нет запросов на проверку";
                    SelectedRequestPanel.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка загрузки: {ex.Message}";
            }
        }

        private void RequestsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedRequest = RequestsListView.SelectedItem as PrescriptionRequest;

            if (selectedRequest != null)
            {
                SelectedRequestText.Text = $"Пользователь: {selectedRequest.UserName}\n" +
                                         $"Лекарство: {selectedRequest.MedicineName}\n" +
                                         $"Дата запроса: {selectedRequest.RequestDate:dd.MM.yyyy HH:mm}";

                SelectedRequestPanel.Visibility = Visibility.Visible;
                NotesTextBox.Text = "";
            }
            else
            {
                SelectedRequestPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRequest == null) return;

            bool success = db.UpdatePrescriptionStatus(
                selectedRequest.Id,
                PrescriptionStatus.Approved,
                currentEmployee.Id,
                NotesTextBox.Text
            );

            if (success)
            {
                MessageBox.Show($"✅ Рецепт для {selectedRequest.UserName} одобрен!\n\n" +
                               $"Лекарство: {selectedRequest.MedicineName}\n" +
                               $"Пользователь получит уведомление.",
                               "Рецепт одобрен",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);

                LoadRequests();
                SelectedRequestPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("❌ Не удалось обновить статус",
                               "Ошибка",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRequest == null) return;

            if (string.IsNullOrWhiteSpace(NotesTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, укажите причину отказа",
                               "Требуется примечание",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            bool success = db.UpdatePrescriptionStatus(
                selectedRequest.Id,
                PrescriptionStatus.Rejected,
                currentEmployee.Id,
                NotesTextBox.Text
            );

            if (success)
            {
                MessageBox.Show($"❌ Рецепт для {selectedRequest.UserName} отклонен.\n\n" +
                               $"Причина: {NotesTextBox.Text}\n" +
                               $"Пользователь получит уведомление.",
                               "Рецепт отклонен",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);

                LoadRequests();
                SelectedRequestPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadRequests();
        }
    }
}