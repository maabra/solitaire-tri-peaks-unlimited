"""New event UI, consent gate, scrolling chapter and narrow-screen regression checks."""
import asyncio,json
from pathlib import Path
from playwright.async_api import async_playwright
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'artifacts'/'adventure-screenshots';OUT.mkdir(exist_ok=True)
async def main():
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True)
  c=await browser.new_context(viewport={'width':1280,'height':800},reduced_motion='reduce')
  page=await c.new_page();errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
  await page.goto('http://127.0.0.1:8765/')
  assert await page.locator('#allow-storage').is_visible()
  assert await page.locator('.story-node').count()==0
  await page.keyboard.press('Escape');assert await page.locator('#dialog').evaluate('(d)=>d.open')
  assert not await page.locator('#dialog .close').is_visible()
  await page.locator('#allow-storage').click();await page.locator('.story-node').first.wait_for()
  assert await page.locator('.story-node').count()==100
  assert any(x['name']=='meadow_storage' for x in await c.cookies())
  assert await page.locator('.chapter-ribbon small').inner_text()=='CHAPTER 1 · 1–100'
  await page.wait_for_timeout(150)
  bottom=await page.locator('.map-viewport').evaluate('(e)=>e.scrollTop')
  assert bottom>4000
  await page.locator('.map-viewport').evaluate('(e)=>e.scrollTop=0')
  assert await page.locator('[data-action="preview:100"]').count()==1
  await page.locator('[data-action=locate]').click()
  await page.screenshot(path=str(OUT/'chapter-100-scroll.png'))
  await page.evaluate("async()=>{let {defaults}=await import('./session.mjs'),{features}=await import('./journey-guide.mjs');let p=defaults();p.HighestPlayedLevel=2500;p.ChapterGifts100=[1];p.GuideSeen=[...features.map(f=>f.id),'cards',...Array.from({length:16},(_,i)=>'mechanic'+(i+1))];localStorage.setItem('meadow-peaks-profile',JSON.stringify(p))}")
  await page.reload()
  await page.locator('[data-action=events]').click()
  panels=['album','missions','voyage','tournament','hunt','bingo','regatta','balloon','travel','milestones','chest','postcard','tickets']
  for size in [(1280,800),(390,844),(320,640)]:
   await page.set_viewport_size({'width':size[0],'height':size[1]})
   for name in panels:
    await page.locator('#modal [data-action="ev:'+name+'"]').click()
    if name=='travel':
     await page.wait_for_function("[...document.querySelectorAll('#modal img[data-chapter]')].every(i=>i.complete&&i.naturalWidth>0&&i.src.startsWith('data:'))")
     assert await page.locator('#modal img[data-chapter]').evaluate_all('(a)=>new Set(a.map(i=>i.src)).size===a.length')
    assert await page.locator('#modal').evaluate('(e)=>e.scrollWidth<=e.clientWidth+2'),f'{name} overflow at {size}'
    await page.screenshot(path=str(OUT/f'{name}-{size[0]}.png'))
    if name=='album':
     await page.locator('#modal [data-action="ev:album-0"]').click();assert await page.locator('.sticker-card').count()==10
     await page.locator('#modal [data-action="ev:categories"]').click()
     if size[0]==1280:
      await page.locator('#modal [data-action="ev:pack"]').click();assert await page.locator('.pack-reveal .sticker-card').count()==3
      assert await page.locator('.pack-reveal .fresh').count()==0
    await page.locator('#modal [data-action="ev:hub"]').click()
  await page.locator('[data-action=close]').last.click()
  await page.reload();assert await page.locator('#allow-storage').count()==0
  await page.locator('[data-action=events]').click();await page.locator('#modal [data-action="ev:album"]').click()
  assert 1<=await page.evaluate("Object.keys(JSON.parse(localStorage.getItem('meadow-peaks-profile')).Adventure.Album.Cards).length")<=3
  await page.locator('#modal [data-action="ev:hub"]').click();await page.locator('#modal [data-action="ev:travel"]').click()
  await page.locator('#modal [data-action="ev:travel-2"]').click();assert 'CHAPTER 2' in await page.locator('.chapter-ribbon small').inner_text()
  assert await page.locator('.story-node').count()==100
  assert await page.locator('[data-action="preview:101"]').is_disabled()
  assert not errors,errors
  await browser.close()
 print('PASS: explicit storage consent, 100 scrolling levels/chapter, all 13 event panels at three sizes, pack opening, category navigation, persistence and locked future chapters.')
asyncio.run(main())
