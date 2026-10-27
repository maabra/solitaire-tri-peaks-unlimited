namespace MeadowPeaks;
internal static class Program
{
    // Initialize Windows application services and select normal startup or diagnostic execution.
    [STAThread]
    static void Main(string[] args)
    {
        bool smoke=args.Contains("--smoke"), checking=args.Contains("--startup-check");
        try
        {
            StartupDiagnostics.Write("MeadowPeaks-startup.txt","Starting managed game...");
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException+=(_,e)=>{StartupDiagnostics.Error(e.Exception,smoke||checking);Application.Exit();};
            ApplicationConfiguration.Initialize();
            IProfileStore store=smoke ? new MemoryProfileStore() : new JsonProfileStore(JsonProfileStore.DefaultPath);
            StartupDiagnostics.Write("MeadowPeaks-startup.txt","Profile loaded. Building game window...");
            using var singleInstance=new Mutex(true,"Local\\MeadowPeaks.SharedUI",out bool first);
            if(!smoke&&!checking&&!first){MessageBox.Show("Meadow Peaks is already open.","Meadow Peaks");return;}
            using Form window=args.Contains("--legacy-smoke") ? new GameWindow(Composition.CreateSession(store),new MeadowArtwork(),new MeadowSoundtrack(),true,new EffectScene(),checking) : new DesktopGameWindow(store,smoke,checking);
            Application.Run(window);
        }
        catch(Exception e) { StartupDiagnostics.Error(e,smoke||checking); }
    }
}
