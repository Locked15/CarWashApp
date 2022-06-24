using System.Windows.Controls;
using CarWashApp.Entities;

namespace CarWashApp.UserControls
{
    /// <summary>
    /// Логика взаимодействия для EmployesListItem.xaml
    /// </summary>
    public partial class EmployesListItem : UserControl
    {
        public Employe CurrentEmploye { get; set; }

        public EmployesListItem(Employe employe)
        {
            CurrentEmploye = employe;
            InitializeComponent();
            DataContext = CurrentEmploye;
        }
    }
}
