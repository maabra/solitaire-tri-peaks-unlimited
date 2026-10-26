"""Exercise the final illustrated chapter, bounded menus and endless continuation."""
import asyncio,os,json
from playwright.async_api import async_playwright
async def route(page,action):
 await page.evaluate("a=>{const d=document.querySelector('#dialog');d.close();const b=document.createElement('button');b.dataset.action=a;document.body.append(b);b.click();b.remove()}",action)
async def main():
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True);page=await b.new_page(viewport={'width':1440,'height':900},reduced_motion='reduce');errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'))
  art=await page.evaluate("""async()=>{const {chapterPicture}=await import('./chapter-worlds.mjs');const result=[];for(let c=1;c<=100;c++){const picture=await chapterPicture(c),hash=await crypto.subtle.digest('SHA-256',new TextEncoder().encode(picture));result.push([...new Uint8Array(hash)].join(','));}return new Set(result).size}""");assert art==100,art
  last=await page.evaluate("""async()=>{const {Session}=await import('./session.mjs'),{features}=await import('./journey-guide.mjs'),{helpers}=await import('./core.mjs');const s=new Session();s.p.Unlocked=10000;s.p.Hearts=30;s.p.GuideSeen=[...features.map(f=>f.id),'cards',...Array.from({length:17},(_,i)=>'mechanic'+i),...helpers.map(h=>'helper-'+h.Id)];s.p.ChapterGifts100=[1];s.p.Customization.Unlocked=[0,1];s.start(10000,false,12345);const path=s.hand.Deal.Solution;for(const step of path.slice(0,-1)){if(step.Index<0){s.tick(2);s.draw();}else s.play(step.Index);}s.save();return path.at(-1).Index;}""")
  await page.reload();await route(page,'continue');await page.locator(f'[data-action="card:{last}"]').click();await page.locator('.victory').wait_for();assert 'Play endless' in await page.locator('.victory [data-action=next]').inner_text()
  await page.locator('[data-action=next]').click();assert 'Endless hand' in await page.locator('.level-preview h2').inner_text();await page.locator('[data-action=beginPreview]').click();await page.locator('.board').wait_for();assert await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Active.Endless")
  await route(page,'map');assert await page.locator('.adventure-map').get_attribute('data-chapter')=='100'
  ids=await page.locator('.story-node').evaluate_all("es=>es.map(e=>Number(e.dataset.action.split(':')[1]))");assert min(ids)==9901 and max(ids)==10000
  await route(page,'ev:travel');await page.locator('[data-action="ev:travelCurrent"]').click();await page.locator('.travel-list').wait_for();assert await page.locator('[data-action="ev:travelNext"]').is_disabled();assert await page.locator('[data-action="ev:travel-100"]').count()==1;assert await page.locator('[data-action="ev:travel-101"]').count()==0
  await route(page,'ev:customize')
  for _ in range(8):await page.locator('[data-action="ev:skinsNext"]').click()
  await page.locator('[data-action="ev:skinsNext"]:disabled').wait_for();assert await page.locator('[data-back="100"]').count()==1;assert await page.locator('[data-back="101"]').count()==0
  await route(page,'guide')
  for kind,level in [(1,'150'),(3,'350'),(4,'600'),(14,'9,600')]:assert 'From level '+level in await page.locator(f'[data-guide-kind="{kind}"]').inner_text()
  await route(page,'settings')
  raw=await page.evaluate("async()=>{const {defaults}=await import('./session.mjs');return {...defaults(),Unlocked:480000,Coins:1739,Stars:{1:3,480000:2},Scores:{480000:90}}}")
  await page.locator('#import-file').set_input_files({'name':'old-meadow-save.json','mimeType':'application/json','buffer':json.dumps(raw).encode()})
  await page.locator('[data-action=confirmImport]').click()
  profile=await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile'))")
  assert profile['Unlocked']==10000 and profile['Coins']==1739 and profile['LegacyCampaign']['Stars']['480000']==2,profile
  assert not errors,errors
  await b.close()
 print('PASS: all 100 chapter paintings load uniquely; final win offers endless, map stops at 10,000, Travel and Card Studio stop at chapter 100.')
asyncio.run(main())
