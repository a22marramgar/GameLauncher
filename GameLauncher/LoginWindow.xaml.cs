using System.Diagnostics;
using System.Windows.Navigation;

using System.Windows;
using System.Net.Http;
using System.Text;
using System;
using Newtonsoft.Json;

namespace GameLauncher
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly HttpClient httpClient;
        public LoginWindow()
        {
            InitializeComponent();
            httpClient = new HttpClient();
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            WarningIcon.Visibility = Visibility.Hidden;
            WarningText.Visibility = Visibility.Hidden;
            StartLoading();
            string email = EmailTextBox.Text;
            string password = PasswordTextBox.Password;
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                Login(email, password);
            }
            else
            {
                StopLoading();
                WarningIcon.Visibility = Visibility.Visible;
                WarningText.Visibility = Visibility.Visible;
                WarningText.Margin = new Thickness(76,140,0,0);
                WarningText.Text = "All the fields must be filled";
            }
            
            
        }
        private async void Login(string email, string password)
        {
            try
            {
                // Create the request data
                var requestData = new { email = email, password = password };
                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Make the HTTP POST request to your API endpoint
                HttpResponseMessage response = await httpClient.PostAsync("http://localhost:3666/users/login", content);
                StopLoading();
                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Handle successful login here
                    MainWindow window = new MainWindow(email + ";" + password);
                    window.Show();
                    Close();
                }
                else
                {
                    // Handle unsuccessful login here
                    Debug.WriteLine("Login failed: " + response.StatusCode);
                    WarningIcon.Visibility = Visibility.Visible;
                    WarningText.Visibility = Visibility.Visible;
                    WarningText.Margin = new Thickness(76, 123, 0, 0);
                    WarningText.Text = "We can't verify your account with this information";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                StopLoading();
                WarningIcon.Visibility = Visibility.Visible;
                WarningText.Visibility = Visibility.Visible;
                WarningText.Margin = new Thickness(76, 123, 0, 0);
                WarningText.Text = "It seems the servers are unavailable right now";
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

        private void StartLoading()
        {
            LoadingIcon.Visibility = Visibility.Visible;
            EmailText.Visibility = Visibility.Hidden;
            PasswordText.Visibility = Visibility.Hidden;
            EmailTextBox.Visibility = Visibility.Hidden;
            PasswordTextBox.Visibility = Visibility.Hidden;
            LoginButton.Visibility = Visibility.Hidden;
            RegisterText.Visibility = Visibility.Hidden;
        }

        private void StopLoading()
        {
            LoadingIcon.Visibility = Visibility.Hidden;
            EmailText.Visibility = Visibility.Visible;
            PasswordText.Visibility = Visibility.Visible;
            EmailTextBox.Visibility = Visibility.Visible;
            PasswordTextBox.Visibility = Visibility.Visible;
            LoginButton.Visibility = Visibility.Visible;
            RegisterText.Visibility = Visibility.Visible;
        }
    }
}
