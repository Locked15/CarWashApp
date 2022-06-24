using System.Windows;
using System.Windows.Controls;

namespace CarWashApp.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для EmployeAddingWindow.xaml
    /// </summary>
    public partial class EmployeAddingWindow : Window
    {
        private bool IsEmpty = false;
        public bool IsPressed { get; private set; } = false;

        public EmployeAddingWindow()
        {
            InitializeComponent();
        }

        private void EmployeAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsEmpty)
            {
                IsPressed = true;
                this.Close();
            }
        }

        private void IsEmptyCheck()
        {
            if (EmployeFirstName.Text.Length > 1 &
                EmployeLastName.Text.Length > 1 &
                EmployePatranymic.Text.Length > 1 &
                EmployePhoneNumber.Text.Length > 10 &
                RoleComboBox.SelectedItem != null )
            {
                IsEmpty = true;
                EmployeAddButton.IsEnabled = true;
            }
            else
            {
                EmployeAddButton.IsEnabled = false;
            }
        }

        private void TextBoxTextChanging(object sender, TextChangedEventArgs e)
        {
            IsEmptyCheck();
        }

        private void ComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IsEmptyCheck();
        }
    }
}
