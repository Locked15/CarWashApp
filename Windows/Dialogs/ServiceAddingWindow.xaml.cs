using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CarWashApp.Entities;

namespace CarWashApp.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для ServiceAddingWindow.xaml.
    /// </summary>
    public partial class ServiceAddingWindow : Window
    {
        public Service CreatingService { get; set; }

        public ServiceAddingWindow(Service service)
        {
            CreatingService = service ?? new Service();

            InitializeComponent();
            DataContext = CreatingService;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ServiceTypeComboBox.ItemsSource = CarWashAppEntities.Instance.ServiceType.ToList();
            ServiceTypeComboBox.SelectedItem = CreatingService.ServiceType;

            if (CreatingService.ID != 0)
            {
                ServiceAddButton.Content = "Сохранить услугу";
                Title = "Изменение услуги";
            }
        }

        private void ServiceTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServiceTypeComboBox.SelectedItem is ServiceType type && type != null)
            {
                CreatingService.ServiceType = type;
                CreatingService.ServicesType = type.ID;
            }
        }

        private void ServiceAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(ServicePriceTextBox.Text) || CreatingService.Price < 0)
            {
                MessageBox.Show("Некорректная стоимость услуги.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            else if (String.IsNullOrEmpty(CreatingService.Title))
            {
                MessageBox.Show("Некорректное название услуги.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            else
            {
                DialogResult = true;

                Close();
            }
        }
    }
}
