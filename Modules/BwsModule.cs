using System;
using System.Windows;
using LuminaDesktop.Modules.Bws;

namespace LuminaDesktop.Modules;

public class BwsModule : IDisposable
{
    private KeyboardHook? _keyboardHook;
    private BwsWindow? _mainWindow;
    private bool _isRunning;

    public void Start()
    {
        if (_isRunning) return;
        _isRunning = true;

        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            _mainWindow = new BwsWindow();
            _mainWindow.Show();
            _mainWindow.Hide();
        });

        WindowManager.InitializeMruTracking();
        WorkspaceManager.Initialize();

        try
        {
            _keyboardHook = new KeyboardHook();
            _keyboardHook.AltTabOpen += KeyboardHook_AltTabOpen;
            _keyboardHook.AltReleased += KeyboardHook_AltReleased;
            _keyboardHook.EnterPressed += KeyboardHook_EnterPressed;
            _keyboardHook.EscPressed += KeyboardHook_EscPressed;
            _keyboardHook.QPressed += KeyboardHook_QPressed;
            _keyboardHook.DirectionKeyPressed += KeyboardHook_DirectionKeyPressed;
            _keyboardHook.IsSwitcherActive = () => _mainWindow != null && _mainWindow.IsSwitcherActive;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to install keyboard hook for bws: {ex.Message}");
        }
    }

    public void Stop()
    {
        if (!_isRunning) return;
        _isRunning = false;

        if (_keyboardHook != null)
        {
            _keyboardHook.Dispose();
            _keyboardHook = null;
        }

        WindowManager.ShutdownMruTracking();
        WorkspaceManager.Shutdown();

        if (_mainWindow != null)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                _mainWindow.Close();
                _mainWindow = null;
            });
        }
    }

    private void KeyboardHook_AltTabOpen(object? sender, bool isSticky)
    {
        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            _mainWindow?.ShowSwitcher(isSticky);
        }));
    }

    private void KeyboardHook_AltReleased(object? sender, EventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (_mainWindow != null && _mainWindow.IsSwitcherActive)
            {
                _mainWindow.CommitSelection();
            }
        }));
    }

    private void KeyboardHook_EnterPressed(object? sender, EventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (_mainWindow != null && _mainWindow.IsSwitcherActive)
            {
                _mainWindow.CommitSelection(true);
            }
        }));
    }

    private void KeyboardHook_EscPressed(object? sender, EventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (_mainWindow != null && _mainWindow.IsSwitcherActive)
            {
                _mainWindow.HideSwitcher();
            }
        }));
    }

    private void KeyboardHook_QPressed(object? sender, EventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (_mainWindow != null && _mainWindow.IsSwitcherActive)
            {
                _mainWindow.CloseSelection();
            }
        }));
    }

    private void KeyboardHook_DirectionKeyPressed(object? sender, MoveDirection dir)
    {
        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (_mainWindow != null && _mainWindow.IsSwitcherActive)
            {
                _mainWindow.MoveSelection(dir);
            }
        }));
    }

    public void Dispose()
    {
        Stop();
    }
}

