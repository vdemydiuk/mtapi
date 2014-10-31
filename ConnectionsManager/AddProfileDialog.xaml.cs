using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Net;

namespace ConnectionsManager
{
    /// <summary>
    /// Interaction logic for AddProfileDialog.xaml
    /// </summary>
    public partial class AddProfileDialog : Window
    {
//        private const string PIPE_SERVER_NAME = "localhost(pipe)";
        private const string LOCALHOST_NAME = "localhost";

        public string ProfileName { get; set; }

        public static readonly DependencyProperty HostProperty = DependencyProperty.Register("Host", typeof(string), typeof(AddProfileDialog));
        public string Host 
        {
            get { return (string)this.GetValue(HostProperty); }
            set { this.SetValue(HostProperty, value); }
        }

        public string Port { get; set; }

        public DelegateCommand OkCommand { get; set; }
        
        public List<string> HostList { get; set; }

        public AddProfileDialog(Window owner)
        {
            InitializeComponent();

            this.Owner = owner;

            OkCommand = new DelegateCommand(ExecuteOk, CanExecuteOk);
            _MainLayout.DataContext = this;

            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

            IPHostEntry ips = Dns.GetHostEntry(Dns.GetHostName());

            HostList = new List<string>();
            HostList.Add(string.Empty);
            HostList.Add(LOCALHOST_NAME);

            if (ips != null)
            {
                foreach (IPAddress ipAddress in ips.AddressList)
                {
                    HostList.Add(ipAddress.ToString());
                }
            }

            Host = HostList.First();
        }

        private bool CanExecuteOk(object o)
        {
            return string.IsNullOrEmpty(ProfileName) == false
                && string.IsNullOrEmpty(Port) == false;
        }

        private void ExecuteOk(object o)
        {
            this.DialogResult = true;
            Close();
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9]+");
            return !regex.IsMatch(text);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OkCommand.RaiseCanExecuteChanged();
        }
    }
}
