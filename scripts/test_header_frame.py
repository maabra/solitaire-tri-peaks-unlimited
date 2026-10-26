"""The header and its icon artwork must clear the carved game and map rails."""
import asyncio,os
from pathlib import Path
from playwright.async_api import async_playwright
async def main():
 out=Path('artifacts/header-frame');out.mkdir(exist_ok=True)
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True)
  for width,height in [(1440,900),(1280,720),(572,720)]:
   page=await b.new_page(viewport={'width':width,'height':height},reduced_motion='reduce');await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')");await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'))
   for action in ['map','home','gems','continue']:
    if action=='continue':
     await page.evaluate("()=>{const e=document.createElement('button');e.dataset.action='level:1';document.body.append(e);e.click();e.remove()}")
    else:await page.evaluate("a=>{const e=document.createElement('button');e.dataset.action=a;document.body.append(e);e.click();e.remove()}",action)
    result=await page.evaluate('''()=>{
     const body=document.body.getBoundingClientRect(),header=document.querySelector('header'),nodes=[...header.querySelectorAll('nav>button,nav>button>.game-icon')].filter(e=>e.getClientRects().length);
     const clear=nodes.every(e=>{const r=e.getBoundingClientRect();return r.top>=body.top+18&&r.bottom+4<=body.top+74&&r.left>=body.left+18&&r.right<=body.right-18});
     const contained=[...header.querySelectorAll('.wallet>.game-icon,.heart-counter>.game-icon')].every(e=>{const a=e.getBoundingClientRect(),b=e.parentElement.getBoundingClientRect();return a.left>=b.left&&a.right<=b.right&&a.top>=b.top&&a.bottom<=b.bottom});
     return {clear,contained};
    }''')
    assert result['clear'] and result['contained'],(width,action,result)
    if width==1440 and action=='map':await page.screenshot(path=str(out/'safe-header.png'))
   await page.close()
  await b.close()
 print('PASS: header controls, shadows and icon art clear the frame across map/home/garden/play and desktop/small windows.')
asyncio.run(main())
