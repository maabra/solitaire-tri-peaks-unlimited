"""Changing motion preferences stops feedback already in flight without script errors."""
import asyncio, os
from playwright.async_api import async_playwright

async def main():
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True)
  page=await browser.new_page(reduced_motion='no-preference')
  errors=[]
  page.on('pageerror',lambda error:errors.append(str(error)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'))
  await page.evaluate("""async()=>{
   const {bounce,sparkle}=await import('./celebration-effects.mjs');
   const button=document.createElement('button');button.id='motion-probe';button.textContent='Feedback';document.body.append(button);
   bounce(button);sparkle(button);
   if(!button.getAnimations().length)throw Error('Bounce did not start');
   document.body.classList.add('reduced');
  }""")
  await page.wait_for_timeout(100)
  assert await page.locator('#motion-probe').evaluate('e=>e.getAnimations().length')==0
  assert await page.locator('.button-sparkle').count()==0
  await page.evaluate("""async()=>{
   document.body.classList.remove('reduced');
   const {bounce}=await import('./celebration-effects.mjs');bounce(document.querySelector('#motion-probe'));
  }""")
  await page.emulate_media(reduced_motion='reduce')
  await page.wait_for_function("document.querySelector('#motion-probe').getAnimations().length===0")
  assert not errors,errors
  await browser.close()
 print('PASS: saved and system reduced-motion preferences cancel active feedback and clean up sparks without rejected promises.')

asyncio.run(main())
