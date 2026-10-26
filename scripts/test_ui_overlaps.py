import asyncio,json,os
from pathlib import Path
from playwright.async_api import async_playwright
AUDIT=r'''()=>{
 const root=document.querySelector('#dialog[open]')??document.querySelector('#app');
 const rect=e=>{let r=e.getBoundingClientRect(),a={left:r.left,top:r.top,right:r.right,bottom:r.bottom};for(let p=e.parentElement;p;p=p.parentElement){const c=getComputedStyle(p),b=p.getBoundingClientRect();if(/auto|scroll|hidden|clip/.test(c.overflowX)){a.left=Math.max(a.left,b.left);a.right=Math.min(a.right,b.right)}if(/auto|scroll|hidden|clip/.test(c.overflowY)){a.top=Math.max(a.top,b.top);a.bottom=Math.min(a.bottom,b.bottom)}}return a};
 const nodes=[...root.querySelectorAll('button,input,select,summary,a[href]')].filter(e=>!e.matches('.card,.gem,.story-node,.helper-info')&&!e.closest('[hidden]')&&e.getClientRects().length&&getComputedStyle(e).visibility!=='hidden').map(e=>({e,r:rect(e),label:e.getAttribute('aria-label')||e.textContent.trim().slice(0,50)})).filter(({e,r})=>r.right-r.left>8&&r.bottom-r.top>8);
 const overlaps=[];for(let i=0;i<nodes.length;i++)for(let j=i+1;j<nodes.length;j++){const a=nodes[i],b=nodes[j];if(a.e.contains(b.e)||b.e.contains(a.e))continue;const w=Math.min(a.r.right,b.r.right)-Math.max(a.r.left,b.r.left),h=Math.min(a.r.bottom,b.r.bottom)-Math.max(a.r.top,b.r.top);if(w>3&&h>3)overlaps.push([a.label,b.label,Math.round(w),Math.round(h)])}
 const obscured=[];for(const {e,r,label}of nodes){const x=(r.left+r.right)/2,y=(r.top+r.bottom)/2;if(x<0||x>innerWidth||y<0||y>innerHeight)continue;const hit=document.elementFromPoint(x,y);if(hit&&hit!==e&&!e.contains(hit)&&!hit.closest('.helper-info'))obscured.push([label,hit.tagName,hit.className?.baseVal??hit.className])}
 const clip=[...root.querySelectorAll('h1,h2,h3,.tool-name,.menu-location')].filter(e=>e.getClientRects().length&&e.scrollWidth>e.clientWidth+2&&!e.closest('.activity-tabs')).map(e=>[e.className,e.textContent.trim().slice(0,60),e.scrollWidth,e.clientWidth]);
 return {overlaps,obscured,clip};
}'''
async def audit(unseen):
 out=Path('artifacts/overlap-regression');out.mkdir(parents=True,exist_ok=True)
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True);page=await b.new_page(viewport={'width':1440,'height':900},reduced_motion='reduce');await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'));await page.evaluate("async()=>{const {Session}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const {helpers}=await import('./core.mjs');const s=new Session();s.p.Unlocked=10000;s.p.HighestPlayedLevel=10000;s.p.ChapterGifts100=[1];s.p.GuideSeen=[];s.start(351,false,106723,'MP21');s.save()}");
  if not unseen: await page.evaluate("async()=>{const {Session}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const {helpers}=await import('./core.mjs');const s=new Session();s.p.GuideSeen=[...features.map(f=>f.id),'cards',...helpers.map(h=>'helper-'+h.Id),...Array.from({length:17},(_,i)=>'mechanic'+i)];s.save()}")
  await page.reload();results={}
  for action in ['map','home','gems','continue','lives','shop','settings','guide','journal','wheel','events','ev:trail','ev:memory','ev:album','ev:customize','ev:missions','ev:voyage','ev:tournament','ev:hunt','ev:bingo','ev:regatta','ev:balloon','ev:travel','ev:milestones','ev:chest','ev:postcard','ev:tickets']:
   await page.evaluate("a=>{const e=document.createElement('button');e.dataset.action=a;(document.querySelector('#dialog').open?document.querySelector('#modal'):document.body).append(e);e.click();e.remove()}",action);await page.wait_for_timeout(100)
   report=await page.evaluate(AUDIT);results[action]=report
   if any(report.values()):print(action,json.dumps(report),flush=True);await page.screenshot(path=str(out/(action.replace(':','-')+'.png')))
  (out/'report.json').write_text(json.dumps(results,indent=2),encoding='utf-8');await b.close()
  assert not any(any(r.values()) for r in results.values()), json.dumps(results,indent=2)
  print('No overlaps or covered controls:', 'first-time tutorials' if unseen else 'returning player')
async def main():
 await audit(True)
 await audit(False)
asyncio.run(main())
