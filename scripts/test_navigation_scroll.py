"""Back returns to the same place in a book page rather than its top."""
import asyncio,os
from playwright.async_api import async_playwright
async def action(page,name):
 await page.evaluate("a=>{const e=document.createElement('button');e.dataset.action=a;(document.querySelector('#dialog').open?document.querySelector('#modal'):document.body).append(e);e.click();e.remove()}",name)
async def main():
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True);page=await b.new_page(viewport={'width':1440,'height':900},reduced_motion='reduce');await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')");await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'))
  await page.evaluate("async()=>{const {Session}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const s=new Session();s.p.Unlocked=50001;s.p.HighestPlayedLevel=50001;s.p.ChapterGifts100=[1];s.p.Customization.Unlocked=[0,1];s.p.GuideSeen=features.map(f=>f.id);s.save()}");await page.reload()
  for parent,child in [('settings','ev:customize'),('journal','events')]:
   if await page.locator('#dialog').evaluate('e=>e.open'):await page.locator('.shell-close').click()
   await action(page,parent);content=page.locator('.dialog-content');await content.evaluate('e=>e.scrollTop=e.scrollHeight')
   button=page.locator(f'.dialog-content [data-action="{child}"]');await button.scroll_into_view_if_needed();before=await content.evaluate('e=>e.scrollTop');assert before>0
   await button.click();await page.locator('[data-action=menuBack]').click();await page.wait_for_timeout(100)
   after=await page.locator('.dialog-content').evaluate('e=>e.scrollTop');assert abs(before-after)<2,(parent,before,after)
  await b.close()
 print('PASS: Settings/Card studio and journal/Activities Back restore the book page scroll position.')
asyncio.run(main())
