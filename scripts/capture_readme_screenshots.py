"""Refresh every documentation screenshot from an isolated, running game profile."""
import asyncio,os
from pathlib import Path
from playwright.async_api import async_playwright
OUT=Path('docs/screenshots');OUT.mkdir(exist_ok=True)
# Create an isolated sample profile for reproducible captures. Documentation screenshots
# should exercise the real interface without modifying the developer's saved adventure.
async def main():
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True)
  page=await browser.new_page(viewport={'width':1440,'height':900},device_scale_factor=1,reduced_motion='reduce');errors=[]
  page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'))
  await page.evaluate("""async()=>{const {Session}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const {helpers}=await import('./core.mjs');const s=new Session();s.p.Unlocked=12;s.p.Coins=1840;s.p.Hearts=12;s.p.GuideSeen=[...features.map(f=>f.id),'cards',...Array.from({length:17},(_,i)=>'mechanic'+i),...helpers.map(h=>'helper-'+h.Id)];s.p.Stars=Object.fromEntries(Array.from({length:11},(_,i)=>[i+1,1+i%3]));s.events.a.Album.Cards=Object.fromEntries(Array.from({length:42},(_,i)=>[i,1]));s.events.a.Album.Packs=[2,3,1];s.save();}""")
  await page.reload()
  async def route(action):
   await page.evaluate("""a=>{document.querySelector('#dialog').close();const b=document.createElement('button');b.dataset.action=a;document.body.append(b);b.click();b.remove()}""",action)
   await page.wait_for_timeout(250)
  async def shot(*names):
   await page.evaluate('document.fonts.ready')
   await page.wait_for_timeout(500)
   for name in names:await page.screenshot(path=str(Path('docs')/name))
  async def hand(level,effect=None):
   await page.evaluate("""async ({level,effect})=>{const {Session}=await import('./session.mjs');const s=new Session();s.p.Unlocked=Math.max(s.p.Unlocked,level);s.p.Hearts=20;s.start(level,false,12345);if(effect)s.hand.use(effect);s.save()}""",{'level':level,'effect':effect})
   await page.reload();await route('continue');await page.locator('.board').wait_for()
  await route('map');await shot('screenshots/map.png','adventure-map.png')
  await route('home');await shot('screenshots/home.png','home.png')
  await route('gems');await shot('screenshots/garden.png','gems.png')
  await hand(1751);await shot('screenshots/cards.png','solitaire.png')
  await route('shop');await shot('screenshots/shop.png')
  await route('settings');await shot('screenshots/settings.png')
  await route('journal');await shot('journal.png')
  await route('ev:album');await shot('screenshots/album.png')
  await route('lives');await shot('screenshots/hearts.png')
  await route('wheel');await shot('screenshots/wheel.png')
  await route('ev:chest');await shot('screenshots/chest.png','progression.png')
  await route('ev:travel');await shot('screenshots/travel.png')
  await route('preview:1751');await shot('screenshots/preview.png')
  await hand(9600);await shot('deep-board.png','special-cards.png')
  await hand(9995);await shot('screenshots/long-garden.png')
  await hand(22);await shot('formations.png')
  await hand(1751,'peek');await shot('stock-preview.png')
  await hand(1751,'lucky');await shot('temporary-effects.png')
  last=await page.evaluate("""async()=>{const {Session}=await import('./session.mjs');const s=new Session();s.p.Hearts=20;s.start(1751,false,12345);const path=s.hand.Deal.Solution;for(const step of path.slice(0,-1)){if(step.Index<0){s.draw();s.tick(2);}else s.play(step.Index);}s.save();return path.at(-1).Index;}""")
  await page.reload();await route('continue');await page.locator(f'[data-action="card:{last}"]').click();await page.locator('.victory').wait_for();await shot('screenshots/victory.png','celebration.png')
  assert not errors,errors
  await browser.close()
 print('PASS: all README and existing documentation screenshots refreshed from the 10,000-level running game.')
asyncio.run(main())
