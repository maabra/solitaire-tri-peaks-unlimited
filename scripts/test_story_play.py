"""Exercise the card-first layout and its real, incremental browser updates."""
import asyncio
from pathlib import Path
from playwright.async_api import async_playwright

async def main():
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True)
  page=await browser.new_page(reduced_motion='reduce');errors=[]
  page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  for width,height,level in [(1280,800,351),(1366,768,3),(800,600,9600),(390,844,2),(320,640,351)]:
   await page.set_viewport_size({'width':width,'height':height})
   await page.goto('http://127.0.0.1:8765/')
   path=await page.evaluate("""async level=>{const {Session}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const {helpers}=await import('./core.mjs');const s=new Session();s.p.Unlocked=level;s.p.Hearts=20;s.p.GuideSeen=[...features.map(f=>f.id),'cards',...Array.from({length:17},(_,i)=>'mechanic'+i),...helpers.map(h=>'helper-'+h.Id)];s.start(level,false,12345);s.save();return s.hand.Deal.Solution}""",level)
   await page.reload();await page.locator('[data-action=previewCurrent]').click();await page.locator(f'[data-action="level:{level}"]').click();await page.wait_for_timeout(250)
   bounds=await page.evaluate("""()=>{const board=document.querySelector('.board').getBoundingClientRect();return {overflow:document.documentElement.scrollWidth>innerWidth+2,cards:[...document.querySelectorAll('.board .card')].filter(c=>{const r=c.getBoundingClientRect();return r.left<board.left-1||r.right>board.right+1||r.top<board.top-1||r.bottom>board.bottom+1}).map(c=>c.dataset.action)}}""")
   assert not bounds['overflow'] and not bounds['cards'],(width,level,bounds)
   assert await page.locator('[data-action=favorite]').is_visible()
   assert await page.locator('[data-action=share]').is_visible()
   assert await page.locator('#hint-button .game-icon').count()==1
   for step in path[:6]:
    if step['Index']<0:
     await page.wait_for_function("!document.querySelector('[data-action=draw]').disabled")
     await page.locator('[data-action=draw]').click()
    else:await page.locator(f'[data-action="card:{step["Index"]}"]').click()
    await page.wait_for_timeout(40)
    assert await page.evaluate("""()=>{const h=JSON.parse(localStorage.getItem('meadow-peaks-profile')).Active;return !!h}""")
    suit=await page.evaluate("""()=>{const waste=document.querySelector('.hand-row .pile:nth-child(3) .card');return {data:+waste.dataset.suit,rank:waste.querySelector('.rank small').textContent,color:getComputedStyle(waste.querySelector('.face-rank')).color}}""")
    assert suit['rank']==['♠','♥','♦','♣'][suit['data']],(width,suit)
   Path('artifacts/story-play').mkdir(exist_ok=True)
   await page.screenshot(path=f'artifacts/story-play/{width}-{level}.png')
  # Dismissing onboarding must not resize the hand or leave a stale pointer.
  await page.set_viewport_size({'width':1280,'height':800})
  await page.evaluate("""async()=>{const {Session}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const s=new Session();s.p.Unlocked=1;s.p.GuideSeen=features.map(f=>f.id);s.start(1,false,12345);s.save()}""")
  await page.reload();await page.locator('[data-action=previewCurrent]').click();await page.locator('[data-action="level:1"]').click();await page.locator('.guide-hand').wait_for();await page.wait_for_timeout(200)
  before=await page.locator('.board').bounding_box()
  assert await page.locator('.bird-guide.in-play [data-action=birdGo]').count()==0
  await page.locator('[data-action=birdDismiss]').click();await page.wait_for_timeout(100)
  after=await page.locator('.board').bounding_box()
  assert all(abs(before[k]-after[k])<.6 for k in before),(before,after)
  assert await page.locator('.guide-hand').count()==0
  assert not errors,errors
  await browser.close()
 print('PASS: five playfield sizes, rotated-card bounds, save/code access, persistent hint icon, suit updates and stable tutorial dismissal.')
asyncio.run(main())
