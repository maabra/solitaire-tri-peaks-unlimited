import asyncio
from pathlib import Path
from playwright.async_api import async_playwright
# Scenario: Exercise volume controls in an isolated browser session. Fail on violated assertions rather than accepting screenshots alone.
async def main():
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True)
  page=await b.new_page();errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto('http://127.0.0.1:8765/')
  await page.locator('[data-action=settings]').click()
  for width,height in [(1280,800),(390,844),(320,640)]:
   await page.set_viewport_size({'width':width,'height':height})
   for key in ['EffectsVolume','MusicVolume']:
    slider=page.locator(f'[data-audio-volume={key}]');await slider.scroll_into_view_if_needed()
    rect=await slider.bounding_box();assert rect['width']>=120,(width,rect)
    assert await slider.evaluate('e=>e.getBoundingClientRect().right<=innerWidth')
    await page.mouse.click(rect['x']+rect['width']*.75,rect['y']+rect['height']/2)
    value=int(await slider.input_value());assert 65<=value<=85,(width,key,value)
    assert await slider.locator('..').locator('output').inner_text()==f'{value}%'
    await slider.focus();await page.keyboard.press('Home');assert await slider.input_value()=='0'
    await page.keyboard.press('End');assert await slider.input_value()=='100'
    await page.keyboard.press('ArrowLeft');assert await slider.input_value()=='99'
   assert await page.locator('#dialog').evaluate('e=>e.scrollWidth<=e.clientWidth+2')
  await page.reload();await page.locator('[data-action=settings]').click()
  for key in ['EffectsVolume','MusicVolume']:
   assert await page.locator(f'[data-audio-volume={key}]').input_value()=='99'
  await page.set_viewport_size({'width':390,'height':844})
  await page.locator('.audio-sliders').scroll_into_view_if_needed()
  Path('artifacts/settings-screenshots').mkdir(exist_ok=True)
  await page.screenshot(path='artifacts/settings-screenshots/volume-390.png')
  assert not errors,errors
  await b.close()
 print('PASS: real pointer and keyboard volume changes, 0/100 endpoints, live percentage, reload persistence and three window sizes.')
asyncio.run(main())
