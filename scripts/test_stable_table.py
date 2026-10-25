import asyncio
from playwright.async_api import async_playwright
async def measure(page):
 return await page.evaluate("""()=>{const box=e=>{const r=e.getBoundingClientRect();return {x:r.x+scrollX,y:r.y+scrollY,w:r.width,h:r.height}};return {scroll:scrollY,header:box(document.querySelector('#app>.subhead')),zoom:document.querySelector('#app').style.zoom,board:box(document.querySelector('.board')),cards:Object.fromEntries([...document.querySelectorAll('.board .card')].map(e=>[e.dataset.action,box(e)]))}}""")
def stable(before,after,label):
 assert before['zoom']==after['zoom'],(label,'zoom',before['zoom'],after['zoom'])
 for k in ['x','y','w','h']:assert abs(before['board'][k]-after['board'][k])<.6,(label,'board',k,before['board'],after['board'])
 for key,rect in after['cards'].items():
  if key in before['cards']:
   for k in ['x','y','w','h']:assert abs(rect[k]-before['cards'][key][k])<.6,(label,key,k)
async def main():
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True);page=await b.new_page(reduced_motion='reduce');errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  for width,height,level in [(1280,800,1),(1280,800,351),(800,600,351),(390,844,351),(800,600,50095)]:
   await page.set_viewport_size({'width':width,'height':height});await page.goto('http://127.0.0.1:8765/')
   path=await page.evaluate("""async level=>{const {Session}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const {helpers}=await import('./core.mjs');const s=new Session();s.p.Unlocked=level;s.p.Hearts=20;s.p.GuideSeen=[...features.map(f=>f.id),'cards',...Array.from({length:17},(_,i)=>'mechanic'+i),...helpers.map(h=>'helper-'+h.Id)];s.start(level,false,12345);s.save();return s.hand.Deal.Solution;}""",level)
   await page.reload();await page.locator('[data-action=previewCurrent]').click();await page.locator(f'[data-action="level:{level}"]').click();await page.wait_for_timeout(300)
   baseline=await measure(page)
   for i,step in enumerate(path[:min(18,len(path)-2)]):
    if step['Index']<0:
     draw=page.locator('[data-action=draw]');await draw.wait_for();await page.wait_for_function("!document.querySelector('[data-action=draw]').disabled");await draw.click()
    else:await page.locator(f'[data-action="card:{step["Index"]}"]').click()
    await page.mouse.move(5,5)
    await page.wait_for_timeout(60);stable(baseline,await measure(page),(width,level,i))
   # Timer updates and undo cannot change tableau geometry.
   await page.wait_for_timeout(1100);stable(baseline,await measure(page),'timers')
   undo=page.locator('[data-action=undo]')
   if await undo.is_enabled():await undo.click();await page.wait_for_timeout(80);stable(baseline,await measure(page),'undo')
   await page.locator('.play-actions [data-action=shop]').click();await page.locator('.menu-resume').click();await page.wait_for_timeout(150);stable(baseline,await measure(page),'resume')
   await page.set_viewport_size({'width':width+40,'height':height+60});await page.wait_for_timeout(200)
   resized=await measure(page);await page.wait_for_timeout(1100);stable(resized,await measure(page),'after resize')
  assert not errors,errors;await b.close()
 print('PASS: fixed board/card geometry through up to 18 moves per hand, stock draws, timers, undo, menu resume and viewport resize at desktop and phone sizes.')
asyncio.run(main())
