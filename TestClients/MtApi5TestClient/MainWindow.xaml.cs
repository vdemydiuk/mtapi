using System.Text.RegularExpressions;
using System.Windows.Input;

namespace MtApi5TestClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private ViewModel Vm { get; }

        public MainWindow()
        {
            InitializeComponent();

            Vm = new ViewModel();
            _MainLayout.DataContext = Vm;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Vm.Close();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
