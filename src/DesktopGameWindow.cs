using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
namespace MeadowPeaks;
// Windows hosts the same packaged HTML, CSS and modules as GitHub Pages.
internal sealed partial class DesktopGameWindow:Form
{
    const string Origin="https://meadow-peaks.local/";
    readonly WebView2 browser=new(){Dock=DockStyle.Fill,DefaultBackgroundColor=Color.FromArgb(49,82,59)};
    readonly Label loading=new(){Dock=DockStyle.Fill,Text="Meadow Peaks\nPreparing your meadow...",TextAlign=ContentAlignment.MiddleCenter,BackColor=Color.FromArgb(49,82,59),ForeColor=Color.FromArgb(255,244,211),Font=new Font("Segoe UI",22,FontStyle.Bold)};
    readonly IProfileStore store;readonly bool smoke,checking;Progress profile;string initialProfile,loadWarning="";bool loaded,closing,initializingNavigation;
    readonly System.Windows.Forms.Timer timeout=new(){Interval=170000};
    public DesktopGameWindow(IProfileStore store,bool smoke,bool checking)
    {
        this.store=store;this.smoke=smoke;this.checking=checking;
        string? shared=smoke?null:DesktopProfileBridge.Load(out loadWarning);
        if(shared!=null){profile=JsonSerializer.Deserialize<Progress>(shared)!;initialProfile=shared;}
        else {var restored=Composition.CreateSession(store);profile=restored.Profile;loadWarning=restored.LoadWarning;
        if(profile.Active!=null)profile.Active=restored.Hand?.Capture(restored.Seed,restored.Level,restored.Endless);
        if(profile.ActiveGarden!=null)profile.ActiveGarden=(restored.CoinGame as IPersistentCoinGame)?.Capture();
        if(smoke){profile.ReducedMotion=true;profile.HighestPlayedLevel=2500;}
        initialProfile=JsonSerializer.Serialize(profile);}
        if(smoke){var data=System.Text.Json.Nodes.JsonNode.Parse(initialProfile)!;data["GuideSeen"]=JsonSerializer.SerializeToNode(new[]{"trail","memory","welcome","cards","garden","missions","wheel","album","tournament","balloon","hunt","bingo","regatta","voyage","travel","chest","postcard","tickets","milestones","customize"});initialProfile=data.ToJsonString();}
        Text="Meadow Peaks";ClientSize=new(1280,800);MinimumSize=new(800,600);StartPosition=FormStartPosition.CenterScreen;Icon=Icon.ExtractAssociatedIcon(Environment.ProcessPath!);
        Controls.Add(browser);Controls.Add(loading);Shown+=async(_,_)=>await Initialize();
        FormClosing+=async(_,e)=>{if(smoke||checking||closing||!loaded)return;e.Cancel=true;closing=true;try{await browser.CoreWebView2.ExecuteScriptAsync("window.dispatchEvent(new Event('pagehide'))");await Task.Delay(80);}catch{}Close();};
        timeout.Tick+=(_,_)=>Fail(new TimeoutException("The game interface did not finish loading."));
        if(smoke||checking)timeout.Start();
    }
    async Task Initialize()
    {
        try
        {
            string? runtime=await DesktopAssets.Runtime();
            var options=new CoreWebView2EnvironmentOptions();
            var environment=await CoreWebView2Environment.CreateAsync(runtime,Path.Combine(DesktopAssets.Root,smoke||checking?"browser-check":"browser"),options);
            await browser.EnsureCoreWebView2Async(environment);var core=browser.CoreWebView2;
            core.Settings.AreDevToolsEnabled=false;core.Settings.AreDefaultContextMenusEnabled=false;core.Settings.IsStatusBarEnabled=false;

            await core.AddScriptToExecuteOnDocumentCreatedAsync("globalThis.meadowDesktop={profile:"+initialProfile+",warning:"+JsonSerializer.Serialize(loadWarning)+",readOnly:"+((smoke||checking)?"true":"false")+"};");
            core.WebMessageReceived+=MessageReceived;
            core.NavigationStarting+=(_,e)=>{if(!e.Uri.StartsWith(Origin,StringComparison.Ordinal))e.Cancel=true;};
            FormWindowState previousState=WindowState;Rectangle previousBounds=Bounds;FormBorderStyle previousBorder=FormBorderStyle;
            core.ContainsFullScreenElementChanged+=(_,_)=>{if(core.ContainsFullScreenElement){previousState=WindowState;previousBounds=Bounds;previousBorder=FormBorderStyle;WindowState=FormWindowState.Normal;FormBorderStyle=FormBorderStyle.None;Bounds=Screen.FromControl(this).Bounds;}else{FormBorderStyle=previousBorder;Bounds=previousBounds;WindowState=previousState;}};
            core.NewWindowRequested+=(_,e)=>e.Handled=true;
            core.AddWebResourceRequestedFilter("*",CoreWebView2WebResourceContext.All);
            core.WebResourceRequested+=(_,e)=>
            {
                if(e.Request.Uri.StartsWith("data:",StringComparison.Ordinal)||e.Request.Uri.StartsWith("blob:",StringComparison.Ordinal))return;
                if(!e.Request.Uri.StartsWith(Origin,StringComparison.Ordinal)){e.Response=environment.CreateWebResourceResponse(null,403,"Offline game","");return;}
                string path=new Uri(e.Request.Uri).AbsolutePath.TrimStart('/');if(path.Length==0)path="index.html";
                var stream=typeof(DesktopGameWindow).Assembly.GetManifestResourceStream("MeadowPeaks.Web/"+path);
                string mime=Path.GetExtension(path) switch {".html"=>"text/html; charset=utf-8",".mjs"=>"text/javascript; charset=utf-8",".css"=>"text/css; charset=utf-8",".svg"=>"image/svg+xml",".png"=>"image/png",".ttf"=>"font/ttf",".woff2"=>"font/woff2",_=>"application/octet-stream"};
                e.Response=environment.CreateWebResourceResponse(stream,stream==null?404:200,stream==null?"Not found":"OK","Content-Type: "+mime+"\r\nCache-Control: no-store");
            };
            core.DownloadStarting+=(_,e)=>{using var dialog=new SaveFileDialog{FileName="MeadowPeaks-save.json",Filter="Meadow Peaks save (*.json)|*.json"};if(dialog.ShowDialog(this)!=DialogResult.OK){e.Cancel=true;return;}e.ResultFilePath=dialog.FileName;e.Handled=true;};
            core.NavigationCompleted+=async(_,e)=>
            {
                if(loaded||initializingNavigation||!core.Source.StartsWith(Origin,StringComparison.Ordinal))return;initializingNavigation=true;if(!e.IsSuccess){Fail(new InvalidOperationException("Could not load the packaged game interface: "+e.WebErrorStatus));return;}
                for(int i=0;i<120;i++){if(await core.ExecuteScriptAsync("!!document.querySelector('.story-node')")=="true")break;await Task.Delay(50);}
                if(await core.ExecuteScriptAsync("!!document.querySelector('.story-node')")!="true"){Fail(new InvalidOperationException("The packaged game interface did not initialize. Your save is preserved."));return;}
                loaded=true;loading.Hide();browser.Focus();StartupDiagnostics.Write("MeadowPeaks-startup.txt","PASS: shared Windows/web interface loaded from packaged local assets. Profile loaded; active level "+profile.Active?.Level);
                if(checking){timeout.Stop();Close();}else if(smoke)await SmokeSharedUi();
            };
            core.Navigate(Origin+"index.html");
        }
        catch(Exception error){Fail(error);}
    }
    void MessageReceived(object? sender,CoreWebView2WebMessageReceivedEventArgs e)
    {
        if(!e.Source.StartsWith(Origin,StringComparison.Ordinal))return;
        try
        {
            using var data=JsonDocument.Parse(e.WebMessageAsJson);if(data.RootElement.GetProperty("type").GetString()!="save"||smoke||checking)return;
            string json=data.RootElement.GetProperty("profile").GetRawText();DesktopProfileBridge.Save(json);
        }
        catch(Exception error){StartupDiagnostics.Write("MeadowPeaks-save-error.txt",error.ToString());browser.CoreWebView2.ExecuteScriptAsync("document.querySelector('#toast').textContent='Could not save progress. Export your adventure from Settings.';document.querySelector('#toast').classList.add('show');");}
    }
    void Fail(Exception error){closing=true;timeout.Stop();if(smoke)StartupDiagnostics.Write("smoke-result.txt",error.ToString());StartupDiagnostics.Error(error,smoke||checking);Close();}
    protected override void Dispose(bool disposing){if(disposing){timeout.Dispose();browser.Dispose();loading.Dispose();}base.Dispose(disposing);}
}
