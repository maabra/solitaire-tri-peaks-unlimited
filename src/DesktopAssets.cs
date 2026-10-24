using System.Diagnostics;
using System.Reflection;
namespace MeadowPeaks;
internal static class DesktopAssets
{
    public static readonly string Root=Environment.GetEnvironmentVariable("MEADOW_PEAKS_CACHE") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"MeadowPeaks");
    public static async Task<string?> Runtime()
    {
        string cab=Path.Combine(AppContext.BaseDirectory,"WebViewRuntime.cab");if(!File.Exists(cab))return null;
        return await Task.Run(()=>
        {
            using var gate=new Mutex(false,"Local\\MeadowPeaks.RuntimeExtraction");bool acquired;try{acquired=gate.WaitOne(TimeSpan.FromSeconds(120));}catch(AbandonedMutexException){acquired=true;}if(!acquired)throw new TimeoutException("The game runtime is being prepared by another window.");
            try
            {
                string directory=Path.Combine(Root,"runtime-156.0.4314.42"),ready=Path.Combine(directory,"ready.txt");Directory.CreateDirectory(directory);
                if(!File.Exists(ready))
                {
                    var start=new ProcessStartInfo(Path.Combine(Environment.SystemDirectory,"expand.exe")){UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden};start.ArgumentList.Add("-F:*");start.ArgumentList.Add(cab);start.ArgumentList.Add(directory);
                    using var process=Process.Start(start)!;if(!process.WaitForExit(120000)){process.Kill(true);process.WaitForExit();throw new TimeoutException("Preparing the game renderer took too long. Reopen the game to retry.");}if(process.ExitCode!=0)throw new InvalidOperationException("Could not unpack the bundled game renderer. Check free disk space.");
                    // Microsoft requires these read/execute grants for Fixed Runtime 120+ on Windows 10.
                    string? runtimeFolder=Directory.EnumerateFiles(directory,"msedgewebview2.exe",SearchOption.AllDirectories).Select(Path.GetDirectoryName).FirstOrDefault();
                    if(runtimeFolder==null)throw new FileNotFoundException("The bundled renderer is incomplete.");
                    var permissions=new ProcessStartInfo(Path.Combine(Environment.SystemDirectory,"icacls.exe")){UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden};
                    permissions.ArgumentList.Add(runtimeFolder);permissions.ArgumentList.Add("/grant");permissions.ArgumentList.Add("*S-1-15-2-2:(OI)(CI)(RX)");permissions.ArgumentList.Add("/grant");permissions.ArgumentList.Add("*S-1-15-2-1:(OI)(CI)(RX)");
                    using var grant=Process.Start(permissions)!;if(!grant.WaitForExit(30000)){grant.Kill(true);grant.WaitForExit();throw new TimeoutException("Preparing renderer permissions took too long.");}if(grant.ExitCode!=0)throw new IOException("Could not prepare permissions for the bundled renderer.");
                    File.WriteAllText(ready,"156.0.4314.42");
                }
                string? executable=Directory.EnumerateFiles(directory,"msedgewebview2.exe",SearchOption.AllDirectories).FirstOrDefault();if(executable==null)throw new FileNotFoundException("The bundled renderer is incomplete.");return Path.GetDirectoryName(executable);
            }
            finally{gate.ReleaseMutex();}
        });
    }
}
