namespace MeadowPeaks;
internal static class Program
{
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
            var session=Composition.CreateSession(store);
            StartupDiagnostics.Write("MeadowPeaks-startup.txt","Profile loaded. Building game window...");
            using var soundtrack=new MeadowSoundtrack();
            using var window=new GameWindow(session,new MeadowArtwork(),soundtrack,smoke,new EffectScene(),checking);
            Application.Run(window);
        }
        catch(Exception e) { StartupDiagnostics.Error(e,smoke||checking); }
    }
}
