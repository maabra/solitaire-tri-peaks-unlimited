import asyncio
from playwright.async_api import async_playwright
# Scenario: Exercise progress clarity in an isolated browser session. Fail on violated assertions rather than accepting screenshots alone.
async def main():
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True);page=await b.new_page(reduced_motion='reduce');errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto('http://127.0.0.1:8765/')
  await page.evaluate("""async()=>{const {Session}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const s=new Session();s.p.Unlocked=10000;s.p.Hearts=999;s.p.GuideSeen=[...features.map(f=>f.id),'cards'];s.p.Stars=Object.fromEntries(Array.from({length:35},(_,i)=>[i+1,1]));s.p.Customization.Unlocked.push(2);s.events.a.Bingo.Drawn=Array.from({length:13},(_,i)=>i);s.events.a.Bingo.Lines=[0,1];s.save();}""")
  await page.reload()
  await page.locator('.map-dock [data-action="ev:bingo"]').click()
  assert '13 / 25' in await page.locator('[data-progress="Numbers marked"]').inner_text()
  assert '2 / 12' in await page.locator('[data-progress="Lines completed"]').inner_text()
  for w,h in [(1280,800),(390,844),(320,640)]:
   await page.set_viewport_size({'width':w,'height':h})
   assert await page.locator('#dialog').evaluate('e=>e.scrollWidth<=e.clientWidth+2')
   for el in await page.locator('.goal-progress').all():assert await el.evaluate('e=>e.scrollWidth<=e.clientWidth+1')
   await page.screenshot(path=f'artifacts/progress-bingo-{w}.png')
  await page.locator('.shell-close').click();await page.locator('.map-dock [data-action="ev:customize"]').click()
  assert '35 / 100' in await page.locator('.skin-card').first.inner_text()
  assert await page.locator('.map-dock [data-action="ev:customize"] [data-icon=studio]').count()==1
  assert await page.evaluate("async()=>{const {gameIcon}=await import('./game-art.mjs');return ['bookmark','dealCode','dice','seed','pairs','studio','mapRoute','cardProgress','ratingStar','streak','pipGuide','ruleBook'].every(n=>gameIcon(n).includes('data-icon=\"'+n+'\"'))}")
  assert not errors,errors;await b.close()
 print('PASS: accurate bingo and chapter-design counts, no horizontal clipping at three sizes, distinct purpose-specific icons.')
asyncio.run(main())
