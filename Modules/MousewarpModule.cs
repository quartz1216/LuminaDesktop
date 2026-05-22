using System;
using System.Threading;
using System.Threading.Tasks;

namespace LuminaDesktop.Modules;

public class MousewarpModule : IDisposable
{
    private bool _isRunning;
    private Task? _monitorTask;
    private CancellationTokenSource? _cts;

    public void Start()
    {
        if (_isRunning) return;

        _isRunning = true;
        _cts = new CancellationTokenSource();
        _monitorTask = Task.Run(() => MonitorLoop(_cts.Token));
    }

    public void Stop()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _cts?.Cancel();
        _monitorTask?.Wait(500); // Wait gracefully
        _cts?.Dispose();
        _cts = null;
    }

    private void MonitorLoop(CancellationToken token)
    {
        IntPtr activeHwnd = IntPtr.Zero;

        while (!token.IsCancellationRequested)
        {
            Thread.Sleep(100);

            // Alt key is pressed
            if ((Win32.GetAsyncKeyState(Win32.VK_MENU) & 0x8000) != 0)
            {
                // Wait until Alt key is released
                while ((Win32.GetAsyncKeyState(Win32.VK_MENU) & 0x8000) != 0)
                {
                    if (token.IsCancellationRequested) return;
                    Thread.Sleep(50);
                }

                // Alt key released, window changed?
                IntPtr hwnd = Win32.GetForegroundWindow();
                if (activeHwnd != hwnd && hwnd != IntPtr.Zero)
                {
                    MoveMouseToCenter(hwnd);
                    activeHwnd = Win32.GetForegroundWindow();
                }
            }
        }
    }

    private void MoveMouseToCenter(IntPtr hwnd)
    {
        if (Win32.GetWindowRect(hwnd, out Win32.RECT rect))
        {
            int x = (rect.Left + rect.Right) / 2;
            int y = (rect.Top + rect.Bottom) / 2;
            Win32.SetCursorPos(x, y);
        }
    }

    public void Dispose()
    {
        Stop();
    }
}

