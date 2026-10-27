"""Picnic tasks fit the tray and claim real, one-time rewards without inflated progress."""
import asyncio,os
from pathlib import Path
from playwright.async_api import async_playwright

# Route through the real UI dispatcher so menu framing and tutorial lifecycle are included.
async def action(page,name):
 await page.evaluate("a=>{const b=document.createElement('button');b.dataset.action=a;document.body.append(b);b.click();b.remove()}",name)

# Check a prepared ready/pending mixture, then collect all rewards and the empty state.
async def main():
 out=Path('artifacts/picnic-menu');out.mkdir(exist_ok=True)
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True)
  for width,height,tutorial in [(1440,900,False),(1280,720,True),(700,432,True)]:
   page=await browser.new_page(viewport={'width':width,'height':height},reduced_motion='reduce');errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
   await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
   await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'))
   await page.evaluate("async tutorial=>{const {Session}=await import('./session.mjs');const s=new Session();s.p.Unlocked=12;s.p.GuideSeen=tutorial?[]:['missions'];s.events.a.Daily.Values={wins:4,clears:86,draws:2};s.save()}",tutorial);await page.reload()
   await action(page,'ev:missions')
   if tutorial:assert await page.locator('.bird-guide.in-panel[data-guide-panel=lesson]').count()==1
   await page.evaluate("async()=>{const i=new Image();i.src='assets/menu-place-picnic.png';await i.decode()}")
   assert await page.locator('#dialog').get_attribute('data-place')=='picnic'
   assert await page.locator('.event-missions .activity-tabs').count()==0
   assert await page.locator('.picnic-task').count()==3
   assert await page.locator('.picnic-progress small').all_text_contents()==['3 / 3','60 / 60','2 / 20']
   assert await page.locator('.picnic-claim:enabled').count()==2
   assert await page.locator('.picnic-claim:disabled').inner_text()=='Collect reward'
   assert await page.locator('#dialog').evaluate("e=>e.scrollWidth<=e.clientWidth+1")
   assert await page.locator('.picnic-task').evaluate_all("es=>es.every(e=>e.scrollWidth<=e.clientWidth+1)")
   assert await page.locator('.picnic-footer').is_visible()
   if width>=1280 and not tutorial:
    assert await page.locator('.picnic-footer').evaluate("e=>{const a=e.getBoundingClientRect(),b=e.closest('.dialog-content').getBoundingClientRect();return a.bottom<=b.bottom+1}"),'Tasks or footer need scrolling at 720p'
   if tutorial:await page.locator('.picnic-footer').evaluate("e=>e.scrollIntoView({block:'end'})")
   if width>=1280:assert await page.locator('.picnic-footer').evaluate("e=>e.getBoundingClientRect().bottom<=e.closest('dialog').getBoundingClientRect().bottom-64"),'Footer overlaps painted wooden rim'
   await page.screenshot(path=str(out/f'picnic-{width}.png'))
   await action(page,'birdDismiss')
   before=await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Coins")
   await page.locator('[data-action="ev:mission-0"]').click()
   assert await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Coins")==before+60
   await action(page,'ev:mission-0')
   assert await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Coins")==before+60,'Repeated claim duplicated coins'
   assert await page.locator('.picnic-task').count()==3
   await page.evaluate("async()=>{const {Session}=await import('./session.mjs');const s=new Session();s.events.a.Daily.Values={wins:4,clears:86,draws:25,spent:200,stars:20,packs:3};s.save()}");await page.reload();await action(page,'ev:missions')
   for _ in range(5):await page.locator('.picnic-claim:enabled').first.click()
   await page.locator('.picnic-complete').wait_for();assert '6 / 6 collected' in await page.locator('.picnic-summary').inner_text()
   assert await page.locator('[data-action="ev:eventPlay"]').is_visible()
   assert not errors,errors;await page.close()
  await browser.close()
 print('PASS: fixed picnic tray contains its scrolling paper; progress is capped; ready/pending states and tutorial fit; one-time claims reveal new tasks; six claims reach the complete state.')
asyncio.run(main())
