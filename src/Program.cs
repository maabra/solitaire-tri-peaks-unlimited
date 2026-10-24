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
        using var soundtrack = new MeadowSoundtrack();
        Application.Run(new GameWindow(session, new MeadowArtwork(), soundtrack, smoke, new EffectScene()));
    }
}
