using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LuminaDesktop;

public class AppSettings
{
    public bool StartWithWindows { get; set; } = false;
    public bool StartAsAdmin { get; set; } = false;

    // Modules
    public bool IsMousewarpEnabled { get; set; } = true;
    public bool IsBwsEnabled { get; set; } = true;
    public bool IsLumaEdgesEnabled { get; set; } = true;

    // LumaEdges specific
    public int LumaEdgesThickness { get; set; } = 2;
    public string LumaEdgesTriggerButton { get; set; } = "Left";
    
    public Dictionary<string, string> LumaEdgesZones { get; set; } = new()
    {
        { "Top", "Ctrl+Alt+Up" },
        { "Bottom", "Ctrl+Alt+Down" },
        { "Left", "Alt+Left" },
        { "Right", "Alt+Right" },
        { "TopLeft", "Win+Tab" },
        { "TopRight", "Alt+F4" },
        { "BottomLeft", "Ctrl+Esc" },
        { "BottomRight", "Win+D" }
    };
}

