import asyncio
from playwright.async_api import async_playwright
async def main():
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True)
  page=await b.new_page(reduced_motion='reduce');errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto('http://127.0.0.1:8765/')
  await page.locator('[data-action=previewCurrent]').click()
  assert await page.locator('#modal .panel-heading [data-ui-info]').count()==0
  await page.locator('[data-action=backMap]').click();await page.locator('[data-action=settings]').click()
  assert await page.locator('#modal .panel-heading [data-ui-info]').count()==0
  await page.locator('.shell-close').click();await page.locator('.heart-counter').click();await page.locator('#modal [data-action=gems]').click()
  opener=page.locator('[data-ui-info=gems]')
  for w,h in [(1280,800),(800,600),(390,844),(320,640)]:
   await page.set_viewport_size({'width':w,'height':h});await opener.click()
   help=page.locator('#context-help')
   assert await help.locator('.help-step').count()==3
   assert await help.evaluate('e=>{const r=e.getBoundingClientRect();return r.left>=0&&r.right<=innerWidth+1&&r.top>=0&&r.bottom<=innerHeight+1&&e.scrollWidth<=e.clientWidth+1}')
   assert await help.locator('.help-done').evaluate('e=>document.activeElement===e')
   await page.screenshot(path=f'artifacts/help-makeover-{w}.png')
   await page.keyboard.press('Escape');assert not await help.evaluate('e=>e.open')
  assert not errors,errors
  await b.close()
 print('PASS: no redundant preview/settings info; illustrated rule help fits four sizes, focuses Got it and closes with Escape.')
asyncio.run(main())
