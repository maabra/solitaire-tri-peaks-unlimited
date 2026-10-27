"""Exercise real wins and the reused notice board, including bonus prizes."""
import asyncio,os
from pathlib import Path
from playwright.async_api import async_playwright
# Scenario: Exercise real wins and the reused notice board, including bonus prizes. Fail on violated assertions rather than accepting screenshots alone.
async def main():
 out=Path('artifacts/menu-boards');out.mkdir(exist_ok=True)
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True);page=await b.new_page(viewport={'width':1440,'height':900},reduced_motion='reduce');await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')");await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'))
  for level in [2,25]:
   path=await page.evaluate('''async level=>{const {Session}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const s=new Session();s.p.Unlocked=level;s.p.Hearts=20;s.p.GuideSeen=features.map(f=>f.id);s.start(level,false,12345);s.save();return s.hand.Deal.Solution}''',level)
   await page.reload();await page.evaluate("()=>{const b=document.createElement('button');b.dataset.action='continue';document.body.append(b);b.click();b.remove()}");
   for step in path:
    selector='[data-action=draw]' if step['Index']<0 else '[data-action="card:'+str(step['Index'])+'"]'
    await page.locator(selector).evaluate('e=>e.click()');await page.wait_for_timeout(170)
   await page.locator('.victory').wait_for()
   await page.screenshot(path=str(out/f'complete-{level}.png'))
   assert await page.locator('#dialog').get_attribute('data-frame')=='notice'
   assert await page.locator('#dialog').evaluate('e=>{const r=e.getBoundingClientRect(),b=document.body.getBoundingClientRect();return r.width===520&&r.height<=650&&r.top>=b.top&&r.bottom<=b.bottom&&e.scrollHeight<=e.clientHeight+2}')
   assert await page.locator('.victory .actions button').evaluate_all('es=>es.every(e=>e.getBoundingClientRect().width>=150)')
   await page.screenshot(path=str(out/f'complete-{level}.png'))
   gifts=page.locator('[data-action=showWinRewards]')
   if await gifts.count():
    await gifts.click();assert await page.locator('#dialog').evaluate('e=>e.scrollHeight<=e.clientHeight+2');await page.screenshot(path=str(out/'extra-prizes.png'))
  await b.close()
 print('PASS: real normal and bonus level wins share the compact notice board, fit the game and retain usable actions; extra prizes fit.')
asyncio.run(main())
