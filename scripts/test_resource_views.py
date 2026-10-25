import asyncio
from pathlib import Path
from playwright.async_api import async_playwright
async def main():
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True)
  page=await b.new_page(reduced_motion='reduce');errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto('http://127.0.0.1:8765/')
  await page.evaluate("async()=>{const {defaults}=await import('./session.mjs');const p=defaults();p.Unlocked=201;p.GemHeartProgress=5;localStorage.setItem('meadow-peaks-profile',JSON.stringify(p));}")
  await page.reload()
  if await page.locator('[data-action=birdDismiss]').count():await page.locator('[data-action=birdDismiss]').click()
  await page.locator('[data-action=gems]').first.click()
  await page.locator('.garden-resources').wait_for()
  assert await page.locator('.garden-resources [data-resource=coins]').count()==1
  assert await page.locator('.resource-goal').get_attribute('aria-label')=='5 of 10 successful swaps toward 1 heart'
  for width,height in [(1280,800),(390,844),(320,640)]:
   await page.set_viewport_size({'width':width,'height':height})
   assert await page.locator('.garden-resources').evaluate('e=>e.scrollWidth<=e.clientWidth+1')
   for badge in await page.locator('.garden-resources .resource-badge').all():
    assert await badge.get_attribute('aria-label')
   Path('artifacts/resource-screenshots').mkdir(exist_ok=True)
   await page.screenshot(path=f'artifacts/resource-screenshots/garden-{width}.png')
  await page.set_viewport_size({'width':1280,'height':800})
  await page.locator('.heart-counter').click()
  assert await page.locator('.lives-panel .resource-goal [data-resource=heart]').count()==1
  await page.locator('#modal [data-action=events]').click()
  assert await page.locator('.event-hub .resource-badge').count()>0
  await page.locator('[data-action="ev:missions"]').first.click()
  assert await page.locator('.event-row [data-resource=coins]').count()>0
  # Live text updates get formatted once, without nested duplicates or lost values.
  await page.evaluate("document.querySelector('#toast').textContent='+90 coins · +1 heart'")
  await page.wait_for_timeout(100)
  assert await page.locator('#toast .resource-badge').count()==2
  await page.evaluate("document.querySelector('#toast').textContent='+25 coins'")
  await page.wait_for_timeout(100)
  assert await page.locator('#toast .resource-badge').count()==1
  assert await page.locator('#toast .resource-badge b').inner_text()=='+25'
  assert not errors,errors
  await b.close()
 print('PASS: icon resource strips at three sizes, swap-to-heart goal, activities, rewards, accessible labels and dynamic updates.')
asyncio.run(main())

