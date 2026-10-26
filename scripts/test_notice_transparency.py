"""Compact notices must not inherit large-menu overlays or impossible costs."""
import asyncio,os
from pathlib import Path
from playwright.async_api import async_playwright
async def action(page,name):
 await page.evaluate("a=>{const e=document.createElement('button');e.dataset.action=a;(document.querySelector('#dialog').open?document.querySelector('#modal'):document.body).append(e);e.click();e.remove()}",name)
async def main():
 out=Path('artifacts/notice-transparency');out.mkdir(exist_ok=True)
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True)
  for width,height in [(1440,900),(572,720)]:
   page=await b.new_page(viewport={'width':width,'height':height},reduced_motion='reduce');await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')");await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'))
   await page.evaluate("async()=>{const {Session}=await import('./session.mjs');const s=new Session();s.p.Unlocked=5;s.start(5,false,106723,'MP21');s.save()}");await page.reload()
   for parent in ['shop','settings','ev:album']:
    await action(page,parent);await action(page,'preview:100')
    assert await page.locator('#dialog').get_attribute('data-frame')=='notice'
    assert await page.locator('#dialog').get_attribute('data-place') is None
    assert await page.locator('.life-warning').count()==0
    assert 'Complete level 99' in await page.locator('.unlock-requirement').inner_text()
    assert await page.locator('#dialog').evaluate("e=>{const c=e.querySelector('.dialog-content'),r=e.getBoundingClientRect();return getComputedStyle(c).backgroundColor==='rgba(0, 0, 0, 0)'&&getComputedStyle(e).backgroundImage==='none'&&e.scrollHeight<=e.clientHeight+2&&r.top>=0&&r.bottom<=innerHeight}")
   await page.evaluate("async()=>{const image=new Image();image.src='assets/level-notice-board.png';await image.decode()}");await page.screenshot(path=str(out/f'locked-100-{width}.png'))
   await action(page,'preview:1');assert await page.locator('.life-warning').count()==1,'Playable replacement lost its cost warning'
   await action(page,'preview:5');assert await page.locator('.life-warning').count()==0,'Resuming incorrectly costs a heart'
   await page.close()
  await b.close()
 print('PASS: locked level 100 has transparent surroundings after shop/settings/album, fits without scrolling and shows only its unlock requirement; replacement and resume costs remain correct.')
asyncio.run(main())
