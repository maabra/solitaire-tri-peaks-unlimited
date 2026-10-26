"""Native menu scrollbar drags must not be mistaken for map panning."""
import asyncio,os
from pathlib import Path
from playwright.async_api import async_playwright

async def main():
 out=Path('artifacts/map-input');out.mkdir(exist_ok=True)
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True,ignore_default_args=['--hide-scrollbars'])
  page=await browser.new_page(viewport={'width':1440,'height':900},reduced_motion='reduce')
  errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'));await page.locator('.story-node.current').wait_for()
  await page.wait_for_timeout(150)
  before=await page.locator('.map-viewport').evaluate('e=>[e.scrollLeft,e.scrollTop]')
  menu=await page.locator('.map-dock-scroll').evaluate('e=>{const r=e.getBoundingClientRect();return {x:r.x,y:r.y,w:r.width,h:r.height,client:e.clientWidth,thumb:e.clientHeight*e.clientHeight/e.scrollHeight}}')
  x=menu['x']+menu['w']-5;y=menu['y']+max(18,menu['thumb']/2)
  await page.mouse.move(x,y);await page.mouse.down();await page.mouse.move(x,y+120,steps=12);await page.mouse.up()
  assert await page.locator('.map-dock-scroll').evaluate('e=>e.scrollTop>0'),'Menu scrollbar did not move'
  assert before==await page.locator('.map-viewport').evaluate('e=>[e.scrollLeft,e.scrollTop]'),'Scrollbar drag moved map'
  await page.mouse.move(menu['x']+8,menu['y']+100);await page.mouse.down();await page.mouse.move(menu['x']+8,menu['y']+170,steps=8);await page.mouse.up()
  assert before==await page.locator('.map-viewport').evaluate('e=>[e.scrollLeft,e.scrollTop]'),'Menu background drag moved map'
  body=await page.locator('body').bounding_box()
  await page.mouse.move(body['x']+145,body['y']+120);await page.mouse.down();await page.mouse.move(body['x']+145,body['y']+200,steps=6);await page.mouse.up()
  assert before==await page.locator('.map-viewport').evaluate('e=>[e.scrollLeft,e.scrollTop]'),'Dragging wood outside map moved map'
  await page.mouse.move(body['x']+350,body['y']+250);await page.mouse.down();await page.mouse.move(body['x']+350,body['y']+370,steps=10);await page.mouse.up()
  after=await page.locator('.map-viewport').evaluate('e=>[e.scrollLeft,e.scrollTop]')
  assert before!=after,'Map surface no longer pans'
  await page.mouse.move(body['x']+400,body['y']+400)
  assert after==await page.locator('.map-viewport').evaluate('e=>[e.scrollLeft,e.scrollTop]'),'Map kept dragging after release'
  # Wheel over an icon and at a menu boundary must never move the map/page.
  await page.locator('.map-dock-scroll').evaluate('e=>e.scrollTop=0')
  before=await page.locator('.map-viewport').evaluate('e=>e.scrollTop')
  await page.mouse.move(menu['x']+45,menu['y']+80);await page.mouse.wheel(0,240);await page.wait_for_timeout(100)
  assert await page.locator('.map-dock-scroll').evaluate('e=>e.scrollTop>0'),'Wheel did not scroll menu'
  assert before==await page.locator('.map-viewport').evaluate('e=>e.scrollTop'),'Menu wheel moved map'
  await page.locator('.map-dock-scroll').evaluate('e=>e.scrollTop=e.scrollHeight')
  await page.mouse.wheel(0,300);await page.wait_for_timeout(100)
  assert before==await page.locator('.map-viewport').evaluate('e=>e.scrollTop'),'Menu end chained wheel into map'
  await page.mouse.move(body['x']+500,body['y']+300);await page.mouse.wheel(0,-240);await page.wait_for_timeout(100)
  assert before!=await page.locator('.map-viewport').evaluate('e=>e.scrollTop'),'Wheel did not scroll map'
  await page.screenshot(path=str(out/'refined-frames.png'))
  assert not errors,errors
  await browser.close()
 print('PASS: menu scrollbar scrolls only menu, menu dragging does not pan, map dragging works and release stops panning.')

asyncio.run(main())
