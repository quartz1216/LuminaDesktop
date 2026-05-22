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

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ((App)System.Windows.Application.Current).LumaEdges.SetDebugColorVisible(true);
    }

    private void Window_Closed(object sender, System.EventArgs e)
    {
        ((App)System.Windows.Application.Current).LumaEdges.SetDebugColorVisible(false);
    }

    private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            this.DragMove();
    }

    private void LoadSettings()
    {
        var settings = SettingsService.Load();
        sldThickness.Value = settings.LumaEdgesThickness;

        // Right Button
        txtRightTop.Text = GetZone(settings.LumaEdgesRightZones, "Top");
        txtRightBottom.Text = GetZone(settings.LumaEdgesRightZones, "Bottom");
        txtRightLeft.Text = GetZone(settings.LumaEdgesRightZones, "Left");
        txtRightRight.Text = GetZone(settings.LumaEdgesRightZones, "Right");
        txtRightTopLeft.Text = GetZone(settings.LumaEdgesRightZones, "TopLeft");
        txtRightTopRight.Text = GetZone(settings.LumaEdgesRightZones, "TopRight");
        txtRightBottomLeft.Text = GetZone(settings.LumaEdgesRightZones, "BottomLeft");
        txtRightBottomRight.Text = GetZone(settings.LumaEdgesRightZones, "BottomRight");

        // Left Button
        txtLeftTop.Text = GetZone(settings.LumaEdgesLeftZones, "Top");
        txtLeftBottom.Text = GetZone(settings.LumaEdgesLeftZones, "Bottom");
        txtLeftLeft.Text = GetZone(settings.LumaEdgesLeftZones, "Left");
        txtLeftRight.Text = GetZone(settings.LumaEdgesLeftZones, "Right");
        txtLeftTopLeft.Text = GetZone(settings.LumaEdgesLeftZones, "TopLeft");
        txtLeftTopRight.Text = GetZone(settings.LumaEdgesLeftZones, "TopRight");
        txtLeftBottomLeft.Text = GetZone(settings.LumaEdgesLeftZones, "BottomLeft");
        txtLeftBottomRight.Text = GetZone(settings.LumaEdgesLeftZones, "BottomRight");

        // Middle Button
        txtMiddleTop.Text = GetZone(settings.LumaEdgesMiddleZones, "Top");
        txtMiddleBottom.Text = GetZone(settings.LumaEdgesMiddleZones, "Bottom");
        txtMiddleLeft.Text = GetZone(settings.LumaEdgesMiddleZones, "Left");
        txtMiddleRight.Text = GetZone(settings.LumaEdgesMiddleZones, "Right");
        txtMiddleTopLeft.Text = GetZone(settings.LumaEdgesMiddleZones, "TopLeft");
        txtMiddleTopRight.Text = GetZone(settings.LumaEdgesMiddleZones, "TopRight");
        txtMiddleBottomLeft.Text = GetZone(settings.LumaEdgesMiddleZones, "BottomLeft");
        txtMiddleBottomRight.Text = GetZone(settings.LumaEdgesMiddleZones, "BottomRight");
    }

    private string GetZone(System.Collections.Generic.Dictionary<string, string> dict, string key)
    {
        return dict.TryGetValue(key, out var val) ? val : "";
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        var settings = SettingsService.Load();
        settings.LumaEdgesThickness = (int)sldThickness.Value;

        settings.LumaEdgesRightZones = new System.Collections.Generic.Dictionary<string, string>
        {
            { "Top", txtRightTop.Text }, { "Bottom", txtRightBottom.Text },
            { "Left", txtRightLeft.Text }, { "Right", txtRightRight.Text },
            { "TopLeft", txtRightTopLeft.Text }, { "TopRight", txtRightTopRight.Text },
            { "BottomLeft", txtRightBottomLeft.Text }, { "BottomRight", txtRightBottomRight.Text }
        };

        settings.LumaEdgesLeftZones = new System.Collections.Generic.Dictionary<string, string>
        {
            { "Top", txtLeftTop.Text }, { "Bottom", txtLeftBottom.Text },
            { "Left", txtLeftLeft.Text }, { "Right", txtLeftRight.Text },
            { "TopLeft", txtLeftTopLeft.Text }, { "TopRight", txtLeftTopRight.Text },
            { "BottomLeft", txtLeftBottomLeft.Text }, { "BottomRight", txtLeftBottomRight.Text }
        };

        settings.LumaEdgesMiddleZones = new System.Collections.Generic.Dictionary<string, string>
        {
            { "Top", txtMiddleTop.Text }, { "Bottom", txtMiddleBottom.Text },
            { "Left", txtMiddleLeft.Text }, { "Right", txtMiddleRight.Text },
            { "TopLeft", txtMiddleTopLeft.Text }, { "TopRight", txtMiddleTopRight.Text },
            { "BottomLeft", txtMiddleBottomLeft.Text }, { "BottomRight", txtMiddleBottomRight.Text }
        };

        ((App)System.Windows.Application.Current).ApplySettings(settings);
        Close();
    }

    private void sldThickness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded) return;
        var app = (App)System.Windows.Application.Current;
        app.CurrentSettings.LumaEdgesThickness = (int)e.NewValue;
        app.LumaEdges.RebuildHotEdges();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
