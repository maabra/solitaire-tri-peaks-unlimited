using System.Text.Json;
using Microsoft.Web.WebView2.Core;
namespace MeadowPeaks;
internal sealed partial class DesktopGameWindow
{
    async Task<string> Eval(string script)=>await browser.CoreWebView2.ExecuteScriptAsync(script);
    async Task Check(string expression,string label){if(await Eval(expression)!="true")throw new InvalidOperationException("Shared UI check failed: "+label);}
    async Task ClickAction(string action)=>await Eval("document.querySelector('[data-action=\""+action+"\"]').click()");
    async Task Shot(string name)
    {
        await Task.Delay(120);
        string directory=Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!,"screenshots");Directory.CreateDirectory(directory);
        using var file=File.Create(Path.Combine(directory,name+".png"));await browser.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png,file);
    }
    async Task SmokeSharedUi()
    {
        try
        {
            // NavigationCompleted can precede the final module's first render.
            for(int i=0;i<100&&await Eval("!!document.querySelector('.story-node')")!="true";i++)await Task.Delay(50);
            await Check("document.querySelectorAll('.story-node').length===20","map loaded");await Shot("01-shared-map");
            await ClickAction("wheel");await Shot("02-shared-wheel");await ClickAction("spinWheel");await Task.Delay(150);await Check("meadowDesktop.profile.WheelSpinsUsed===1","wheel credited once");await ClickAction("close");
            await ClickAction("preview:1");await Shot("03-shared-preview");await ClickAction("level:1");await Check("document.querySelectorAll('.board .card').length===28","level loaded");await Shot("04-shared-cards");
            await Eval("globalThis.__smokeComplete=false;globalThis.__smokeError=null;(async()=>{try{let path=meadowDesktop.profile.Active.DealtBoard.Solution;let stock=document.querySelector('[data-action=draw]');let nodes=new Map([...document.querySelectorAll('.board .card')].map(e=>[e.dataset.action,e]));for(let s of path){let a=s.Index<0?'draw':'card:'+s.Index;while(document.querySelector('[data-action=\"'+a+'\"]').disabled)await new Promise(r=>setTimeout(r,50));document.querySelector('[data-action=\"'+a+'\"]').click();if(document.querySelector('[data-action=draw]')!==stock||![...document.querySelectorAll('.board .card')].every(e=>nodes.get(e.dataset.action)===e))throw Error('Tableau nodes replaced');}await new Promise(r=>setTimeout(r,200));__smokeComplete=true;}catch(e){__smokeError=String(e)}})()");
            for(int i=0;i<700&&await Eval("__smokeComplete")!="true";i++){if(await Eval("__smokeError!==null")=="true")throw new Exception(await Eval("__smokeError"));await Task.Delay(50);}
            await Check("meadowDesktop.profile.Unlocked===2&&!!document.querySelector('.victory')","win and rewards");await Shot("05-shared-win");
            await ClickAction("home");await ClickAction("shop");await Shot("06-shared-shop");await ClickAction("close");await ClickAction("settings");await Shot("07-shared-settings");await ClickAction("close");await ClickAction("gems");await Shot("08-shared-gems");
            StartupDiagnostics.Write("smoke-result.txt","PASS: shared web/Windows renderer, packaged offline assets, map, wheel reward, level preview, legal win, persistent card nodes, rewards, shop, settings and Gem Garden.");timeout.Stop();Close();
        }
        catch(Exception error){Fail(error);}
    }
}
