"""Verify framed activity surfaces, artwork and navigation on desktop and phone."""
import asyncio
from pathlib import Path
from playwright.async_api import async_playwright

async def main():
 out=Path('artifacts/framed-navigation');out.mkdir(parents=True,exist_ok=True)
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True)
  for width,height in [(1075,1757),(1280,800),(390,844),(320,640)]:
   page=await browser.new_page(viewport={'width':width,'height':height},reduced_motion='reduce')
   errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
   await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
   await page.goto('http://127.0.0.1:8765/')
   await page.locator('.story-node.current').wait_for()
   # New players have locked features and open the bottom of the chapter.
   # Test that state before unlocking everything for navigation checks.
   assert await page.locator('.feature-lock .game-icon').evaluate_all('es=>es.length>0&&es.every(e=>{const r=e.getBoundingClientRect();return r.width<=18&&r.height<=18})')
   assert await page.locator('.map-viewport').evaluate("e=>['::before','::after'].every(p=>getComputedStyle(e,p).position==='fixed')"),'Map borders scroll away'
   assert await page.locator('.map-camera').evaluate("e=>getComputedStyle(e).paddingBottom==='0px'"),'Blank space below scenery'
   assert await page.locator('.map-scenery').evaluate('e=>e.getBoundingClientRect().bottom>=document.body.getBoundingClientRect().bottom-2')
   await page.screenshot(path=str(out/f'fresh-{width}x{height}.png'))
   await page.evaluate("""async()=>{const {Session}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const s=new Session();s.p.Unlocked=12000;s.p.GuideSeen=features.map(f=>f.id);s.save()}""")
   await page.reload()
   await page.locator('header [data-action=home]').click()
   await page.locator('#app [data-action=map]').click()
   assert await page.locator('.map-dock').evaluate("e=>getComputedStyle(e,'::before').backgroundImage.includes('menu-scroll-cap.svg')")
   assert await page.locator('.map-dock .map-icon').evaluate_all("es=>es.every(e=>{const a=e.getBoundingClientRect(),b=e.closest('.map-dock').getBoundingClientRect();return a.left>=b.left&&a.right<=b.right})")
   await page.screenshot(path=str(out/f'map-{width}.png'))
   await page.locator('.map-dock [data-action=events]').click()
   await page.locator('#modal [data-action="ev:trail"]').click()
   assert await page.locator('.menu-parent').count()==1
   assert await page.locator('[data-action=menuBack]').count()==0,'Parent and Back duplicate same destination'
   for view in ['trail','memory']:
    if view=='memory':await page.locator('.activity-tabs [data-action="ev:memory"]').click()
    await page.locator('.activity-scene').evaluate('e=>e.decode()')
    assert await page.locator('.activity-scene').evaluate('e=>e.naturalWidth>1000')
    assert await page.locator('.current-tab').count()==1
    assert await page.locator('.current-tab').get_attribute('aria-current')=='page'
    assert await page.locator('#modal').evaluate('e=>e.scrollWidth<=e.clientWidth+2')
    actions=await page.locator('.activity-tabs [data-action],.related-places [data-action]').evaluate_all('es=>es.map(e=>e.dataset.action)')
    assert len(actions)==len(set(actions)),'Repeated sibling links'
    await page.screenshot(path=str(out/f'{view}-{width}.png'))
   await page.locator('.menu-parent').click()
   assert await page.locator('.event-overview').is_visible()
   assert await page.locator('[data-action=menuBack]').count()==0,'Returning to directory retained a loop'
   await page.locator('.shell-close').click()
   await page.locator('.map-dock [data-action=gems]').click()
   await page.wait_for_function("document.querySelector('.landscape').style.backgroundImage.includes('gem-garden-scene.png')")
   assert await page.locator('.gems .gem').count()==64
   assert await page.locator('.gems .gem').evaluate_all("es=>es.every(e=>{const a=e.getBoundingClientRect(),b=e.closest('.gems').getBoundingClientRect();return a.left>=b.left&&a.right<=b.right&&a.top>=b.top&&a.bottom<=b.bottom})"),'Garden clips playable gems'
   assert not errors,errors
   await page.screenshot(path=str(out/f'gems-{width}.png'))
   await page.close()
  await browser.close()
 print('PASS: desktop and phone map frames, dedicated artwork, unique links, parent navigation and Gem Garden.')

asyncio.run(main())
