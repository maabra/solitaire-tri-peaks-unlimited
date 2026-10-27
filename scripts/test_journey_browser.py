"""Portrait worlds, Pip onboarding, natural unlocks, skins, responsive tables."""
import asyncio,json
from pathlib import Path
from playwright.async_api import async_playwright
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'artifacts'/'journey-screenshots';OUT.mkdir(exist_ok=True)
# Scenario: Portrait worlds, Pip onboarding, natural unlocks, skins, responsive tables. Fail on violated assertions rather than accepting screenshots alone.
async def main():
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True);errors=[]
  # Create the isolated browser context and seed its scenario-specific saved profile.
  async def context(profile=None,size=(1280,800)):
   c=await browser.new_context(viewport={'width':size[0],'height':size[1]},reduced_motion='reduce');await c.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
   if profile:await c.add_init_script('if(!localStorage.getItem("meadow-peaks-profile"))localStorage.setItem("meadow-peaks-profile",'+json.dumps(json.dumps(profile))+');')
   page=await c.new_page();page.on('pageerror',lambda e:errors.append(str(e)));await page.goto('http://127.0.0.1:8765/');await page.wait_for_timeout(200);return c,page
  c,page=await context();assert 'Pip' in await page.locator('.bird-guide').inner_text()
  await page.locator('[data-action=birdGo]').click();assert await page.locator('.bird-guide.in-panel').count()==1
  await page.locator('[data-action="level:1"]').click();await page.wait_for_selector('.painted-table');assert 'One higher' in await page.locator('.bird-guide').inner_text()
  await page.locator('[data-action=birdDismiss]').click();await page.screenshot(path=str(OUT/'table-first.png'))
  profile=await page.evaluate("async()=>{let {defaults}=await import('./session.mjs');let {features}=await import('./journey-guide.mjs');let p=defaults();p.Unlocked=5101;for(let i=1;i<=10000;i++)p.Stars[i]=3;p.GuideSeen=[...features.map(f=>f.id),'cards',...Array.from({length:16},(_,i)=>'mechanic'+(i+1))];return p}")
  await c.close();c,page=await context(profile)
  # Fifty-one genuinely different paintings are sampled directly from original columns.
  result=await page.evaluate("""async()=>{const {chapterPicture}=await import('./chapter-worlds.mjs');const {chapterTitles}=await import('./art-catalog.mjs');let hashes=new Set();for(let i=1;i<=100;i++){let url=await chapterPicture(i);hashes.add(url);}return {titles:new Set(chapterTitles).size,images:hashes.size}}""")
  assert result=={'titles':100,'images':100},result
  cosmetics=await page.evaluate("""async()=>{const {backPicture}=await import('./customization.mjs');const {tableSurface}=await import('./chapter-worlds.mjs');const backs=new Set(),tables=new Set();for(let i=1;i<=100;i++)backs.add(await backPicture(i));for(let i=1;i<=100;i++)tables.add(await tableSurface(i,300,180));return [backs.size,tables.size]}""")
  assert cosmetics==[100,100],cosmetics
  await page.locator('[data-action="ev:customize"]').first.click();await page.wait_for_function("[...document.querySelectorAll('.skin-card img')].every(i=>i.complete&&i.naturalWidth>0)")
  await page.screenshot(path=str(OUT/'card-studio.png'))
  await page.locator('[data-action="ev:back-1"]').click();assert await page.locator('body').evaluate('e=>e.classList.contains("custom-backs")')
  await page.locator('[data-action="ev:table-2"]').click();await page.locator('[data-action="ev:eventPlay"]').click();await page.wait_for_selector('.painted-table')
  assert await page.locator('.card.back').first.evaluate("e=>getComputedStyle(e).backgroundImage.startsWith('url(\"data:')")
  for width,height in [(2555,1274),(1058,1263),(1280,800),(800,600),(390,844),(320,640)]:
   await page.set_viewport_size({'width':width,'height':height});await page.wait_for_timeout(300)
   if not await page.evaluate('document.documentElement.scrollWidth<=innerWidth'):print('OVERFLOW',width,await page.evaluate("[...document.querySelectorAll('body *')].filter(e=>e.getBoundingClientRect().right>innerWidth+2).slice(0,12).map(e=>[e.className,e.getBoundingClientRect().right,e.getBoundingClientRect().width])"))
   assert await page.evaluate('document.documentElement.scrollWidth<=innerWidth'),f'overflow {width}'
   assert await page.locator('.board-shell').evaluate("e=>e.classList.contains('painted-table')&&getComputedStyle(e).backgroundSize==='100% 100%'")
   await page.screenshot(path=str(OUT/f'table-{width}.png'),full_page=width<700)
  # Reload preserves selected cosmetics and their ownership.
  await page.reload();saved=await page.evaluate('JSON.parse(localStorage.getItem("meadow-peaks-profile"))')
  assert saved['Customization']['Back']==1 and saved['Customization']['Table']==2
  await page.locator('[data-action=events]').click();await page.locator('#modal [data-action="ev:travel"]').click();await page.wait_for_function("[...document.querySelectorAll('#modal img[data-chapter]')].every(i=>i.complete&&i.naturalWidth>0&&i.src.startsWith('data:'))")
  await page.screenshot(path=str(OUT/'travel-portraits.png'))
  await c.close()
  # A new feature is introduced at its threshold, with an actionable tour.
  profile['Unlocked']=15;profile['Stars']={};profile['GuideSeen']=[x for x in profile['GuideSeen'] if x!='album']
  c,page=await context(profile);assert 'parcel of memories' in (await page.locator('.bird-guide').inner_text()).lower();await page.locator('[data-action=birdGo]').click();assert await page.locator('.event-album').count()==1
  await c.close();assert not errors,errors;await browser.close()
 print('PASS: 100 matched chapter scenes, card backs and tables, Pip welcome/navigation/unlock tours, earned skins, persistence and fitted isolated tables at six viewport sizes.')
asyncio.run(main())
