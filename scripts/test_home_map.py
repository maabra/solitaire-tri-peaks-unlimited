"""Home controls fit the game, and scenery moves with the map instead of HUD."""
import asyncio,os
from pathlib import Path
from playwright.async_api import async_playwright
async def main():
 out=Path('artifacts/home-map');out.mkdir(exist_ok=True)
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True)
  for width,height in [(1440,900),(700,432)]:
   page=await b.new_page(viewport={'width':width,'height':height},reduced_motion='reduce');await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')");await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'))
   await page.locator('header [data-action=home]').click()
   assert await page.locator('#app').evaluate('e=>e.scrollHeight<=e.clientHeight+2'),'Home needs internal scrolling'
   assert await page.locator('#app button').evaluate_all('es=>es.every(e=>{const a=e.getBoundingClientRect(),b=document.body.getBoundingClientRect();return a.left>=b.left&&a.right<=b.right&&a.top>=b.top&&a.bottom<=b.bottom})'),'A home action is cut off'
   assert await page.locator('.home-place').count()==3
   await page.evaluate("async()=>{const image=new Image();image.src='assets/menu-parchment-frame.png';await image.decode()}");await page.wait_for_timeout(100)
   await page.screenshot(path=str(out/f'home-{width}.png'))
   if width==1440:
    await page.locator('header [data-action=settings]').click();assert await page.locator('[data-action=export],[data-action=import]').count()==2;await page.locator('.shell-close').click()
    await page.locator('.home-intro [data-action=map]').click()
    await page.wait_for_function("document.querySelector('.adventure-map>.map-scenery')?.style.backgroundImage.includes('map-meadow-v2.png')")
    scenery=page.locator('.map-scenery');road=page.locator('.map-trail');dock=page.locator('.map-dock');frame=page.locator('.map-picture-frame')
    before=[await x.bounding_box() for x in [scenery,road,dock,frame]]
    await page.locator('.map-viewport').evaluate('e=>e.scrollTop-=300')
    after=[await x.bounding_box() for x in [scenery,road,dock,frame]]
    assert abs(after[0]['y']-before[0]['y']-300)<1
    assert abs(after[1]['y']-before[1]['y']-300)<1
    assert before[2:]==after[2:],'Map menu or frame moved with scenery'
    await page.screenshot(path=str(out/'scrolling-map.png'))
   await page.close()
  await b.close()
 print('PASS: home fits all actions at full game size; road and painting scroll together while menu and frame stay fixed.')
asyncio.run(main())
