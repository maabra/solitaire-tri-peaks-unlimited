"""Long book menus scroll inside the painted pages, leaving the cover fixed."""
import asyncio,os
from pathlib import Path
from playwright.async_api import async_playwright
async def action(page,name):
 await page.evaluate("a=>{const e=document.createElement('button');e.dataset.action=a;(document.querySelector('#dialog').open?document.querySelector('#modal'):document.body).append(e);e.click();e.remove()}",name)
async def main():
 out=Path('artifacts/book-pages');out.mkdir(exist_ok=True)
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True)
  for width,height in [(1440,900),(868,748)]:
   page=await b.new_page(viewport={'width':width,'height':height},reduced_motion='reduce');await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')");await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'))
   for route in ['guide','settings','journal']:
    await action(page,route);assert await page.locator('#dialog').get_attribute('data-place')=='study'
    await page.evaluate("async()=>{const image=new Image();image.src='assets/menu-place-study.png';await image.decode()}")
    before=await page.locator('.menu-navigation').bounding_box()
    assert await page.locator('#dialog').evaluate("e=>{const r=e.getBoundingClientRect(),c=e.querySelector('.dialog-content').getBoundingClientRect();return getComputedStyle(e).overflowY==='hidden'&&e.scrollHeight<=e.clientHeight+2&&c.left>=r.left+40&&c.right<=r.right-40&&c.top>=r.top+140&&c.bottom<=r.bottom-110}")
    if route=='guide':
     await page.screenshot(path=str(out/f'guide-top-{width}.png'))
     content=page.locator('.dialog-content');assert await content.evaluate('e=>e.scrollHeight>e.clientHeight')
     box=await content.bounding_box();await page.mouse.move(box['x']+box['width']/2,box['y']+box['height']/2);await page.mouse.wheel(0,1200)
     await page.wait_for_timeout(100);assert await content.evaluate('e=>e.scrollTop>0')
     await content.evaluate('e=>e.scrollTop=e.scrollHeight');assert before==await page.locator('.menu-navigation').bounding_box()
     assert await page.locator('#dialog').evaluate('e=>e.scrollTop===0')
     await page.screenshot(path=str(out/f'guide-bottom-{width}.png'))
     done=page.locator('.dialog-content [data-action=close]');assert await done.is_visible();assert await done.evaluate("e=>{const r=e.getBoundingClientRect(),hit=document.elementFromPoint(r.x+r.width/2,r.y+r.height/2);return hit===e||e.contains(hit)}")
   await page.close()
  await b.close()
 print('PASS: guide, settings and journal remain on the book pages; guide wheel scrolling keeps the cover/navigation fixed and the final action reachable.')
asyncio.run(main())
