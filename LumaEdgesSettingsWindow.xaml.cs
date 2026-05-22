using System.Windows;
using System.Windows.Controls;

namespace LuminaDesktop;

public partial class LumaEdgesSettingsWindow : Window
{
    public LumaEdgesSettingsWindow()
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

        sldThickness.Value = settings.LumaEdgesThickness;
        foreach (ComboBoxItem item in cmbTriggerButton.Items)
        {
            if (item.Content.ToString() == settings.LumaEdgesTriggerButton)
            {
                cmbTriggerButton.SelectedItem = item;
                break;
            }
        }

        txtTop.Text = GetZone(settings, "Top");
        txtBottom.Text = GetZone(settings, "Bottom");
        txtLeft.Text = GetZone(settings, "Left");
        txtRight.Text = GetZone(settings, "Right");
        txtTopLeft.Text = GetZone(settings, "TopLeft");
        txtTopRight.Text = GetZone(settings, "TopRight");
        txtBottomLeft.Text = GetZone(settings, "BottomLeft");
        txtBottomRight.Text = GetZone(settings, "BottomRight");
    }

    private string GetZone(AppSettings settings, string zone)
    {
        return settings.LumaEdgesZones.TryGetValue(zone, out var val) ? val : "";
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        var app = (App)System.Windows.Application.Current;
        var settings = app.CurrentSettings;

        settings.LumaEdgesThickness = (int)sldThickness.Value;
        if (cmbTriggerButton.SelectedItem is ComboBoxItem item)
        {
            settings.LumaEdgesTriggerButton = item.Content.ToString() ?? "Left";
        }

        settings.LumaEdgesZones["Top"] = txtTop.Text;
        settings.LumaEdgesZones["Bottom"] = txtBottom.Text;
        settings.LumaEdgesZones["Left"] = txtLeft.Text;
        settings.LumaEdgesZones["Right"] = txtRight.Text;
        settings.LumaEdgesZones["TopLeft"] = txtTopLeft.Text;
        settings.LumaEdgesZones["TopRight"] = txtTopRight.Text;
        settings.LumaEdgesZones["BottomLeft"] = txtBottomLeft.Text;
        settings.LumaEdgesZones["BottomRight"] = txtBottomRight.Text;

        app.ApplySettings(settings);
        
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
