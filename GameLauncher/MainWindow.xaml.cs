﻿using Robot_Escape.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;

namespace GameLauncher
{
    enum LauncherStatus{
        ready,
        failed,
        downloadingGame,
        downloadingUpdate
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string rootPath;
        private string versionFile;
        private string gameZip;
        private string gameExe;
        private string _login;

        private LauncherStatus _status;
        
        internal LauncherStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case LauncherStatus.ready:
                        PlayButton.Opacity = 1;
                        PlayButton.Content = "Start Game";
                        break;
                    case LauncherStatus.failed:
                        PlayButton.Opacity = 1;
                        PlayButton.Content = "Update Failed - Retry";
                        break;
                    case LauncherStatus.downloadingGame:
                        PlayButton.Opacity = 0.6;
                        PlayButton.Content = "Downloading Game";
                        break;
                    case LauncherStatus.downloadingUpdate:
                        PlayButton.Opacity = 0.6;
                        PlayButton.Content = "Downloading Update";
                        break;
                    default:
                        break;
                }
            }
        }
        public MainWindow(string login)
        {
            InitializeComponent();
            _login = login;
            WelcomeText.Text = login.Split(";")[0];
            rootPath = Directory.GetCurrentDirectory();
            versionFile = Path.Combine(rootPath,"version.txt");
            gameZip = Path.Combine(rootPath, "Build.zip");
            gameExe = Path.Combine(rootPath, "Build", "ProyectoFinal2.exe");
        }

        private void CheckForUpdates()
        {
            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));
                VersionText.Text = localVersion.ToString();

                try
                {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
                    WebClient webClient = new WebClient();
#pragma warning restore SYSLIB0014 // Type or member is obsolete
                    Version onlineVersion = new Version(webClient.DownloadString("https://onedrive.live.com/download?resid=82A5D8627B8C73F6%214113&authkey=!ACpSW6dtbU-aZXc"));

                    if (onlineVersion.IsDifferentThan(localVersion))
                    {
                        InstallGameFiles(true, onlineVersion);
                    }
                    else
                    {
                        Status = LauncherStatus.ready;
                    }
                }
                catch (Exception ex)
                {
                    Status = LauncherStatus.failed;
                    MessageBox.Show($"Error checking for game updates: {ex}");
                }
            }
            else
            {
                InstallGameFiles(false, Version.zero);
            }
        }

        private void InstallGameFiles(bool _isUpdate, Version _onlineVersion)
        {
            try
            {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
                WebClient webClient = new WebClient();
#pragma warning restore SYSLIB0014 // Type or member is obsolete
                if (_isUpdate)
                {
                    Status = LauncherStatus.downloadingUpdate;
                }
                else
                {
                    Status = LauncherStatus.downloadingGame;
                    _onlineVersion = new Version(webClient.DownloadString("https://onedrive.live.com/download?resid=82A5D8627B8C73F6%214113&authkey=!ACpSW6dtbU-aZXc"));
                }
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).         
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
                webClient.DownloadFileAsync(new Uri("https://onedrive.live.com/download?resid=82A5D8627B8C73F6%214114&authkey=!AOJhq3HlX-0ACgo"), gameZip, _onlineVersion);
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                MessageBox.Show($"Error installing game files: {ex}");
            }
        }

        private void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadProgress.Value = e.ProgressPercentage;
            if(Status == LauncherStatus.downloadingUpdate)
            {
                PlayButton.Content = $"Downloading Update ({e.ProgressPercentage}%)";
            }
            else
            {
                PlayButton.Content = $"Downloading Game ({e.ProgressPercentage}%)";
            }
            
        }

        private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
#pragma warning disable CS8605 // Unboxing a possibly null value.
                string onlineVersion = ((Version)e.UserState).ToString();
#pragma warning restore CS8605 // Unboxing a possibly null value.
                ZipFile.ExtractToDirectory(gameZip, rootPath, true);
                File.Delete(gameZip);

                File.WriteAllText(versionFile, onlineVersion);

                VersionText.Text = onlineVersion;
                Status = LauncherStatus.ready;
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                MessageBox.Show($"Error finishing download: {ex}");
            }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CheckForUpdates();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(File.Exists(gameExe));
            if (File.Exists(gameExe) && Status == LauncherStatus.ready)
            {
                Debug.WriteLine("Starting game");
                ProcessStartInfo startInfo = new ProcessStartInfo(gameExe);
                startInfo.WorkingDirectory = Path.Combine(rootPath, "Build");
                Process.Start(startInfo);
                File.WriteAllText("Build/ProyectoFinal2_Data/StreamingAssets/login.txt", _login);
                Close();
            }
            else if (Status == LauncherStatus.failed)
            {
                CheckForUpdates();
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reset();
            LoginWindow window = new LoginWindow();
            window.Show();
            Close();
        }
    }

    struct Version
    {
        internal static Version zero = new Version(0,0,0);

        private short major;
        private short minor;
        private short subMinor;

        internal Version(short _major, short _minor, short _subMinor)
        {
            major = _major;
            minor = _minor;
            subMinor = _subMinor;
        }
        internal Version(string version)
        {
            string[] _versionStrings = version.Split('.');
            if(_versionStrings.Length != 3 )
            {
                major = 0;
                minor = 0;
                subMinor = 0;
                return;
            }
            major = short.Parse(_versionStrings[0]);
            minor = short.Parse(_versionStrings[1]);
            subMinor = short.Parse(_versionStrings[2]);
        }

        internal bool IsDifferentThan(Version other)
        {
            if(major != other.major)
            {
                return true;
            }
            else
            {
                if (minor != other.minor)
                {
                    return true;
                }
                else
                {
                    if(subMinor != other.subMinor)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            return $"{major}.{minor}.{subMinor}";
        }
    }
}
