"""Painted adventure, collection and picnic menus keep all content on their paper."""
import asyncio,os
from pathlib import Path
from playwright.async_api import async_playwright

# Navigate through the normal command path so reused dialogs reset their surface and scroll state.
async def action(page,name):
 await page.evaluate("a=>{document.querySelector('#dialog').close();const b=document.createElement('button');b.dataset.action=a;document.body.append(b);b.click();b.remove()}",name)

# Compare the painted frame and navigation before/after scrolling an overflowing menu page.
async def main():
 out=Path('artifacts/menu-surfaces');out.mkdir(exist_ok=True)
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True)
  for width,height in [(1440,900),(868,748),(700,432)]:
   page=await browser.new_page(viewport={'width':width,'height':height},reduced_motion='reduce');errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
   await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
   await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'))
   await page.evaluate("async()=>{const {Session}=await import('./session.mjs'),{features}=await import('./journey-guide.mjs');const s=new Session();s.p.Unlocked=10000;s.p.HighestPlayedLevel=10000;s.p.GuideSeen=features.map(f=>f.id);s.save()}");await page.reload()
   for route,place in [('events','adventures'),('ev:album','collections'),('ev:travel','adventures'),('ev:missions','picnic')]:
    await action(page,route);dialog=page.locator('#dialog');assert await dialog.get_attribute('data-place')==place
    await page.evaluate("async p=>{const i=new Image();i.src='assets/menu-place-'+p+'.png';await i.decode()}",place)
    assert await dialog.evaluate("e=>{const r=e.getBoundingClientRect(),c=e.querySelector('.dialog-content').getBoundingClientRect(),n=e.querySelector('.menu-navigation').getBoundingClientRect(),s=getComputedStyle(e);return s.overflowY==='hidden'&&s.borderImageSource==='none'&&s.backgroundSize==='100% 100%'&&e.scrollHeight<=e.clientHeight+1&&n.left>=r.left+32&&n.right<=r.right-32&&c.left>=r.left+32&&c.right<=r.right-32&&c.top>=n.bottom&&c.bottom<=r.bottom-56}"),'Content escapes the painted paper: '+route
    before=await dialog.bounding_box();nav=await page.locator('.menu-navigation').bounding_box();content=page.locator('.dialog-content')
    await page.screenshot(path=str(out/f'{place}-{route.replace(":","-")}-{width}-top.png'))
    if await content.evaluate('e=>e.scrollHeight>e.clientHeight+1'):
     await content.hover();await page.mouse.wheel(0,1000);await page.wait_for_timeout(100)
     assert await content.evaluate('e=>e.scrollTop>0'),'Paper ignores wheel input: '+route
     assert await dialog.bounding_box()==before and await page.locator('.menu-navigation').bounding_box()==nav,'Frame/navigation moved with the paper'
     assert await dialog.evaluate('e=>e.scrollTop===0')
     await content.evaluate('e=>e.scrollTop=e.scrollHeight')
     await page.screenshot(path=str(out/f'{place}-{width}-bottom.png'))
   assert not errors,errors;await page.close()
  await browser.close()
 print('PASS: adventure, collection and picnic menus occupy complete fixed paintings; content stays on paper; wheel scrolling leaves frames and navigation fixed.')
asyncio.run(main())
