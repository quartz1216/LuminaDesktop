using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace LuminaDesktop.Modules.LumaEdges;

public class LumaEdgesModule : IDisposable
{
    private const uint EventSystemForeground = 0x0003;
    private const uint WineventOutOfContext = 0x0000;

    private readonly List<HotEdgeForm> _hotEdgeForms = new();
    private WinEventDelegate? _foregroundEventDelegate;
    private nint _foregroundEventHook;
    private bool _isRunning;
    private SynchronizationContext? _uiContext;

    // Mouse Hook
    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_MBUTTONDOWN = 0x0207;
    private delegate nint LowLevelMouseProc(int nCode, nint wParam, nint lParam);
    private LowLevelMouseProc? _mouseProcDelegate;
    private nint _mouseHookId = nint.Zero;

    private static readonly object CooldownLock = new();
    private static readonly TimeSpan Cooldown = TimeSpan.FromMilliseconds(300);
    private static DateTimeOffset _lastTriggeredAt = DateTimeOffset.MinValue;

    public void Start()
    {
        if (_isRunning) return;
        _isRunning = true;

        _uiContext = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();

        _foregroundEventDelegate = OnForegroundChanged;
        _foregroundEventHook = SetWinEventHook(
            EventSystemForeground,
            EventSystemForeground,
            nint.Zero,
            _foregroundEventDelegate,
            0,
            0,
            WineventOutOfContext);

        _mouseProcDelegate = MouseHookCallback;
        using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
        using (var curModule = curProcess.MainModule)
        {
            if (curModule != null)
                _mouseHookId = SetWindowsHookEx(WH_MOUSE_LL, _mouseProcDelegate, GetModuleHandle(curModule.ModuleName), 0);
        }

        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
        RebuildHotEdges();
    }

    public void Stop()
    {
        if (!_isRunning) return;
        _isRunning = false;

        SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;

        if (_foregroundEventHook != nint.Zero)
        {
            UnhookWinEvent(_foregroundEventHook);
            _foregroundEventHook = nint.Zero;
        }

        if (_mouseHookId != nint.Zero)
        {
            UnhookWindowsHookEx(_mouseHookId);
            _mouseHookId = nint.Zero;
        }

        DisposeHotEdges();
    }

    public void RebuildHotEdges()
    {
        DisposeHotEdges();

        var settings = SettingsService.Load();
        var thickness = Math.Max(1, settings.LumaEdgesThickness);

        foreach (var screen in Screen.AllScreens)
        {
            var bounds = screen.Bounds;
            var t = Math.Min(thickness, Math.Max(1, Math.Min(bounds.Width, bounds.Height)));

            var left = bounds.Left;
            var top = bounds.Top;
            var right = bounds.Right;
            var bottom = bounds.Bottom;
            var width = bounds.Width;
            var height = bounds.Height;

            // Corners (t x t)
            AddHotEdge(bounds, new System.Drawing.Rectangle(left, top, t, t), HotZone.TopLeft);
            AddHotEdge(bounds, new System.Drawing.Rectangle(right - t, top, t, t), HotZone.TopRight);
            AddHotEdge(bounds, new System.Drawing.Rectangle(left, bottom - t, t, t), HotZone.BottomLeft);
            AddHotEdge(bounds, new System.Drawing.Rectangle(right - t, bottom - t, t, t), HotZone.BottomRight);

            // Edges (between corners)
            if (width > t * 2)
            {
                AddHotEdge(bounds, new System.Drawing.Rectangle(left + t, top, width - t * 2, t), HotZone.Top);
                AddHotEdge(bounds, new System.Drawing.Rectangle(left + t, bottom - t, width - t * 2, t), HotZone.Bottom);
            }
            if (height > t * 2)
            {
                AddHotEdge(bounds, new System.Drawing.Rectangle(left, top + t, t, height - t * 2), HotZone.Left);
                AddHotEdge(bounds, new System.Drawing.Rectangle(right - t, top + t, t, height - t * 2), HotZone.Right);
            }
        }

        ReassertHotEdges();
    }

