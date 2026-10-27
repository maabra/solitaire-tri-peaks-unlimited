import asyncio
from playwright.async_api import async_playwright
# Scenario: Exercise travel navigation in an isolated browser session. Fail on violated assertions rather than accepting screenshots alone.
async def main():
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True)
  page=await b.new_page(reduced_motion='reduce')
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto('http://127.0.0.1:8765/')
  await page.evaluate("async()=>{const {defaults}=await import('./session.mjs');const p=defaults();p.Unlocked=201;localStorage.setItem('meadow-peaks-profile',JSON.stringify(p));}")
  await page.reload()
  if await page.locator('[data-action=birdDismiss]').count():await page.locator('[data-action=birdDismiss]').click()
  assert await page.locator('[data-action=prevChapter],[data-action=nextChapter],.map-controls [data-action=goto]').count()==0
  assert await page.locator('.map-viewport').evaluate("e=>getComputedStyle(e).scrollbarWidth")=='none'
  await page.locator('.map-controls [data-action="ev:travel"]').click()
  await page.locator('[data-action="ev:travel-2"]').click()
  assert await page.locator('.adventure-map').get_attribute('data-chapter')=='2'
  await page.locator('[data-action=locate]').click()
  assert await page.locator('.travel-list').is_visible()
  assert await page.locator('.adventure-map').get_attribute('data-chapter')=='2'
  await page.locator('[data-action="ev:travel-3"]').click()
  assert await page.locator('.adventure-map').get_attribute('data-chapter')=='3'
  await b.close()
 print('PASS: chapter selection only in Travel Hub, locate routes through hub, hidden map scrollbar.')
asyncio.run(main())
