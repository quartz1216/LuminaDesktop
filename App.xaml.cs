using System.Configuration;
using System.Data;
using System.Windows;
using LuminaDesktop.Modules;
using LuminaDesktop.Modules.LumaEdges;

namespace LuminaDesktop;

public partial class App : System.Windows.Application
{
    private TrayIconManager? _trayIconManager;

    public MousewarpModule Mousewarp { get; } = new MousewarpModule();
    public BwsModule Bws { get; } = new BwsModule();
    public LumaEdgesModule LumaEdges { get; } = new LumaEdgesModule();

    public AppSettings CurrentSettings { get; private set; } = new AppSettings();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        CurrentSettings = SettingsService.Load();

        if (CurrentSettings.StartAsAdmin && !IsAdministrator())
        {
            RestartAsAdmin();
            return;
        }

        _trayIconManager = new TrayIconManager();
        
        ApplySettings(CurrentSettings);
    }

    private static bool IsAdministrator()
    {
        var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
        var principal = new System.Security.Principal.WindowsPrincipal(identity);
        return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
    }

    private static void RestartAsAdmin()
    {
        var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrEmpty(exePath)) return;

        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = true,
            Verb = "runas"
        };

        try
        {
            System.Diagnostics.Process.Start(startInfo);
        }
        catch
        {
            // User cancelled UAC prompt
        }

        System.Windows.Application.Current.Shutdown();
    }

    public void ApplySettings(AppSettings settings)
    {
        CurrentSettings = settings;
        SettingsService.Save(settings);

        if (settings.IsMousewarpEnabled) Mousewarp.Start(); else Mousewarp.Stop();
        if (settings.IsBwsEnabled) Bws.Start(); else Bws.Stop();
        
        if (settings.IsLumaEdgesEnabled) 
        {
            LumaEdges.Start(); 
            LumaEdges.RebuildHotEdges();
        }
        else 
        {
            LumaEdges.Stop();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Mousewarp.Stop();
        Bws.Stop();
        LumaEdges.Stop();

        _trayIconManager?.Dispose();
        base.OnExit(e);
    }
}


