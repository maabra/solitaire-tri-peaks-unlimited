"""Check the authored 16:9 scene and full main activities inside the 720p frame."""
import asyncio
from playwright.async_api import async_playwright

async def main():
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True)
  page=await browser.new_page(viewport={'width':1440,'height':900},reduced_motion='reduce')
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto('http://127.0.0.1:8765/')
  await page.wait_for_function("document.querySelector('.adventure-map>.map-scenery')?.style.backgroundImage.includes('map-meadow-v2.png')")
  assert await page.evaluate("async()=>{const i=new Image();i.src='assets/map-meadow-720p.png';await i.decode();return Math.abs(i.naturalWidth/i.naturalHeight-16/9)<.01}")
  before=await page.locator('.map-scenery').bounding_box()
  await page.locator('.map-viewport').evaluate('e=>e.scrollTop-=400')
  assert (await page.locator('.map-scenery').bounding_box())['y']!=before['y'],'Painting did not scroll with the route'
  await page.locator('.map-locate').click()
  await page.locator('.story-node.current').click()
  assert await page.locator('#dialog').evaluate('e=>e.scrollHeight<=e.clientHeight+2')
  await page.locator('[data-action="level:1"]').click()
  assert await page.locator('#app').evaluate('e=>e.scrollHeight<=e.clientHeight+2'),'Card controls extend below frame'
  assert await page.locator('.tools button,.play-actions button').evaluate_all('es=>es.every(e=>{const a=e.getBoundingClientRect(),b=document.body.getBoundingClientRect();return a.top>=b.top&&a.bottom<=b.bottom})')
  await page.locator('header [data-action=home]').click()
  await page.locator('#app [data-action=gems]').click()
  assert await page.locator('#app').evaluate('e=>e.scrollHeight<=e.clientHeight+2'),'Garden extends below frame'
  assert await page.locator('.gem').count()==64
  await page.evaluate("async()=>{const {Session}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const s=new Session();s.p.Unlocked=10000;s.p.GuideSeen=features.map(f=>f.id);s.save()}")
  await page.reload()
  for view in ['trail','memory']:
   await page.locator(f'.map-dock [data-action="ev:{view}"]').click()
   assert await page.locator('#dialog').evaluate('e=>e.scrollHeight<=e.clientHeight+2'),view
   await page.locator('.shell-close').click()
  await browser.close()
 print('PASS: native 16:9 art, scrolling map scenery, card tools, garden, previews and side boards fit 720p.')

asyncio.run(main())
