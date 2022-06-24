using System.Windows.Controls;
using CarWashApp.Entities;

namespace CarWashApp.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ServicesListItem.xaml.
    /// </summary>
    public partial class ServicesListItem : UserControl
    {
        public Service CurrentService { get; set; }

        public ServicesListItem(Service currentService)
        {
            CurrentService = currentService;

            InitializeComponent();
            DataContext = CurrentService;
        }
    }
}
