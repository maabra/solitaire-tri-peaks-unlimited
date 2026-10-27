"""Exercise Pip's framed picker, long destination list and in-menu lesson layout."""
import asyncio,os
from pathlib import Path
from playwright.async_api import async_playwright

# Use normal action dispatch so the test observes the same menu lifecycle as a player.
async def action(page,name):
 await page.evaluate("a=>{const b=document.createElement('button');b.dataset.action=a;document.body.append(b);b.click();b.remove()}",name)

# Check the panel and its fixed footer, then prove the destination list owns its scrolling.
async def check_picker(page):
 panel=page.locator('.bird-guide[data-guide-panel=places]');await panel.wait_for()
 assert await panel.evaluate("e=>getComputedStyle(e).borderImageSource.includes('pip-help-frame.png')")
 assert await panel.evaluate("e=>{const r=e.getBoundingClientRect(),g=document.body.getBoundingClientRect();return r.left>=g.left+12&&r.right<=g.right-12&&r.top>=g.top+80&&r.bottom<=g.bottom-12&&e.scrollWidth<=e.clientWidth+1}"),'Picker clips the game or its own content'
 assert await panel.locator('.guide-actions').evaluate("e=>{const a=e.getBoundingClientRect(),p=e.closest('.bird-guide').getBoundingClientRect();return a.bottom<=p.bottom-20&&a.left>=p.left+20&&a.right<=p.right-20}"),'Footer clips the frame'
 assert await panel.locator('.guide-places button').evaluate_all("es=>es.every(e=>e.clientHeight>=70&&e.scrollWidth<=e.clientWidth+1)")
 if await panel.locator('.guide-places').evaluate('e=>e.scrollHeight>e.clientHeight+1'):
  await panel.locator('.guide-places').hover();before=await panel.bounding_box();await page.mouse.wheel(0,1200);await page.wait_for_timeout(120)
  assert await panel.locator('.guide-places').evaluate('e=>e.scrollTop>0'),'Destination list ignores wheel scrolling'
  assert await panel.bounding_box()==before,'Scrolling moved the whole frame'

# Exercise both the small early-game menu and a fully unlocked destination list at fixed game size.
async def main():
 out=Path('artifacts/guide-frames');out.mkdir(exist_ok=True)
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True)
  for width,height,level in [(1440,900,12),(1440,900,10000),(700,432,10000)]:
   page=await browser.new_page(viewport={'width':width,'height':height},reduced_motion='reduce');errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
   await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
   await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'))
   await page.evaluate("async l=>{const {Session}=await import('./session.mjs');const s=new Session();s.p.Unlocked=l;s.p.HighestPlayedLevel=l;s.save()}",level);await page.reload()
   await action(page,'map');await action(page,'birdHelp');await check_picker(page)
   await page.evaluate("async()=>{const i=new Image();i.src='assets/pip-help-frame.png';await i.decode()}")
   await page.screenshot(path=str(out/f'picker-{width}-{level}.png'))
   await action(page,'birdDismiss');await action(page,'settings');await action(page,'birdHelp')
   assert await page.locator('.bird-guide.in-panel').count()==1
   panel=page.locator('.bird-guide.in-panel');assert await panel.evaluate("e=>{const a=e.getBoundingClientRect(),b=e.closest('dialog').getBoundingClientRect();return a.left>=b.left&&a.right<=b.right&&e.scrollWidth<=e.clientWidth+1}")
   await page.screenshot(path=str(out/f'in-menu-{width}-{level}.png'))
   # Closing a parent menu must move the picker back to the game, not leave it hidden in the closed dialog.
   await action(page,'close');await page.locator('body>.bird-guide:not(.in-panel)').wait_for()
   await action(page,'birdDismiss');await action(page,'level:3');await action(page,'birdDismiss')
   board=await page.locator('.board').bounding_box();tools=await page.locator('.tools').bounding_box()
   await action(page,'birdHelp');await check_picker(page)
   assert await page.locator('.board').bounding_box()==board,'Picker resized the tableau'
   assert await page.locator('.tools').bounding_box()==tools,'Picker moved the helper column'
   assert not errors,errors;await page.close()
  await browser.close()
 print('PASS: generated frames load; early/full destination menus fit; only choices scroll; footer and in-menu frames stay inside their surfaces.')
asyncio.run(main())
