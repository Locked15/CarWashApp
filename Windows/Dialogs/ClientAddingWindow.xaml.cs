using System;
using System.Linq;
using System.Windows;
using CarWashApp.Entities;

namespace CarWashApp.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для ClientAddingWindow.xaml
    /// </summary>
    public partial class ClientAddingWindow : Window
    {
        public Client CreatingClient { get; set; }

        public ClientAddingWindow(Client client)
        {
            CreatingClient = client ?? new Client();

            InitializeComponent();
            DataContext = CreatingClient;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CarTypeComboBox.ItemsSource = CarWashAppEntities.Instance.CarType.ToList();
            CarTypeComboBox.SelectedItem = CreatingClient.CarType;

            if (CreatingClient.ID != 0)
            {
                ClientAddButton.Content = "Сохранить клиента";
                Title = "Изменение данных о клиенте";
            }
        }

        private void ComboBoxSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CarTypeComboBox.SelectedItem is CarType type && type != null)
            {
                CreatingClient.CarType = type;
                CreatingClient.CarType.ID = type.ID;
            }
        }

        private void ClientAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(CarStateNumberTextBox.Text) || String.IsNullOrEmpty(ClientFullNameTextBox.Text) || String.IsNullOrEmpty(ClientPhoneNumberTextBox.Text))
            {
                MessageBox.Show("Введены некорректная данные.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            else
            {
                DialogResult = true;

                Close();
            }
        }
    }
}
