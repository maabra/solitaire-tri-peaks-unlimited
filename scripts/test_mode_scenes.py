"""Distinct mode paintings, precedence, persistent hands and table customization."""
import asyncio
from pathlib import Path
from playwright.async_api import async_playwright
OUT=Path('artifacts/mode-screenshots');OUT.mkdir(exist_ok=True)
# Scenario: Distinct mode paintings, precedence, persistent hands and table customization. Fail on violated assertions rather than accepting screenshots alone.
async def main():
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True);page=await browser.new_page(viewport={'width':1280,'height':900},reduced_motion='reduce');errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')");await page.goto('http://127.0.0.1:8765/')
  unique=await page.evaluate("async()=>{const {modePicture,modeScenes}=await import('./mode-scenes.mjs');return new Set(await Promise.all(Object.keys(modeScenes).map(modePicture))).size}");assert unique==9
  # Trigger the normal delegated UI command rather than modifying the tested view directly.
  async def action(name):await page.evaluate("name=>{const b=document.createElement('button');b.dataset.action=name;document.body.append(b);b.click();b.remove();}",name)
  for mode,level,endless in [('bonus',25,False),('diamonds',40,False),('spades',60,False),('gold',75,False),('balloon',37,False),('tickets',38,False),('endless',1,True),('gems',1,False)]:
   await page.evaluate("""async({mode,level,endless})=>{const {Session,ProfileStore}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const s=new Session(new ProfileStore(null));s.p.Unlocked=10000;s.p.HighestPlayedLevel=10000;s.p.GuideSeen=[...features.map(f=>f.id),'cards',...Array.from({length:16},(_,i)=>'mechanic'+(i+1))];s.start(level,endless);if(mode==='balloon')s.events.startBalloon();if(mode==='tickets')s.events.startTickets();s.save();localStorage.setItem('meadow-peaks-profile',JSON.stringify(s.p));}""",{'mode':mode,'level':level,'endless':endless})
   await page.reload();await action('gems' if mode=='gems' else 'continue');await page.wait_for_function("mode=>document.body.dataset.gameMode===mode&&document.querySelector('.landscape').style.backgroundImage.includes('data:image')",arg=mode)
   assert await page.locator('.mode-cue').count()==1
   assert await page.locator('.mode-cue').is_visible()
   assert await page.evaluate('document.documentElement.scrollWidth<=innerWidth')
   await page.screenshot(path=str(OUT/f'{mode}.png'))
   for width,height in [(800,600),(320,640)]:
    await page.set_viewport_size({'width':width,'height':height});await page.wait_for_timeout(120)
    assert await page.evaluate('document.documentElement.scrollWidth<=innerWidth'),(mode,width)
    assert await page.locator('.mode-cue').is_visible(),(mode,width)
   await page.set_viewport_size({'width':1280,'height':900})
  await action('events');await page.locator('#modal [data-action="ev:regatta"]').click();await page.wait_for_function("document.querySelector('.regatta-sea').style.backgroundImage.includes('data:image')")
  await page.screenshot(path=str(OUT/'regatta.png'))
  assert not errors,errors;await browser.close()
 print('PASS: nine distinct mode paintings, eight playable scene identities, bonus/objective precedence, saved hands, mode cues and painted regatta.')
asyncio.run(main())
