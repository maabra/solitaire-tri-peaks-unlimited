import asyncio
from playwright.async_api import async_playwright
async def main():
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True);page=await b.new_page(viewport={'width':1280,'height':800});errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto('http://127.0.0.1:8765/')
  await page.evaluate("async()=>{const {Session}=await import('./session.mjs');const s=new Session();s.p.Unlocked=3;s.p.GuideSeen=['welcome','cards','postcard'];s.save()}")
  await page.reload()
  assert await page.locator('[data-feature=garden] .new-feature-badge').count()==1
  assert await page.locator('[data-feature=wheel] .new-feature-badge').count()==0
  await page.locator('.map-dock [data-action=gems]').click()
  assert await page.locator('.tutorial-step').inner_text()=='1 / 3'
  assert await page.locator('.gems').evaluate("e=>e.classList.contains('guide-highlight')")
  await page.locator('[data-action=birdNext]').click();assert await page.locator('.tutorial-step').inner_text()=='2 / 3'
  await page.locator('[data-action=birdNext]').click();assert await page.locator('.tutorial-step').inner_text()=='3 / 3'
  await page.screenshot(path='artifacts/garden-tutorial-bounce.png')
  await page.locator('[data-action=birdNext]').click()
  assert await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).GuideSeen.includes('garden')")
  await page.locator('[data-action=home]').click();await page.locator('#app [data-action=map]').click()
  assert await page.locator('[data-feature=garden] .new-feature-badge').count()==0
  await page.evaluate("const s=JSON.parse(localStorage.getItem('meadow-peaks-profile'));s.Unlocked=7;s.GuideSeen.push('missions','chest');localStorage.setItem('meadow-peaks-profile',JSON.stringify(s))")
  await page.reload();await page.locator('.map-dock [data-action=wheel]').click()
  assert await page.locator('.tutorial-step').inner_text()=='1 / 2'
  await page.locator('.shell-close').click()
  assert await page.locator('[data-feature=wheel] .new-feature-badge').count()==1
  await page.locator('.map-dock [data-action=wheel]').click()
  assert await page.locator('.tutorial-step').inner_text()=='1 / 2'
  await page.locator('[data-action=birdNext]').click();await page.locator('[data-action=birdNext]').click()
  await page.locator('.shell-close').click();await page.locator('[data-action=previewCurrent]').click();await page.locator('[data-action="level:7"]').click()
  wild=page.locator('.tool[data-action="helper:wild"]')
  assert await wild.locator('.new-feature-badge').count()==1
  before=await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Wilds")
  await wild.click()
  assert await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Wilds")==before
  await page.locator('[data-action=birdDismiss]').click()
  assert await wild.locator('.new-feature-badge').count()==0
  await wild.click()
  assert await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Wilds")==before-1
  await page.wait_for_timeout(1000)
  assert await page.locator('.button-sparkle').count()==0
  await page.emulate_media(reduced_motion='reduce')
  assert await page.evaluate("async()=>{const {motionAllowed}=await import('./celebration-effects.mjs');return !motionAllowed()}")
  assert not errors,errors;await b.close()
 print('PASS: unlocked-only badges, targeted 3-step garden tutorial, saved acknowledgement, interrupted wheel tutorial stays new, reduced-motion suppression.')
asyncio.run(main())
