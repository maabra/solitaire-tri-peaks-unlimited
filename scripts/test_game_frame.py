"""Check centered game bounds, readable controls, dialogs, and actual play."""
import asyncio
from pathlib import Path
from playwright.async_api import async_playwright

async def main():
 out=Path('artifacts/game-frame');out.mkdir(exist_ok=True)
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True)
  for width,height in [(1920,1080),(1075,1757),(1280,720),(700,432),(390,844),(320,640)]:
   page=await browser.new_page(viewport={'width':width,'height':height},reduced_motion='reduce')
   errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
   await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
   await page.goto('http://127.0.0.1:8765/');await page.locator('.story-node.current').wait_for()
   assert await page.locator('body').evaluate('e=>{const r=e.getBoundingClientRect();return r.width===1280&&r.height===720&&r.x>=0&&r.y>=0}')
   assert await page.locator('body').evaluate('e=>getComputedStyle(e).transform===\'matrix(1, 0, 0, 1, 0, 0)\''),'Game is scaled'
   assert await page.locator('.map-dock .map-icon').first.evaluate('e=>e.getBoundingClientRect().width===88')
   assert await page.locator('.map-dock-scroll').evaluate('e=>e.scrollHeight>e.clientHeight')
   assert await page.locator('.map-dock').evaluate("e=>getComputedStyle(e).borderTopWidth==='0px'"),'Floating box returned around icons'
   assert await page.locator('.map-viewport').evaluate("e=>{const s=getComputedStyle(e,'::before');return s.display==='block'&&s.position==='fixed'&&s.top==='0px'&&s.bottom==='0px'&&s.backgroundImage.includes('map-scroll-edge.svg')}")
   if width<1280:assert await page.evaluate('document.scrollingElement.scrollWidth>document.documentElement.clientWidth')
   if height<720:assert await page.evaluate('document.scrollingElement.scrollHeight>document.documentElement.clientHeight')
   await page.screenshot(path=str(out/f'map-{width}.png'))
   await page.locator('.story-node.current').click()
   assert await page.locator('#modal .bird-guide').count()==0
   assert await page.locator('#dialog').evaluate('e=>{const a=e.getBoundingClientRect(),b=document.body.getBoundingClientRect();return a.left>=0&&a.right<=innerWidth&&a.top>=0&&a.bottom<=innerHeight}')
   await page.locator('[data-action="level:1"]').click();await page.locator('.board .card').first.wait_for()
   assert await page.locator('#app').evaluate('e=>!e.style.zoom||e.style.zoom===\'1\'')
   await page.locator('[data-action=draw]').click()
   await page.screenshot(path=str(out/f'play-{width}.png'))
   assert not errors,errors
   await page.close()
  await browser.close()
 print('PASS: fixed 1280x720 game, full-size icons, outer scrollbars, readable dialogs and playable cards in desktop and small windows.')

asyncio.run(main())
