using System;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using CarWashApp.Classes;
using CarWashApp.Entities;
using System.Threading.Tasks;

namespace CarWashApp.Windows
{
    /// <summary>
    /// Логика взаимодействия для FormateNewOrder.xaml.
    /// </summary>
    public partial class FormateNewOrder : Window
    {
        public Order CurrentOrder { get; set; }

        public FormateNewOrder(Order order)
        {
            CurrentOrder = order;
            CurrentOrder.OrderDateTime = DateTime.Now;

            InitializeComponent();
            DataContext = CurrentOrder;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ClientSelector.ItemsSource = CarWashAppEntities.Instance.Client.ToList();
            EmployeeSelector.ItemsSource = CarWashAppEntities.Instance.Employe.ToList();

            UpdateServiceLists();
        }

        private void UpdateServiceLists()
        {
            SelectedServices.ItemsSource = null;
            SelectedServices.ItemsSource = CurrentOrder.Service.ToList();

            AllServices.ItemsSource = null;
            AllServices.ItemsSource = CarWashAppEntities.Instance.Service.ToList().Except(CurrentOrder.Service);

            OrderPrice.Text = CurrentOrder.FinalPrice_Bind.ToString();
        }

        private void ClientSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            CurrentOrder.CarStateNumber = CurrentOrder.Client.CarStateNumber;
        }

        private void AddServiceToOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (AllServices.SelectedItem is Service selected && selected != null)
            {
                CurrentOrder.Service.Add(selected);

                UpdateServiceLists();
            }
        }

        private void RemoveServiceFromOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedServices.SelectedItem is Service selected && selected != null)
            {
                CurrentOrder.Service.Remove(selected);

                UpdateServiceLists();
            }
        }

        private void SaveOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (CheckToCorrect())
            {
                CarWashAppEntities.Instance.Order.Add(CurrentOrder);
                CarWashAppEntities.Instance.SaveChanges();

                MessageBox.Show("Заказ сохранен в БД.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GenerateDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentOrder.ID == 0)
            {
                MessageBox.Show("Перед формированием чека необходимо внести заказ в систему.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            else
            {
                try
                {
                    InitializeDocumentFormation();
                }

                catch (Exception ex)
                {
                    MessageBox.Show($"При формировании чека произошла ошибка:\n{ex.Message}.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private Boolean CheckToCorrect()
        {
            String errors = String.Empty;

            if (CurrentOrder.FinalPrice_Bind <= 0)
                errors += "Услуги не указаны.\n";

            else if (CurrentOrder.Employe == null)
                errors += "Не выбран сотрудник.\n";

            else if (CurrentOrder.Client == null)
                errors += "Не выбран клиент.\n";

            else if (String.IsNullOrEmpty(CurrentOrder.Address))
                errors += "Не указан адрес.\n";

            return CheckErrorsList(errors);
        }

        private Boolean CheckErrorsList(String errors)
        {
            if (errors == String.Empty)
            {
                return true;
            }

            else
            {
                MessageBox.Show($"Обнаружены ошибки:\n{errors}", "Ошибки", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }
        }

        private void InitializeDocumentFormation()
        {
            SaveFileDialog dialog = new SaveFileDialog()
            {
                Filter = "Документ Microsoft Word (*.docx)|*.docx",
                Title = "Сохранение чека",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            Boolean? result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                Task.Run(() =>
                {
                    MessageBox.Show("Начато формирование чека...", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                });

                DocumentBuilder documentBuilder = new DocumentBuilder(dialog.FileName, CurrentOrder);
                documentBuilder.CreateNewDocument();
            }
        }
    }
}
