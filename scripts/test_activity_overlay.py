import asyncio
from playwright.async_api import async_playwright
# Scenario: Exercise activity overlay in an isolated browser session. Fail on violated assertions rather than accepting screenshots alone.
async def main():
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True)
  page=await b.new_page(reduced_motion='reduce')
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto('http://127.0.0.1:8765/')
  await page.evaluate("async()=>{const {defaults}=await import('./session.mjs');const p=defaults();p.Unlocked=80;p.HighestPlayedLevel=80;localStorage.setItem('meadow-peaks-profile',JSON.stringify(p))}")
  await page.reload()
  for w,h in [(1274,1267),(1280,800),(390,844),(320,640)]:
   await page.set_viewport_size({'width':w,'height':h})
   await page.wait_for_timeout(150)
   assert await page.locator('.adventure-map .road-destination').count()==0
   dock=page.locator('.map-viewport>.map-dock')
   assert await dock.evaluate("e=>getComputedStyle(e).position==='fixed'")
   before=await dock.bounding_box()
   await page.locator('.map-viewport').evaluate('e=>e.scrollTop+=300')
   after=await dock.bounding_box()
   assert before==after
   assert await dock.evaluate('e=>e.scrollHeight>e.clientHeight')
   assert await dock.locator('[data-action="ev:regatta"]').count()==1
   await page.screenshot(path=f'artifacts/activity-overlay-{w}.png')
  if await page.locator('[data-action=birdDismiss]').count():await page.locator('[data-action=birdDismiss]').click()
  await dock.locator('[data-action="ev:regatta"]').click()
  await page.locator('.regatta-sea').wait_for(state='visible')
  await b.close()
  print('PASS: activities stay fixed over scrolling map at four sizes; unlocked Regatta opens directly.')
asyncio.run(main())
