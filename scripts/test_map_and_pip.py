"""Actual map clicks, bounded chapter text, soft scenery, and early/locked Pip paths."""
import asyncio
from pathlib import Path
from playwright.async_api import async_playwright
OUT=Path('artifacts/map-pip-screenshots');OUT.mkdir(exist_ok=True)
# Scenario: Actual map clicks, bounded chapter text, soft scenery, and early/locked Pip paths. Fail on violated assertions rather than accepting screenshots alone.
async def main():
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True);page=await b.new_page(viewport={'width':1274,'height':1267},reduced_motion='reduce');errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')");await page.goto('http://127.0.0.1:8765/')
  await page.evaluate("async()=>{const {defaults}=await import('./session.mjs');const p=defaults();p.Unlocked=2;p.Stars[1]=1;p.Coins=420;localStorage.setItem('meadow-peaks-profile',JSON.stringify(p));}");await page.reload()
  await page.wait_for_function("document.querySelector('.map-ambient').style.backgroundImage.includes('data:image')")
  # Even if a neighboring stop reaches the marker, its face must paint underneath.
  assert await page.evaluate("""()=>{
   const flag=document.querySelector('.here-flag'),current=flag.parentElement;
   const neighbor=[...document.querySelectorAll('.map-stop')].find(e=>e!==current);
   const saved=neighbor.style.cssText;
   neighbor.style.left=(current.offsetLeft+90)+'px';neighbor.style.top=current.offsetTop+'px';
   flag.style.pointerEvents='auto';const r=flag.getBoundingClientRect();
   const top=document.elementFromPoint(r.x+r.width/2,r.y+r.height/2);
   const visible=flag===top||flag.contains(top);
   neighbor.style.cssText=saved;flag.style.pointerEvents='';return visible;
  }"""),'neighboring level covers Play marker'
  for width,height in [(1274,1267),(1280,800),(800,600),(390,844),(320,640)]:
   await page.set_viewport_size({'width':width,'height':height});await page.wait_for_timeout(100)
   assert await page.locator('.chapter-ribbon').evaluate("e=>{const r=e.getBoundingClientRect();return r.left>=0&&r.right<=innerWidth&&e.scrollWidth<=e.clientWidth+2&&e.scrollHeight<=e.clientHeight+2}"),(width,'ribbon')
   clipped=await page.locator('.chapter-ribbon').evaluate("root=>[...root.querySelectorAll('h1,small,p')].filter(e=>e.scrollWidth>e.clientWidth+2||e.scrollHeight>e.clientHeight+2).map(e=>e.innerText)")
   assert not clipped,(width,clipped)
   assert await page.locator('.map-scenery').evaluate('e=>getComputedStyle(e).filter')=='none'
   assert await page.locator('.map-trail').evaluate('e=>getComputedStyle(e).filter')=='none'
   assert await page.locator('.story-node.current').evaluate('e=>{const r=e.getBoundingClientRect();return document.elementFromPoint(r.x+r.width/2,r.y+r.height/2)?.closest(".story-node")===e}'),(width,'road or decoration covers current stop')
   assert await page.locator('.map-controls').evaluate('e=>{const r=e.getBoundingClientRect();return r.left>=0&&r.right<=innerWidth+1}'),width
   assert await page.locator('.story-node.current').evaluate("e=>{const r=e.getBoundingClientRect(),g=document.querySelector('.bird-guide').getBoundingClientRect(),h=document.querySelector('.chapter-ribbon').getBoundingClientRect();return r.top>h.bottom&&r.bottom<g.top}"),(width,'current level covered by map UI')
   await page.screenshot(path=str(OUT/f'map-{width}.png'))
  await page.locator('[data-action=birdGo]').click();assert await page.locator('[data-action="level:2"]').is_visible();assert 'Press Play' in await page.locator('.bird-guide').inner_text()
  await page.locator('.shell-close').click()
  if await page.locator('[data-action=birdDismiss]').count():await page.locator('[data-action=birdDismiss]').click()
  # The visible bird itself must be a real pointer target, not a zero-size parent.
  await page.locator('.map-companion [data-action=birdHelp]').click()
  assert await page.locator('[data-action="birdTopic:welcome"]').is_visible();assert await page.locator('[data-action="birdTopic:cards"]').is_visible()
  await page.locator('[data-action="birdTopic:cards"]').click();assert await page.locator('.guide-grid').is_visible()
  await page.locator('.shell-close').click()
  if await page.locator('[data-action=birdDismiss]').count():await page.locator('[data-action=birdDismiss]').click()
  await page.locator('.map-companion [data-action=birdHelp]').click();await page.locator('[data-action="birdTopic:welcome"]').click();assert await page.locator('[data-action="level:2"]').is_visible()
  await page.locator('.shell-close').click()
  if await page.locator('[data-action=birdDismiss]').count():await page.locator('[data-action=birdDismiss]').click()
  await page.locator('[data-action="previewCurrent"]').click();assert await page.locator('[data-action="level:2"]').is_visible();await page.locator('.shell-close').click()
  # Locked activities are browsable from the overlay, never road destinations.
  assert await page.locator('.road-destination').count()==0
  assert await page.locator('.map-dock [data-action="lockedFeature:missions"]').count()==1
  assert not errors,errors;await b.close()
 print('PASS: bounded map header at five sizes, sharp scenery, actual Pip clicks, functional early help, current-level Play and undiscovered activities hidden.')
asyncio.run(main())
