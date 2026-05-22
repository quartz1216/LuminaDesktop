using System;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace LuminaDesktop;

public class TrayIconManager : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private MainWindow? _settingsWindow;

    public TrayIconManager()
    {
        _notifyIcon = new NotifyIcon
        {
            Visible = true,
            Text = "LuminaDesktop"
        };
        
        try 
        {
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrEmpty(exePath))
            {
                _notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
            }
        }
        catch 
        {
        }

        if (_notifyIcon.Icon == null)
        {
            _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
        }

        var contextMenu = new ContextMenuStrip();
        
        var settingsItem = new ToolStripMenuItem("Settings...");
        settingsItem.Click += OnSettingsClicked;
        contextMenu.Items.Add(settingsItem);
        
        contextMenu.Items.Add(new ToolStripSeparator());
        
        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (s, e) => Application.Current.Shutdown();
        contextMenu.Items.Add(exitItem);

        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += OnSettingsClicked;
    }

    private void OnSettingsClicked(object? sender, EventArgs e)
    {
        if (_settingsWindow == null)
        {
            _settingsWindow = new MainWindow();
            _settingsWindow.Closed += (s, args) => _settingsWindow = null;
            _settingsWindow.Show();
        }
        else
        {
            if (_settingsWindow.WindowState == WindowState.Minimized)
            {
                _settingsWindow.WindowState = WindowState.Normal;
            }
            _settingsWindow.Activate();
        }
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}

