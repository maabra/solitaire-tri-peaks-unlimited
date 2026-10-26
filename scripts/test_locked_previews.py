"""Locked previews remain browsable without starting levels or earning rewards."""
import asyncio
from playwright.async_api import async_playwright
async def main():
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True)
  page=await b.new_page(viewport={'width':1280,'height':800},reduced_motion='reduce');errors=[]
  page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto('http://127.0.0.1:8765/')
  await page.locator('.story-node.current').wait_for()
  await page.evaluate("document.querySelector('[data-action=\"preview:2\"]').click()")
  assert await page.locator('#modal .bird-guide').count()==0,'Pip repeats onboarding inside a level preview'
  assert await page.locator('[data-action="level:2"]').is_disabled()
  assert await page.locator('[data-action="level:2"] .game-icon').count()==1
  await page.locator('[data-action=backMap]').click()
  assert not await page.locator('#dialog').evaluate('e=>e.open')
  await page.locator('[data-action="lockedFeature:regatta"]').click()
  assert '60' in await page.locator('.unlock-requirement').inner_text()
  assert await page.locator('.unlock-play').is_enabled()
  assert '1 / 60' in await page.locator('.goal-progress').inner_text()
  for width,height in [(1280,800),(390,844),(320,640)]:
   await page.set_viewport_size({'width':width,'height':height})
   assert await page.locator('#dialog').evaluate('e=>e.scrollWidth<=e.clientWidth+2')
  await page.locator('.unlock-play').click()
  assert await page.locator('.level-preview [data-action="level:1"]').is_visible()
  assert await page.locator('.board').count()==0
  await page.locator('[data-action=backMap]').click()
  await page.locator('[data-action=events]').click()
  await page.locator('[data-action="ev:bingo"]').click()
  assert '45' in await page.locator('.unlock-requirement').inner_text()
  await page.locator('[data-action=backMap]').click()
  await page.locator('[data-action=previewCurrent]').click()
  await page.locator('[data-action="level:1"]').click()
  await page.locator('.board .card').first.wait_for()
  assert await page.locator('[data-action=remix]').count()==0
  await page.wait_for_timeout(1200)
  assert not errors,errors
  await page.evaluate("async()=>{const {roadFor}=await import('./adventure-map.mjs');for(const c of [1,2,100,4800]){const r=roadFor(c);if(r.stops.some((p,i)=>Math.abs(p[0]-r.centers[i][0])>23))throw Error('Stop off road')}}")
  await b.close()
 print('PASS: locked level and feature previews, disabled Play, Back to map, road alignment, no Fresh deal or timer errors.')
asyncio.run(main())
