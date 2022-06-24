using System.Windows.Controls;
using CarWashApp.Entities;

namespace CarWashApp.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ClientsListItem.xaml
    /// </summary>
    public partial class ClientsListItem : UserControl
    {
        public Client CurrentClient { get; set; }

        public ClientsListItem(Client client)
        {
            CurrentClient = client;
            InitializeComponent();
            DataContext = CurrentClient;
        }
    }
}
