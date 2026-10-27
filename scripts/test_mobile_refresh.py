import asyncio
from pathlib import Path
from playwright.async_api import async_playwright
# Scenario: Exercise mobile refresh in an isolated browser session. Fail on violated assertions rather than accepting screenshots alone.
async def main():
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True);page=await b.new_page(viewport={'width':1280,'height':800},reduced_motion='reduce');errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto('http://127.0.0.1:8765/')
  await page.evaluate("""async()=>{const {Session}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const s=new Session();s.p.Unlocked=401;s.p.Hearts=20;s.p.GuideSeen=[...features.map(f=>f.id),'cards'];s.p.Stars=Object.fromEntries(Array.from({length:100},(_,i)=>[301+i,3]));s.save()}""")
  await page.reload();await page.locator('.map-controls [data-action="ev:travel"]').click();await page.locator('[data-action="ev:travel-4"]').click();await page.locator('.map-dock [data-action=events]').click();await page.locator('#modal [data-action="ev:chest"]').first.click()
  assert await page.locator('.chest-tier').count()==6
  assert await page.locator('.chapter-chest details').count()==0
  assert await page.locator('.chest-tier.reached').count()==6
  assert await page.locator('.chest-rule').is_visible()
  for w,h in [(1280,800),(1366,768),(800,600),(390,844),(320,640)]:
   await page.set_viewport_size({'width':w,'height':h});await page.wait_for_timeout(80)
   assert await page.locator('#dialog').evaluate('e=>e.scrollWidth<=e.clientWidth+2'),w
   assert await page.locator('.chest-tier-track').evaluate('e=>e.scrollWidth<=e.clientWidth+2'),w
   if w>=900:
    assert await page.locator('[data-action="ev:chest-4"]').evaluate('e=>{const r=e.getBoundingClientRect();return r.top>0&&r.bottom<innerHeight}'),(w,'chest action below fold')
   for tab in await page.locator('.activity-tabs button').all():
    await tab.hover();assert await tab.evaluate('e=>{const a=e.getBoundingClientRect(),p=e.parentElement.getBoundingClientRect();return a.top>=p.top&&a.bottom<=p.bottom}'),(w,'hover clipped')
  await page.set_viewport_size({'width':1280,'height':800})
  before=await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Coins")
  await page.locator('[data-action="ev:chest-4"]').click()
  assert await page.locator('.chest-hero [aria-label="Obsidian chest, opened"]').count()==1
  saved=await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile'))")
  assert saved['Coins']==before+350
  assert saved['Adventure']['Chest'].count(4)==1
  # A plain level win skips the extra-prize tray entirely.
  last=await page.evaluate("""async()=>{const {Session}=await import('./session.mjs');const s=new Session();s.start(1,false,12345);const path=s.hand.Deal.Solution;for(const step of path.slice(0,-1)){if(step.Index<0){s.draw();s.tick(2);}else s.play(step.Index);}s.save();return path.at(-1).Index;}""")
  await page.reload();await page.evaluate("const b=document.createElement('button');b.dataset.action='continue';document.body.append(b);b.click();b.remove()")
  await page.locator(f'[data-action="card:{last}"]').click();await page.locator('.victory').wait_for()
  assert await page.locator('[data-action=showWinRewards]').count()==0
  coins=await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Coins")
  await page.locator('.victory [data-action=next]').click();assert await page.locator('.level-preview').is_visible()
  assert await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Coins")==coins
  assert not errors,errors;await b.close()
 print('PASS: six visible tiers, laptop action placement, five responsive sizes, unclipped hovered tabs, one-time Obsidian reward and automatic base-win flow.')
asyncio.run(main())
