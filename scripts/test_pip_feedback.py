import asyncio,os
from pathlib import Path
from playwright.async_api import async_playwright
async def main():
 out=Path('artifacts/pip-feedback');out.mkdir(exist_ok=True)
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True);page=await b.new_page(viewport={'width':1440,'height':900},reduced_motion='reduce');await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')");await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'))
  await page.evaluate("async()=>{const {Session}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const {helpers}=await import('./core.mjs');const s=new Session();s.p.Unlocked=3;s.p.GuideSeen=[...features.map(f=>f.id),'cards',...helpers.map(h=>'helper-'+h.Id)];s.start(3,false,106723,'MP21');s.save()}");await page.reload()
  await page.evaluate("()=>{const e=document.createElement('button');e.dataset.action='continue';document.body.append(e);e.click();e.remove()}")
  bad=await page.evaluate("async()=>{const {Session}=await import('./session.mjs');const h=new Session().hand;return [...document.querySelectorAll('.board .card:not(:disabled)')].find(e=>!h.Moves.includes(Number(e.dataset.action.split(':')[1]))).dataset.action}")
  for i in range(2):await page.locator('[data-action="'+bad+'"]').click()
  assert await page.locator('.bird-guide').count()==0,'Pip interrupted before repeated mistakes'
  await page.locator('[data-action="'+bad+'"]').click();await page.locator('.bird-guide').wait_for()
  assert 'A little tip from Pip' in await page.locator('.bird-guide').inner_text()
  assert await page.locator('.bird-guide .pip-sprite').count()==1
  assert await page.locator('#toast.show').count()==0
  assert await page.locator('.bird-guide').evaluate("e=>{const a=e.getBoundingClientRect(),b=document.querySelector('.play-actions').getBoundingClientRect();return a.bottom<b.top}")
  assert await page.locator('[data-action=share],[data-action=openDeal],[data-action=favorite]').count()==0
  assert await page.locator('.star-icons b').count()==3
  assert await page.locator('.star-meter').evaluate("e=>{const a=e.querySelector('.star-icons').getBoundingClientRect(),b=e.querySelector('.star-track').getBoundingClientRect(),c=e.querySelector('small').getBoundingClientRect();return a.bottom<=b.top&&b.bottom<=c.top}")
  await page.screenshot(path=str(out/'pip-tip-and-progress.png'))
  good=await page.evaluate("async()=>{const {Session}=await import('./session.mjs');return new Session().hand.Moves[0]}");await page.locator(f'[data-action="card:{good}"]').click();assert await page.locator('.bird-guide').count()==0
  await page.locator('header [data-action=home]').click();await page.locator('header [data-action=settings]').click()
  assert await page.locator('[data-action=export],[data-action=import]').count()==2
  assert await page.locator('[data-action=openDeal],[data-action=share],#deal-code,#open-code').count()==0
  async with page.expect_download() as download:await page.locator('[data-action=export]').click()
  backup=await download.value;path=out/'backup.json';await backup.save_as(path)
  await page.locator('#import-file').set_input_files(str(path));await page.locator('[data-action=confirmImport]').wait_for();await page.locator('[data-action=confirmImport]').click()
  assert await page.locator('#dialog').evaluate('e=>!e.open')
  await page.reload();await page.evaluate("()=>{const e=document.createElement('button');e.dataset.action='gems';document.body.append(e);e.click();e.remove()}")
  pair=await page.evaluate("async()=>{const {Session}=await import('./session.mjs');const s=new Session();s.openGems();const g=s.gems;for(let a=0;a<64;a++)for(const b of [a+1,a+8])if(g.neighbors(a,b)){g.swap(a,b);const valid=g.matches().size;g.swap(a,b);if(!valid)return [a,b]}}")
  for _ in range(3):
   for cell in pair:await page.locator(f'[data-action="gem:{cell}"]').click()
  await page.locator('.bird-guide').wait_for();assert 'Show a match' in await page.locator('.bird-guide').inner_text()
  assert await page.locator('.bird-guide').evaluate("e=>{const a=e.getBoundingClientRect(),b=document.querySelector('.gems').getBoundingClientRect();return a.bottom<=b.top}")
  await b.close()
 print('PASS: repeated mistakes get Pip guidance without covering controls, valid moves dismiss it, progress is separated, deal UI is absent, save export/import works in main Settings.')
asyncio.run(main())
