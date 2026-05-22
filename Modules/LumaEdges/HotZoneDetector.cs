using System.Drawing;

namespace LuminaDesktop.Modules.LumaEdges;

public static class HotZoneDetector
{
    public static HotZone Detect(Point position, Rectangle bounds, int thickness)
    {
        if (thickness <= 0)
        {
            return HotZone.None;
        }

        var x = position.X;
        var y = position.Y;

        var top = y >= bounds.Top && y < bounds.Top + thickness;
        var bottom = y < bounds.Bottom && y >= bounds.Bottom - thickness;
        var left = x >= bounds.Left && x < bounds.Left + thickness;
        var right = x < bounds.Right && x >= bounds.Right - thickness;

        if (top && left)
        {
            return HotZone.TopLeft;
        }

        if (top && right)
        {
            return HotZone.TopRight;
        }

        if (bottom && left)
        {
            return HotZone.BottomLeft;
        }

        if (bottom && right)
        {
            return HotZone.BottomRight;
        }

        if (top)
        {
            return HotZone.Top;
        }

        if (bottom)
        {
            return HotZone.Bottom;
        }

        if (left)
        {
            return HotZone.Left;
        }

        if (right)
        {
            return HotZone.Right;
        }

        return HotZone.None;
    }
}


