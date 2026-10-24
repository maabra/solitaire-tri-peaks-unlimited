namespace MeadowPeaks;
internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        bool smoke = args.Contains("--smoke");
        IProfileStore store = smoke ? new MemoryProfileStore() : new JsonProfileStore(JsonProfileStore.DefaultPath);
        var session = Composition.CreateSession(store);
        Application.Run(new GameWindow(session, new MeadowArtwork(), new WindowsChime(), smoke));
    }
}
