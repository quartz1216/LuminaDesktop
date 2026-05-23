using System.Windows;
using Microsoft.Win32;

namespace LuminaDesktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        LoadSettings();
    }

    private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            this.DragMove();
    }

    private void LoadSettings()
    {
        var app = (App)System.Windows.Application.Current;
        var settings = app.CurrentSettings;

        chkStartWithWindows.IsChecked = settings.StartWithWindows;
        chkStartAsAdmin.IsChecked = settings.StartAsAdmin;
        chkMousewarp.IsChecked = settings.IsMousewarpEnabled;
        chkBws.IsChecked = settings.IsBwsEnabled;
        chkLumaEdges.IsChecked = settings.IsLumaEdgesEnabled;
        chkMouseGestures.IsChecked = settings.IsMouseGesturesEnabled;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        var app = (App)System.Windows.Application.Current;
        var settings = app.CurrentSettings;

        settings.StartWithWindows = chkStartWithWindows.IsChecked == true;
        settings.StartAsAdmin = chkStartAsAdmin.IsChecked == true;
        settings.IsMousewarpEnabled = chkMousewarp.IsChecked == true;
        settings.IsBwsEnabled = chkBws.IsChecked == true;
        settings.IsLumaEdgesEnabled = chkLumaEdges.IsChecked == true;
        settings.IsMouseGesturesEnabled = chkMouseGestures.IsChecked == true;

        app.ApplySettings(settings);
        UpdateStartupRegistry(settings.StartWithWindows);

        Close();
    }

    private void BtnLumaEdgesSettings_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new LumaEdgesSettingsWindow();
        settingsWindow.Owner = this;
        settingsWindow.ShowDialog();
    }

    private void BtnMouseGesturesSettings_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new MouseGestureSettingsWindow();
        settingsWindow.Owner = this;
        settingsWindow.ShowDialog();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void UpdateStartupRegistry(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            var appName = "LuminaDesktop";
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
            
            if (exePath == null) return;

            if (enable)
            {
                key?.SetValue(appName, $"\"{exePath}\"");
            }
            else
            {
                key?.DeleteValue(appName, false);
            }
        }
        catch 
        {
            // Ignore errors when failing to update registry
        }
    }
}
