namespace MeadowPeaks;

internal static class StartupDiagnostics
{
    public static void Write(string name, string text)
    {
        foreach (string directory in new[] { Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MeadowPeaks") })
        {
            try { Directory.CreateDirectory(directory); File.WriteAllText(Path.Combine(directory,name),text); return; }
            catch(Exception e) when(e is IOException or UnauthorizedAccessException) { }
        }
    }
    public static void Error(Exception error, bool checking)
    {
        Write("MeadowPeaks-error.txt", DateTimeOffset.Now+Environment.NewLine+error);
        Environment.ExitCode=1;
        if(!checking) MessageBox.Show("Meadow Peaks could not continue. Your saved progress has not been reset.\n\n"+error.Message+"\n\nDetails: MeadowPeaks-error.txt beside the game (or in Local AppData/MeadowPeaks).", "Meadow Peaks - launch problem", MessageBoxButtons.OK,MessageBoxIcon.Error);
    }
}
