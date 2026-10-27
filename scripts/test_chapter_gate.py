"""Check chapter gates in the actual cabinet, including a real level-100 win."""
import asyncio,os
from pathlib import Path
from playwright.async_api import async_playwright

# Dispatch the requested game action through the page while keeping the test profile isolated.
async def route(page,action):
 await page.evaluate("a=>{const e=document.createElement('button');e.dataset.action=a;document.body.append(e);e.click();e.remove()}",action)

# Scenario: Check chapter gates in the actual cabinet, including a real level-100 win. Fail on violated assertions rather than accepting screenshots alone.
async def main():
 out=Path('artifacts/chapter-gate');out.mkdir(exist_ok=True)
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True)
  page=await b.new_page(viewport={'width':1440,'height':900},reduced_motion='reduce');errors=[]
  page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'))
  await route(page,'map');await page.locator('.map-viewport').evaluate('e=>e.scrollTop=0')
  gate=page.locator('.chapter-gate');await gate.locator('img').evaluate('e=>e.decode()')
  assert await gate.get_attribute('class')=='chapter-gate is-locked'
  await gate.click();assert await page.locator('.adventure-map').get_attribute('data-chapter')=='1'
  await page.screenshot(path=str(out/'locked.png'))
  # Replay the deal's actual witness up to its final move, then finish through the UI.
  # This checks the saved win-to-gate transition rather than merely styling an open gate.
  last=await page.evaluate("""async()=>{const {Session}=await import('./session.mjs'),{features}=await import('./journey-guide.mjs'),{helpers}=await import('./core.mjs');const s=new Session();s.p.Unlocked=100;s.p.Stars=Object.fromEntries(Array.from({length:99},(_,i)=>[i+1,1]));s.p.Hearts=30;s.p.GuideSeen=[...features.map(f=>f.id),'cards',...Array.from({length:17},(_,i)=>'mechanic'+i),...helpers.map(h=>'helper-'+h.Id)];s.start(100,false,12345);const path=s.hand.Deal.Solution;for(const step of path.slice(0,-1)){if(step.Index<0){s.tick(2);s.draw();}else s.play(step.Index);}s.save();return path.at(-1).Index;}""")
  await page.reload();await route(page,'continue');await page.locator(f'[data-action="card:{last}"]').click()
  await page.locator('.victory').wait_for();await page.locator('[data-action=next]').click()
  await page.locator('.chapter-gate.is-open').wait_for();await page.wait_for_timeout(1200)
  assert await page.locator('.map-viewport').evaluate('e=>e.scrollTop')==0
  assert await page.evaluate("()=>{const g=document.querySelector('.chapter-gate').getBoundingClientRect(),r=document.querySelector('.chapter-ribbon').getBoundingClientRect(),n=document.querySelector('[data-action=\"preview:100\"]').getBoundingClientRect();return g.top>=r.bottom&&g.bottom<n.top}"),'Gate overlaps ribbon or level 100'
  await page.screenshot(path=str(out/'open.png'))
  before=await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Coins")
  await page.locator('.chapter-gate').click();assert await page.locator('.adventure-map').get_attribute('data-chapter')=='2'
  assert await page.locator('[data-action="preview:101"].current').count()==1
  assert await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Coins")==before
  await page.screenshot(path=str(out/'chapter-two.png'))
  await route(page,'chapterGate:2');assert await page.locator('.adventure-map').get_attribute('data-chapter')=='2','Forged locked gate bypassed progression'
  assert not errors,errors
  await b.close()
 print('PASS: locked gate blocks entry; real level-100 win reveals gate without overlaps; open gate enters chapter 2 at level 101 without charging coins.')
asyncio.run(main())
