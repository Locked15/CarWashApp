using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using CarWashApp.Entities;
using CarWashApp.UserControls;
using CarWashApp.Windows.Dialogs;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Windows.Controls;

namespace CarWashApp.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Client> Clients { get; private set; }

        public List<Employe> Employes { get; private set; }

        public List<Service> Services { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            ClientsListComplection();
            EmployesListComplection();
            ServicesListComplection();
        }

        #region Область: Методы.

        private void ClientsListComplection(List<Client> searchResult = null)
        {
            Clients = searchResult ?? CarWashAppEntities.Instance.Client.ToList();

            ClientsList.Items.Clear();
            ClientsList.SelectedIndex = 0;

            foreach (Client client in Clients)
            {
                ClientsListItem item = new ClientsListItem(client);

                item.ClientEdit.Click += (sender, e) =>
                {
                    ClientMenuButton_Click(ClientEditButton, e);
                };
                item.ClientRemove.Click += (sender, e) =>
                {
                    ClientMenuButton_Click(ClientRemoveButton, e);
                };

                GetOptimalWidth(item);
                ClientsList.Items.Add(item);
            }
        }

        private void EmployesListComplection()
        {
            Employes = CarWashAppEntities.Instance.Employe.ToList();

            EmployesList.Items.Clear();
            EmployesList.SelectedIndex = 0;

            foreach (Employe employe in Employes)
            {
                EmployesListItem item = new EmployesListItem(employe);

                item.EmployeEdit.Click += UserControlContextMenuButtonClick;
                item.EmployeRemove.Click += (sender, e) =>
                {
                    EmployeMenuButton_Click(EmployeRemoveButton, e);
                };

                GetOptimalWidth(item);
                EmployesList.Items.Add(item);
            }
        }

        private void ServicesListComplection()
        {
            Services = CarWashAppEntities.Instance.Service.ToList();

            ServicesList.Items.Clear();
            ServicesList.SelectedIndex = 0;

            foreach (Service service in Services)
            {
                ServicesListItem item = new ServicesListItem(service);

                item.EmployeEdit.Click += (sender, e) =>
                {
                    ServicesMenuButton_Click(ServicesEditButton, e);
                };
                item.EmployeRemove.Click += (sender, e) =>
                {
                    ServicesMenuButton_Click(ServicesRemoveButton, e);
                };

                GetOptimalWidth(item);
                ServicesList.Items.Add(item);
            }
        }

        private void GetOptimalWidth(UserControl item)
        {
            try
            {
                item.Width = ClientsList.ActualWidth - 20;
            }
            catch
            {
                item.Width = ClientsList.Width - 20;
            }
        }

        #endregion

        #region Область: События.

        private void ClientListSelectonChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Client selectedClient = (ClientsList.SelectedItem as ClientsListItem)?.CurrentClient;

            if (selectedClient != null)
            {
                Int64 phone = Convert.ToInt64(selectedClient.PhoneNumber);

                CarTypeImage.Source = new BitmapImage(new Uri($"/Resources/{selectedClient.CarType.Image}", UriKind.Relative));
                CarType.Text = selectedClient.CarType.CarTypeName;
                CarStateNumber.Text = selectedClient.CarStateNumber;
                ClientFullName.Text = selectedClient.ClientFullName;
                ClientPhoneNumber.Text = phone.ToString("+#-###-###-##-##");
            }
        }

        private void EmployesListSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Employe selectedEmploye = (EmployesList.SelectedItem as EmployesListItem)?.CurrentEmploye;

            if (selectedEmploye != null)
            {
                Int64 phone = Convert.ToInt64(selectedEmploye.PhoneNumber);

                EmployeLastName.Text = selectedEmploye.LastName;
                EmployeFirstName.Text = selectedEmploye.FirstName;
                EmployePatronumic.Text = selectedEmploye.Patranymic;
                EmployePhoneNumber.Text = phone.ToString("+#-###-###-##-##");
                EmployeRole.Text = selectedEmploye.Role_Bind;
            }
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (sender == ClientSearchBox)
            {
                List<Client> results = null;

                if (ClientSearchBox.Text.ToLower() is var search && search.Length > 0)
                {
                    results = Clients.Where(p => p.CarStateNumber.ToLower().Contains(search)).ToList();
                }

                else
                {
                    new ToastContentBuilder()
                        .AddArgument("action", "viewConversation")
                        .AddArgument("conversationId", 9813)
                        .AddText("По вашему запросу ничего не найдено!")
                        .Show();
                }

                ClientsListComplection(results);
            }
        }

        private void ClientMenuButton_Click(object sender, RoutedEventArgs e)
        {
            Button senderButton = sender as Button;
            
            if (senderButton != null)
            {
                ClientAddingWindow window;

                // Добавление клиента:
                if (sender == ClientAddButton)
                {
                    window = new ClientAddingWindow(null);
                    Boolean? result = window.ShowDialog();

                    if (window.DialogResult.HasValue && window.DialogResult.Value)
                    {
                        CarWashAppEntities.Instance.Client.Add(window.CreatingClient);

                        CarWashAppEntities.Instance.SaveChanges();
                        ClientsListComplection();
                    }
                }

                // Удаление клиента:
                else if (sender == ClientRemoveButton)
                {
                    if (ClientsList.SelectedItem != null)
                    {
                        // Получаем данные о текущем выбранном клиенте.
                        Client selected = (ClientsList.SelectedItem as ClientsListItem).CurrentClient;
                        List<Order> allOrdersWithThisClient = CarWashAppEntities.Instance.Order.Where(o => o.Client.CarStateNumber == selected.CarStateNumber).ToList();

                        // Удаляем связанные данные и данные самого клиента.
                        allOrdersWithThisClient.ForEach(o => o.Service.Clear());
                        CarWashAppEntities.Instance.Order.RemoveRange(allOrdersWithThisClient);
                        CarWashAppEntities.Instance.Client.Remove(selected);

                        // Сохраняем изменения.
                        CarWashAppEntities.Instance.SaveChanges();
                        ClientsListComplection();
                    }

                    else
                    {
                        MessageBox.Show("Для начала выберите клиента!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }

                // Редактирование клиента:
                else if (sender == ClientEditButton)
                {
                    if (ClientsList.SelectedItem != null)
                    {
                        window = new ClientAddingWindow((ClientsList.SelectedItem as ClientsListItem)?.CurrentClient);
                        window.ShowDialog();

                        if (window.DialogResult.HasValue && window.DialogResult.Value)
                        {
                            CarWashAppEntities.Instance.SaveChanges();
                            ClientsListComplection();
                        }
                    }

                    else
                    {
                        MessageBox.Show("Для начала выберите клиента!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }

                // Формирование заказа:
                else if (sender == OrderFormationButton)
                {
                    FormateNewOrder letsFormate = new FormateNewOrder(new Order()
                    {
                        Client = (ClientsList.SelectedItem as ClientsListItem)?.CurrentClient ?? null
                    });
                    letsFormate.ShowDialog();
                }
            }
        }

        private void EmployeMenuButton_Click(object sender, RoutedEventArgs e)
        {
            // Добавление сотрудника:
            if (sender == EmployeAddButton)
            {
                EmployeAddingWindow window = new EmployeAddingWindow();

                foreach (var item in CarWashAppEntities.Instance.Role.ToList())
                {
                    window.RoleComboBox.Items.Add(item.RoleName);
                }

                window.ShowDialog();

                if (window.IsPressed)
                {
                    CarWashAppEntities.Instance.Employe.Add(new Employe
                    {
                        FirstName = window.EmployeFirstName.Text,
                        LastName = window.EmployeLastName.Text,
                        Patranymic = window.EmployePatranymic.Text,
                        PhoneNumber = window.EmployePhoneNumber.Text,
                        RoleID = window.RoleComboBox.SelectedIndex + 1,
                    });

                    CarWashAppEntities.Instance.SaveChanges();
                    EmployesListComplection();
                }
            }

            // Удаление сотрудника:
            if (sender == EmployeRemoveButton)
            {

                if (EmployesList.SelectedItem != null)
                {
                    // Получаем данные о текущем выбранном сотруднике.
                    Employe selected = (EmployesList.SelectedItem as EmployesListItem).CurrentEmploye;
                    List<Order> allOrdersWithThisEmployee = CarWashAppEntities.Instance.Order.Where(o => o.EmployeID == selected.ID).ToList();

                    // Удаляем связанные данные и данные самого сотрудника.
                    allOrdersWithThisEmployee.ForEach(o => o.Service.Clear());
                    CarWashAppEntities.Instance.Order.RemoveRange(allOrdersWithThisEmployee);
                    CarWashAppEntities.Instance.Employe.Remove(selected);

                    // Сохраняем изменения.
                    CarWashAppEntities.Instance.SaveChanges();
                    EmployesListComplection();
                }

                else
                {
                    MessageBox.Show("Для начала выберите сотрудника!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        private void ServicesMenuButton_Click(object sender, RoutedEventArgs e)
        {
            Button senderButton = sender as Button;

            if (senderButton != null)
            {
                ServiceAddingWindow window;

                // Добавление услуги:
                if (senderButton == ServicesAddButton)
                {
                    window = new ServiceAddingWindow(null);
                    window.ShowDialog();

                    if (window.DialogResult.HasValue && window.DialogResult.Value)
                    {
                        CarWashAppEntities.Instance.Service.Add(window.CreatingService);

                        CarWashAppEntities.Instance.SaveChanges();
                        ServicesListComplection();
                    }
                }

                // Редактирование услуги:
                else if (senderButton == ServicesEditButton)
                {
                    window = new ServiceAddingWindow((ServicesList.SelectedItem as ServicesListItem)?.CurrentService);
                    window.ShowDialog();

                    if (window.DialogResult.HasValue && window.DialogResult.Value)
                    {
                        CarWashAppEntities.Instance.SaveChanges();
                        ServicesListComplection();
                    }
                }

                // Удаление услуги:
                else if (senderButton == ServicesRemoveButton)
                {
                    Service selected = (ServicesList.SelectedItem as ServicesListItem)?.CurrentService;

                    CarWashAppEntities.Instance.Service.Remove(selected);
                    CarWashAppEntities.Instance.SaveChanges();

                    ServicesListComplection();

                    return;
                }
            }
        }

        private void ServicesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServicesList.SelectedItem != null && ServicesList.SelectedItem is ServicesListItem element)
            {
                ServicesName.Text = element.CurrentService.Title;
                ServicesPrice.Text = element.CurrentService.Price.ToString("0.00");
                ServicesDescription.Text = element.CurrentService.Description;
            }

            else
            {
                ServicesName.Text = String.Empty;
                ServicesPrice.Text = String.Empty;
                ServicesDescription.Text = String.Empty;
            }
        }

        private void UserControlContextMenuButtonClick(object sender, dynamic e)
        {
            
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (ClientsListItem item in ClientsList.Items)
            {
                GetOptimalWidth(item);
            }
        }

        #endregion
    }
}
