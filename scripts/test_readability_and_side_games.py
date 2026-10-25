"""Upright card visuals, crisp UI over blurred scenery, and connected activity rewards."""
import asyncio
from pathlib import Path
from playwright.async_api import async_playwright
OUT=Path('artifacts/connected-screenshots');OUT.mkdir(exist_ok=True)
async def main():
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True);page=await browser.new_page(viewport={'width':1280,'height':850},reduced_motion='reduce');errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')");await page.goto('http://127.0.0.1:8765/')
  await page.evaluate("""async()=>{const {Session,ProfileStore}=await import('./session.mjs'),{features}=await import('./journey-guide.mjs');const s=new Session(new ProfileStore(null));s.p.Unlocked=50000;s.p.HighestPlayedLevel=50000;s.p.GuideSeen=[...features.map(f=>f.id),'cards',...Array.from({length:16},(_,i)=>'mechanic'+(i+1))];s.start(3);s.hand.Deal.Layout.forEach(c=>c.Angle+=180);s.side.a.Rolls=2;s.side.a.Seeds=6;s.save();localStorage.setItem('meadow-peaks-profile',JSON.stringify(s.p));}""")
  await page.reload()
  async def action(name):await page.evaluate("name=>{const b=document.createElement('button');b.dataset.action=name;document.body.append(b);b.click();b.remove();}",name)
  await action('continue');await page.wait_for_selector('.painted-table')
  assert await page.locator('.board .card').evaluate_all("cards=>cards.every(c=>Math.abs(parseFloat(c.style.getPropertyValue('--angle')))<=90)")
  assert 'blur(' in await page.locator('.landscape').evaluate('e=>getComputedStyle(e).filter')
  assert await page.locator('.board').evaluate('e=>getComputedStyle(e).filter')=='none'
  assert await page.locator('.chapter-chip .game-icon').count()==1
  assert await page.locator('.star-icons .game-icon').count()==3
  await page.screenshot(path=str(OUT/'upright-header.png'))
  await action('events');await page.locator('#modal [data-action="ev:trail"]').click()
  before=await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Coins")
  await page.locator('[data-action="ev:trailRoll"]').click()
  profile=await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile'))")
  assert profile['SideAdventures']['Rolls']==1 and profile['Coins']>before
  await page.screenshot(path=str(OUT/'treasure-trail.png'))
  await page.locator('#modal [data-action="ev:hub"]').click();await page.locator('#modal [data-action="ev:memory"]').click()
  deck=await page.evaluate("""async()=>{const {SideActivities}=await import('./side-activities.mjs');return new SideActivities({p:JSON.parse(localStorage.getItem('meadow-peaks-profile'))}).memoryCards();}""")
  pair=[i for i,n in enumerate(deck) if n==0]
  before=await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Coins")
  for i in pair:await page.locator(f'[data-action="ev:memoryFlip-{i}"]').click()
  saved=await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile'))")
  assert saved['Coins']==before+20 and saved['SideAdventures']['Seeds']==5
  assert await page.locator('.memory-tile.matched').count()==2
  for width,height in [(1280,850),(390,844),(320,640)]:
   await page.set_viewport_size({'width':width,'height':height});assert await page.locator('#modal').evaluate('e=>e.scrollWidth<=e.clientWidth+2')
   await page.screenshot(path=str(OUT/f'matching-garden-{width}.png'))
  await page.reload();saved=await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile'))");assert len(saved['SideAdventures']['Memory']['Matched'])==2
  assert not errors,errors;await browser.close()
 print('PASS: upright card faces with unchanged deal angles, icon-led header, scenery-only blur, trail rewards, memory pairs, persistence and narrow activity layouts.')
asyncio.run(main())
