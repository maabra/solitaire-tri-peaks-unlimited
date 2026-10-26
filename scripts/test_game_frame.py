"""Check centered game bounds, readable controls, dialogs, and actual play."""
import asyncio
from pathlib import Path
from playwright.async_api import async_playwright

async def main():
 out=Path('artifacts/game-frame');out.mkdir(exist_ok=True)
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True)
  for width,height in [(1920,1080),(1075,1757),(1280,720),(390,844),(320,640)]:
   page=await browser.new_page(viewport={'width':width,'height':height},reduced_motion='reduce')
   errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
   await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
   await page.goto('http://127.0.0.1:8765/');await page.locator('.story-node.current').wait_for()
   assert await page.locator('body').evaluate('e=>{const r=e.getBoundingClientRect();return Math.abs(r.x+r.width/2-innerWidth/2)<1&&Math.abs(r.y+r.height/2-innerHeight/2)<1&&r.width<=1280&&r.height<=720}')
   assert await page.locator('body').evaluate('e=>getComputedStyle(e).transform===\'matrix(1, 0, 0, 1, 0, 0)\''),'Game is scaled'
   assert await page.locator('.map-dock .map-icon').first.evaluate('e=>e.getBoundingClientRect().width>=66')
   assert await page.locator('.map-dock-scroll').evaluate('e=>e.scrollHeight>e.clientHeight')
   await page.screenshot(path=str(out/f'map-{width}.png'))
   await page.locator('.story-node.current').click()
   assert await page.locator('#modal .bird-guide').count()==0
   assert await page.locator('#dialog').evaluate('e=>{const a=e.getBoundingClientRect(),b=document.body.getBoundingClientRect();return a.left>=b.left&&a.right<=b.right&&a.top>=b.top&&a.bottom<=b.bottom}')
   await page.locator('[data-action="level:1"]').click();await page.locator('.board .card').first.wait_for()
   assert await page.locator('#app').evaluate('e=>!e.style.zoom||e.style.zoom===\'1\'')
   await page.locator('[data-action=draw]').click()
   await page.screenshot(path=str(out/f'play-{width}.png'))
   assert not errors,errors
   await page.close()
  await browser.close()
 print('PASS: centered desktop and phone frames, readable menu icons, contained dialogs and playable card tables without app scaling.')

asyncio.run(main())
