"""The disc spins under a fixed pointer, rim lights chase and the saved prize matches its landing."""
import asyncio,os
from pathlib import Path
from playwright.async_api import async_playwright

# Use the normal dispatcher to include dialog lifecycle and spin guards.
async def action(page,name):
 await page.evaluate("a=>{const b=document.createElement('button');b.dataset.action=a;document.body.append(b);b.click();b.remove()}",name)

# Check actual saved rewards, repeat spins, reopening mid-spin and reduced-motion presentation.
async def main():
 out=Path('artifacts/wheel-disc');out.mkdir(exist_ok=True)
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True)
  for reduced in [False,True]:
   page=await browser.new_page(viewport={'width':1440,'height':900},reduced_motion='reduce' if reduced else 'no-preference');errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
   await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
   await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'))
   await page.evaluate("async()=>{const {Session}=await import('./session.mjs'),{features}=await import('./journey-guide.mjs');const s=new Session();s.p.Garden=10;s.p.Coins=300;s.p.Unlocked=12;s.p.HighestPlayedLevel=12;s.p.GuideSeen=features.map(f=>f.id);s.save()}");await page.reload();await action(page,'wheel')
   await page.evaluate("async()=>{const {Session}=await import('./session.mjs');const spin=Session.prototype.spinWheel;let i=0;Session.prototype.spinWheel=function(){return spin.call(this,[4,7,0][i++])}}")
   assert await page.locator('.wheel-lamp').count()==8
   assert await page.locator('.wheel-lamp').evaluate_all("(es,reduced)=>es.every(e=>reduced?getComputedStyle(e).animationName==='none':e.getAnimations().length>0)",reduced)
   await page.locator('[data-action=spinWheel]').click()
   if not reduced:
    await page.wait_for_timeout(150)
    assert await page.locator('.wheel-rotor').evaluate('e=>e.getAnimations().length>0')
    assert await page.locator('.wheel-needle').evaluate("e=>getComputedStyle(e).transform==='none'&&e.getAnimations().length===0")
    assert await page.locator('[data-action=spinWheel]').is_disabled()
    await page.screenshot(path=str(out/'spinning.png'))
   await page.locator('[data-action=claimWheel]').wait_for()
   assert await page.locator('.prize-wheel').get_attribute('data-winning-index')=='4'
   assert await page.locator('.wheel-rotor').evaluate('e=>(Number(e.dataset.angle)+4*45)%360===0')
   assert '100' in await page.locator('#wheel-result').inner_text()
   assert await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Coins")==400
   assert await page.locator('.prize-wheel').evaluate("e=>{const a=e.getBoundingClientRect(),b=e.closest('.dialog-content').getBoundingClientRect();return a.left>=b.left+6&&a.right<=b.right-6&&a.top>=b.top+12&&a.bottom<=b.bottom-8}"),'Wheel clips the painted rim'
   await page.screenshot(path=str(out/f'landed-{reduced}.png'))
   await page.locator('[data-action=spinWheel]').click();await page.locator('[data-action=claimWheel]').wait_for()
   assert await page.locator('.prize-wheel').get_attribute('data-winning-index')=='7'
   assert await page.locator('.wheel-rotor').evaluate('e=>(Number(e.dataset.angle)+7*45)%360===0')
   assert await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Coins")==440
   await page.locator('[data-action=spinWheel]').click()
   if not reduced:await action(page,'close');await action(page,'wheel')
   await page.locator('[data-action=claimWheel]').wait_for()
   assert await page.locator('.prize-wheel').get_attribute('data-winning-index')=='0'
   assert await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Coins")==465
   assert await page.locator('[data-action=spinWheel]').is_disabled()
   await action(page,'birdTopic:wheel');await action(page,'birdGo')
   assert await page.locator('.bird-guide.in-panel').count()==1
   assert await page.locator('.prize-wheel').evaluate("e=>e.getBoundingClientRect().bottom<=e.closest('.dialog-content').getBoundingClientRect().bottom-8"),'Tutorial cuts off the wheel'
   await page.screenshot(path=str(out/f'tutorial-{reduced}.png'))
   await page.locator('[data-action=timedWheel]').click();await page.locator('[data-action=claimWheel]').wait_for()
   index=int(await page.locator('.prize-wheel').get_attribute('data-winning-index'))
   assert await page.locator('.wheel-rotor').evaluate('(e,i)=>(Number(e.dataset.angle)+i*45)%360===0',index)
   assert await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).WheelSpinsUsed")==3,'Daily spin consumed an earned spin'
   assert await page.locator('[data-action=timedWheel]').is_disabled()
   assert 'Ready' not in await page.locator('[data-action=timedWheel]').inner_text()
   assert await page.locator('.wheel-controls button').evaluate_all("es=>es.every(e=>{const a=e.getBoundingClientRect(),b=e.closest('.dialog-content').getBoundingClientRect();return a.top>=b.top&&a.bottom<=b.bottom-4})"),'Result buttons clip with the tutorial open'
   assert not errors,errors;await page.close()
  await browser.close()
 print('PASS: rotating disc/fixed pointer, eight animated rim lights, exact saved prize alignment, repeat spins, reopen recovery and static reduced motion.')
asyncio.run(main())
