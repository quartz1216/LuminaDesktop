using System.Runtime.InteropServices;

namespace LuminaDesktop.Modules.LumaEdges;

public sealed class HotEdgeForm : Form
{
    private const int WsExNoActivate = 0x08000000;
    private const int WsExToolWindow = 0x00000080;
    private const int WsExTransparent = 0x00000020;
    private const int WmGetMinMaxInfo = 0x0024;
    private const uint SwpNoSize = 0x0001;
    private const uint SwpNoMove = 0x0002;
    private const uint SwpNoActivate = 0x0010;
    private const uint SwpShowWindow = 0x0040;
    private const uint SwpNoOwnerZOrder = 0x0200;
    private static readonly nint HwndTop = new(0);
    private static readonly nint HwndTopMost = new(-1);

    private readonly Rectangle _screenBounds;
    private readonly Rectangle _edgeBounds;
    private readonly HotZone _edge;
    private readonly System.Windows.Forms.Timer _topMostTimer;
    private bool _showDebugColor;

    public HotEdgeForm(Rectangle screenBounds, Rectangle edgeBounds, HotZone edge)
    {
        _screenBounds = screenBounds;
        _edgeBounds = edgeBounds;
        _edge = edge;

        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;
        Bounds = edgeBounds;
        ApplyVisualStyle();

        _topMostTimer = new System.Windows.Forms.Timer
        {
            Interval = 250
        };
        _topMostTimer.Tick += (_, _) => KeepTopMost();
        DebugLog.Write($"Create edge={edge} edgeBounds={edgeBounds} screenBounds={screenBounds}");
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= WsExToolWindow | WsExNoActivate | WsExTransparent;
            return cp;
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        ReassertTopMost();
        _topMostTimer.Start();
    }

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);

        if (m.Msg == WmGetMinMaxInfo)
        {
            var minMaxInfo = Marshal.PtrToStructure<MINMAXINFO>(m.LParam);
            minMaxInfo.MinTrackSize.X = 1;
            minMaxInfo.MinTrackSize.Y = 1;
            Marshal.StructureToPtr(minMaxInfo, m.LParam, false);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _topMostTimer.Stop();
            _topMostTimer.Dispose();
        }

        base.Dispose(disposing);
    }

    private static Color GetDebugColor(HotZone edge)
    {
        return edge switch
        {
            HotZone.Top or HotZone.Bottom or HotZone.Left or HotZone.Right => Color.Blue,
            HotZone.TopLeft or HotZone.TopRight or HotZone.BottomLeft or HotZone.BottomRight => Color.Purple,
            _ => Color.Blue
        };
    }

    private void KeepTopMost()
    {
        ReassertTopMost();
    }

    public void ReassertTopMost()
    {
        if (IsDisposed || !IsHandleCreated)
        {
            return;
        }

        SetWindowPos(
            Handle,
            HwndTopMost,
            _edgeBounds.Left,
            _edgeBounds.Top,
            _edgeBounds.Width,
            _edgeBounds.Height,
            SwpNoActivate | SwpShowWindow | SwpNoOwnerZOrder);

        SetWindowPos(
            Handle,
            HwndTop,
            0,
            0,
            0,
            0,
            SwpNoMove | SwpNoSize | SwpNoActivate | SwpShowWindow | SwpNoOwnerZOrder);
    }

    public void SetDebugColorVisible(bool visible)
    {
        if (_showDebugColor == visible)
        {
            return;
        }

        _showDebugColor = visible;
        ApplyVisualStyle();
    }

    private void ApplyVisualStyle()
    {
        BackColor = _showDebugColor ? GetDebugColor(_edge) : Color.Black;
        Opacity = _showDebugColor ? 0.45 : 0.01;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [StructLayout(LayoutKind.Sequential)]
    private struct MINMAXINFO
    {
        public POINT Reserved;
        public POINT MaxSize;
        public POINT MaxPosition;
        public POINT MinTrackSize;
        public POINT MaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }
}