    private void AddHotEdge(System.Drawing.Rectangle screenBounds, System.Drawing.Rectangle edgeBounds, HotZone edge)
    {
        var form = new HotEdgeForm(screenBounds, edgeBounds, edge);
        _hotEdgeForms.Add(form);
        form.Show();
    }

    private void DisposeHotEdges()
    {
        foreach (var form in _hotEdgeForms)
        {
            form.Close();
            form.Dispose();
        }
        _hotEdgeForms.Clear();
    }

    private void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        if (_isRunning)
        {
            RebuildHotEdges();
        }
    }

    public void SetDebugColorVisible(bool visible)
    {
        foreach (var form in _hotEdgeForms)
        {
            form.SetDebugColorVisible(visible);
        }
    }

    private void ReassertHotEdges()
    {
        foreach (var form in _hotEdgeForms)
        {
            form.ReassertTopMost();
        }
    }

    private void OnForegroundChanged(
        nint winEventHook,
        uint eventType,
        nint hwnd,
        int idObject,
        int idChild,
        uint eventThread,
        uint eventTime)
    {
        if (!_isRunning) return;

        if (_uiContext is null)
        {
            ReassertHotEdges();
            return;
        }

        _uiContext.Post(_ => ReassertHotEdges(), null);
    }

    private nint MouseHookCallback(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0)
        {
            var msg = (int)wParam;
            if (msg == WM_LBUTTONDOWN || msg == WM_RBUTTONDOWN || msg == WM_MBUTTONDOWN)
            {
                var cursorPosition = System.Windows.Forms.Cursor.Position;

                var settings = SettingsService.Load();
                var thickness = settings.LumaEdgesThickness;
                HotZone detectedZone = HotZone.None;

                foreach (var screen in Screen.AllScreens)
                {
                    var zone = HotZoneDetector.Detect(cursorPosition, screen.Bounds, thickness);
                    if (zone != HotZone.None)
                    {
                        detectedZone = zone;
                        break;
                    }
                }

                if (detectedZone != HotZone.None)
                {
                    string? hotkey = null;
                    if (msg == WM_LBUTTONDOWN)
                        hotkey = settings.LumaEdgesLeftZones.GetValueOrDefault(detectedZone.ToString());
                    else if (msg == WM_RBUTTONDOWN)
                        hotkey = settings.LumaEdgesRightZones.GetValueOrDefault(detectedZone.ToString());
                    else if (msg == WM_MBUTTONDOWN)
                        hotkey = settings.LumaEdgesMiddleZones.GetValueOrDefault(detectedZone.ToString());

                    if (!string.IsNullOrWhiteSpace(hotkey) && TryStartCooldown())
                    {
                        System.Threading.Tasks.Task.Run(async () => {
                            await System.Threading.Tasks.Task.Delay(10);
                            HotkeySender.SendDetailed(hotkey);
                        });
                        return 1; // Block click
                    }
                }
            }
        }
        return CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
    }

    private static bool TryStartCooldown()
    {
        lock (CooldownLock)
        {
            var now = DateTimeOffset.UtcNow;
            if (now - _lastTriggeredAt < Cooldown)
            {
                return false;
            }

            _lastTriggeredAt = now;
            return true;
        }
    }

    public void Dispose()
    {
        Stop();
    }

    private delegate void WinEventDelegate(
        nint winEventHook,
        uint eventType,
        nint hwnd,
        int idObject,
        int idChild,
        uint eventThread,
        uint eventTime);

    [DllImport("user32.dll")]
    private static extern nint SetWinEventHook(
        uint eventMin,
        uint eventMax,
        nint eventHookAssembly,
        WinEventDelegate eventHookCallback,
        uint processId,
        uint threadId,
        uint flags);

    [DllImport("user32.dll")]
    private static extern bool UnhookWinEvent(nint winEventHook);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, nint hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint GetModuleHandle(string lpModuleName);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public nint dwExtraInfo;
    }
}

