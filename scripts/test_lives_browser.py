import asyncio
from playwright.async_api import async_playwright
async def main():
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True)
  page=await browser.new_page(reduced_motion='reduce');errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto('http://127.0.0.1:8765/')
  await page.locator('.heart-counter').wait_for()
  for width,height in [(1280,800),(390,844),(320,640)]:
   await page.set_viewport_size({'width':width,'height':height})
   assert await page.locator('header nav').evaluate('e=>{const r=e.getBoundingClientRect();const b=document.body.getBoundingClientRect();return r.left>=b.left&&r.right<=b.right+1}')
  await page.set_viewport_size({'width':1280,'height':800})
  if await page.locator('[data-action=birdDismiss]').count():await page.locator('[data-action=birdDismiss]').click()
  await page.locator('[data-action=home]').click();await page.locator('#app [data-action=continue]').click()
  assert await page.locator('.level-preview').is_visible()
  assert await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')||'{}').Active??null")==None
  await page.locator('[data-action="level:1"]').click()
  await page.locator('.play-actions [data-action=replay]').click()
  assert await page.locator('.level-preview').is_visible()
  assert await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Hearts")==3
  await page.locator('[data-action=beginPreview]').click()
  assert await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Hearts")==2
  await page.locator('[data-action=leaveLevel]').click();await page.locator('[data-action=confirmLeave]').click()
  assert await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Hearts")==1
  await page.reload();assert await page.locator('.heart-counter b').inner_text()=='1'
  await page.evaluate("const p=JSON.parse(localStorage.getItem('meadow-peaks-profile'));p.Hearts=0;p.GuideSeen=['welcome','cards'];localStorage.setItem('meadow-peaks-profile',JSON.stringify(p))")
  await page.reload();await page.locator('[data-action=previewCurrent]').click()
  assert await page.locator('[data-action="level:1"]').is_disabled()
  await page.locator('#modal [data-action=lives]').click();await page.locator('#modal [data-action=gems]').click()
  assert await page.locator('.gems').is_visible()
  assert not errors,errors
  await browser.close()
 print('PASS: responsive heart counter, Play opens preview, cancel is free, restart/leave cost one, persistence, zero-heart gate and free Gem Garden access.')
asyncio.run(main())
