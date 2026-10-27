import asyncio,json
from pathlib import Path
from playwright.async_api import async_playwright
OUT=Path('artifacts/ui-audit');OUT.mkdir(exist_ok=True)
# Scenario: Exercise audit ui in an isolated browser session. Fail on violated assertions rather than accepting screenshots alone.
async def main():
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True)
  page=await b.new_page(viewport={'width':1280,'height':800},reduced_motion='reduce');errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto('http://127.0.0.1:8765/')
  await page.evaluate("""async()=>{const {Session}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const {helpers}=await import('./core.mjs');const s=new Session();s.p.Unlocked=10000;s.p.HighestPlayedLevel=10000;s.p.Coins=1840;s.p.Hearts=12;s.p.ChapterGifts100=[1];s.p.Customization.Unlocked=[0,1];s.p.GuideSeen=[...features.map(f=>f.id),'cards',...Array.from({length:17},(_,i)=>'mechanic'+i),...helpers.map(h=>'helper-'+h.Id)];s.events.a.Album.Cards=Object.fromEntries(Array.from({length:42},(_,i)=>[i,1]));s.events.a.Album.Packs=[2,3,1];s.save();}""")
  await page.reload();await page.evaluate('document.fonts.ready')
  report=[]
  for width,height in [(1280,800),(390,844),(320,640)]:
   await page.set_viewport_size({'width':width,'height':height})
   for action in ['map','home','gems','settings','lives','shop','journal','wheel','guide','events','ev:album','ev:missions','ev:travel','ev:regatta','ev:balloon','ev:hunt','ev:bingo','ev:trail','ev:memory','ev:customize','ev:chest','ev:postcard','ev:voyage','ev:tournament','ev:tickets','ev:milestones']:
    await page.evaluate("""action=>{document.querySelector('#dialog')?.close();let b=document.createElement('button');b.dataset.action=action;document.body.append(b);b.click();b.remove();}""",action)
    await page.wait_for_timeout(90)
    await page.evaluate('document.fonts.ready')
    data=await page.evaluate("""()=>{const root=document.querySelector('#dialog[open]')??document.querySelector('#app');return {heading:root.querySelector('h1,h2')?.textContent,overflow:root.scrollWidth>root.clientWidth+2,clipped:[...root.querySelectorAll('button,h1,h2,h3,small,strong')].filter(e=>e.getClientRects().length&&!e.closest('.card,.gem,.map-levels,.hunt-board,.bingo-board')&&(e.scrollWidth>e.clientWidth+3||e.scrollHeight>e.clientHeight+4)).slice(0,15).map(e=>({tag:e.tagName,cls:e.className,text:e.textContent.slice(0,80),w:[e.clientWidth,e.scrollWidth],h:[e.clientHeight,e.scrollHeight]})),font:getComputedStyle(root).fontFamily};}""")
    report.append({'width':width,'action':action,**data})
    if width!=320:await page.screenshot(path=str(OUT/f'{width}-{action.replace(":","-")}.png'))
  (OUT/'report.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
  issues=[r for r in report if r['overflow'] or r['clipped']]
  assert not issues,json.dumps(issues,indent=2)
  assert not errors,errors
  assert await page.evaluate("document.fonts.check('700 16px Nunito')&&document.fonts.check('800 24px \"Baloo 2\"')")
  print('PASS: 26 screens at three sizes, no clipped labels or horizontal overflow, bundled fonts loaded, no script errors.')
  await b.close()
asyncio.run(main())
