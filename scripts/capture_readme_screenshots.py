"""Capture original running-game screenshots; no reference images are used."""
import asyncio
from pathlib import Path
from playwright.async_api import async_playwright
OUT=Path('docs/screenshots');OUT.mkdir(exist_ok=True)
async def main():
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True)
  page=await browser.new_page(viewport={'width':1280,'height':800},device_scale_factor=1,reduced_motion='reduce')
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto('http://127.0.0.1:8765/')
  await page.evaluate("""async()=>{const {Session}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const {helpers}=await import('./core.mjs');const s=new Session();s.p.Unlocked=12;s.p.Coins=1840;s.p.Hearts=12;s.p.GuideSeen=[...features.map(f=>f.id),'cards',...Array.from({length:17},(_,i)=>'mechanic'+i),...helpers.map(h=>'helper-'+h.Id)];s.p.Stars=Object.fromEntries(Array.from({length:11},(_,i)=>[i+1,1+i%3]));s.events.a.Album.Cards=Object.fromEntries(Array.from({length:42},(_,i)=>[i,1]));s.events.a.Album.Packs=[2,3,1];s.save();}""")
  await page.reload();await page.wait_for_function("document.querySelector('.map-scenery').style.backgroundImage.includes('data:image')")
  await page.wait_for_timeout(400);await page.screenshot(path=str(OUT/'map.png'))
  await page.evaluate("async()=>{const {Session}=await import('./session.mjs');const s=new Session();s.p.Unlocked=351;s.start(351);s.save();}")
  await page.reload();await page.locator('[data-action=previewCurrent]').click();await page.locator('[data-action="level:351"]').click()
  await page.locator('.board .card').first.wait_for();await page.wait_for_timeout(500);await page.screenshot(path=str(OUT/'cards.png'))
  await page.locator('.play-actions [data-action=events]').click();await page.locator('.event-tile[data-action="ev:album"]').click()
  await page.wait_for_timeout(400);await page.screenshot(path=str(OUT/'album.png'))
  await page.locator('.shell-close').click();await page.locator('.heart-counter').click();await page.screenshot(path=str(OUT/'hearts.png'))
  async def route(action):
   await page.evaluate("""a=>{document.querySelector('#dialog').close();const b=document.createElement('button');b.dataset.action=a;document.body.append(b);b.click();b.remove()}""",action)
   await page.wait_for_timeout(180)
  async def shot(name):
   await page.evaluate('document.fonts.ready');await page.wait_for_timeout(180);await page.screenshot(path=str(Path('docs')/name))
  async def hand(level,effect=None):
   await page.evaluate("""async ({level,effect})=>{const {Session}=await import('./session.mjs');const s=new Session();s.p.Unlocked=Math.max(s.p.Unlocked,level);s.p.Hearts=20;s.start(level,false,12345);if(effect)s.hand.use(effect);s.save()}""",{'level':level,'effect':effect})
   await page.reload();await route('continue');await page.locator('.board').wait_for();await page.wait_for_timeout(300)
  await route('map');await page.wait_for_timeout(300);await shot('adventure-map.png')
  await route('home');await shot('home.png')
  await route('wheel');await shot('screenshots/wheel.png')
  await route('gems');await shot('gems.png')
  await route('journal');await shot('journal.png')
  await route('ev:chest');await shot('progression.png');await shot('screenshots/chest.png')
  await hand(351);await shot('solitaire.png')
  await hand(50000);await shot('deep-board.png');await shot('special-cards.png')
  await hand(50095);await shot('screenshots/long-garden.png')
  await hand(22);await shot('formations.png')
  await hand(351,'peek');await shot('stock-preview.png')
  await hand(351,'lucky');await shot('temporary-effects.png')
  last=await page.evaluate("""async()=>{const {Session}=await import('./session.mjs');const s=new Session();s.p.Hearts=20;s.start(351,false,12345);const path=s.hand.Deal.Solution;for(const step of path.slice(0,-1)){if(step.Index<0){s.draw();s.tick(2);}else s.play(step.Index);}s.save();return path.at(-1).Index;}""")
  await page.reload();await route('continue');await page.locator(f'[data-action="card:{last}"]').click();await page.locator('.victory').wait_for();await shot('celebration.png')
  await browser.close()
 print('Replaced every README/docs screenshot with current running-game views.')
asyncio.run(main())
