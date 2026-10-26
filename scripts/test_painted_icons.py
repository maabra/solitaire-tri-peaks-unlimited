"""Painted replacements load and retain isolated, correctly shaped viewports."""
import asyncio,os
from pathlib import Path
from playwright.async_api import async_playwright
async def main():
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True)
  page=await browser.new_page(viewport={'width':1440,'height':900},reduced_motion='reduce')
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'))
  result=await page.evaluate('''async()=>{
   const {gameIcon}=await import('./game-art.mjs');const {paintedIcons}=await import('./painted-icons.mjs');const {gemMarkup}=await import('./gem-art.mjs');
   const host=document.createElement('div');host.innerHTML=Object.keys(paintedIcons).map(n=>gameIcon(n,40)).join('')+Array.from({length:6},(_,i)=>gemMarkup(i)).join('');
   const icons=[...host.querySelectorAll('[data-art="imagegen"]')];
   const files=[...new Set(icons.map(e=>e.querySelector('image').getAttribute('href')))];
   const loaded=await Promise.all(files.map(src=>new Promise(resolve=>{const image=new Image();image.onload=()=>resolve([src,image.naturalWidth,image.naturalHeight]);image.onerror=()=>resolve([src,0,0]);image.src=src})));
   return {count:icons.length,files:loaded,paths:host.querySelectorAll('path').length,square:icons.every(e=>{const v=e.viewBox.baseVal;return v.width===v.height}),activities:['dice','pairs'].every(n=>host.querySelector(`[data-icon="${n}"][data-art="imagegen"]`))};
  }''')
  assert result['count']==42 and result['paths']==0 and result['square'] and result['activities'],result
  assert all(w==1536 and h==1024 for _,w,h in result['files']),result
  await browser.close()
 print('PASS: 36 painted interface icons and six jewels load, with isolated square viewports and no vector fallbacks.')
asyncio.run(main())
