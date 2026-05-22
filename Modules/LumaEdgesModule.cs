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

            AddHotEdge(bounds, new System.Drawing.Rectangle(bounds.Left, bounds.Top, bounds.Width, t), HotZone.Top);
            AddHotEdge(bounds, new System.Drawing.Rectangle(bounds.Left, bounds.Bottom - t, bounds.Width, t), HotZone.Bottom);
            AddHotEdge(bounds, new System.Drawing.Rectangle(bounds.Left, bounds.Top, t, bounds.Height), HotZone.Left);
            AddHotEdge(bounds, new System.Drawing.Rectangle(bounds.Right - t, bounds.Top, t, bounds.Height), HotZone.Right);
        }

        ReassertHotEdges();
    }

    private void AddHotEdge(System.Drawing.Rectangle screenBounds, System.Drawing.Rectangle edgeBounds, HotZone edge)
    {
        var form = new HotEdgeForm(screenBounds, edgeBounds, edge, SettingsService.Load, msg => { });
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
}

