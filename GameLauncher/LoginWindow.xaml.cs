using System.Diagnostics;
using System.Windows.Navigation;

using System.Windows;
using System.Windows.Controls;


namespace GameLauncher
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordTextBox.Password;
            Debug.WriteLine(email, password);
            if (!email.Equals("") || !password.Equals(""))
            {
                MainWindow window = new MainWindow();
                window.Show();
                Close();
            }
            
            
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // Open the default web browser with the specified URL
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true; // Prevent the default navigation behavior
        }
    }
}
