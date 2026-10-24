"""Shared UI regression checks: scale, action races, persistence and temporary art."""
import asyncio,json
from pathlib import Path
from playwright.async_api import async_playwright
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'artifacts'/'web-screenshots';OUT.mkdir(exist_ok=True)
URL='http://127.0.0.1:8765/'
async def main():
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True)
  errors=[]
  async def context(width=1280,height=800,profile=None):
   c=await browser.new_context(viewport={'width':width,'height':height},reduced_motion='reduce')
   if profile:await c.add_init_script('localStorage.setItem("meadow-peaks-profile",'+json.dumps(json.dumps(profile))+');')
   await c.add_init_script("localStorage.setItem('meadow-storage-consent','yes')");page=await c.new_page();page.on('pageerror',lambda e:errors.append(str(e)));await page.goto(URL);await page.wait_for_timeout(150);return c,page
  c,bootstrap=await context()
  # Confirm each of the ten actual paintings decodes; these are not tint-only variants.
  assert await bootstrap.evaluate("""async()=>{let {environments}=await import('./chapter-scene.mjs');return environments.length===10&&(await Promise.all(environments.map(name=>new Promise(resolve=>{let i=new Image;i.onload=()=>resolve(i.naturalWidth>1000);i.onerror=()=>resolve(false);i.src='assets/'+name+'.png'})))).every(Boolean)}""")
  profile=await bootstrap.evaluate("""async()=>{let {defaults}=await import('./session.mjs');let p=defaults();p.Unlocked=480000;return p}""")
  for width,height in [(2555,1274),(1058,1263),(1280,800),(800,600),(390,844),(320,640)]:
   ctx,page=await context(width,height,profile)
   assert await page.evaluate('document.documentElement.scrollWidth<=innerWidth')
   assert await page.evaluate("""async()=>{let {roadFor}=await import('./adventure-map.mjs');let route=roadFor(+document.querySelector('.adventure-map').dataset.chapter).stops;let svg=document.querySelector('.map-trail'),m=svg.getScreenCTM();return [...document.querySelectorAll('.story-node')].every((node,i)=>{let p=svg.createSVGPoint();[p.x,p.y]=route[i];p=p.matrixTransform(m);let r=node.parentElement.getBoundingClientRect();return Math.abs(p.x-r.x-r.width/2)<1&&Math.abs(p.y-r.y-r.height/2)<1&&node.scrollWidth<=node.clientWidth})}"""),f'Route mismatch or clipped label: {width}x{height}'
   await page.screenshot(path=str(OUT/f'edge-map-{width}.png'))
   await page.locator('[data-action="wheel"]').click();await page.locator('[data-action="spinWheel"]').click();await page.wait_for_timeout(80);assert await page.locator('[data-action="spinWheel"]').is_disabled();await page.locator('[data-action="close"]').last.click()
   await ctx.close()
  await c.close()
  c,page=await context()
  await page.locator('[data-action="preview:1"]').click();await page.locator('[data-action="level:1"]').click();await page.wait_for_timeout(150)
  await page.evaluate("globalThis.cardsBefore=new Map([...document.querySelectorAll('.board .card')].map(e=>[e.dataset.action,e]));globalThis.stockBefore=document.querySelector('[data-action=draw]');")
  await page.set_viewport_size({'width':850,'height':800});await page.wait_for_timeout(150)
  assert await page.evaluate("[...document.querySelectorAll('.board .card')].every(e=>cardsBefore.get(e.dataset.action)===e)&&stockBefore===document.querySelector('[data-action=draw]')")
  assert await page.evaluate("document.querySelector('.board').getBoundingClientRect().width<800")
  await page.locator('[data-action="favorite"]').click();assert 'Saved' in await page.locator('[data-action="favorite"]').inner_text()
  before=await page.evaluate('JSON.parse(localStorage.getItem("meadow-peaks-profile")).Active.Stock.length')
  await page.evaluate("document.querySelector('[data-action=draw]').click();document.querySelector('[data-action=draw]').click()")
  assert await page.evaluate('JSON.parse(localStorage.getItem("meadow-peaks-profile")).Active.Stock.length')==before-1
  await page.locator('[data-action="home"]').first.click();await page.locator('[data-action="map"]').click();await page.locator('[data-action="preview:1"]').click();await page.locator('[data-action="level:1"]').click()
  assert await page.evaluate('JSON.parse(localStorage.getItem("meadow-peaks-profile")).Active.Stock.length')==before-1,'Reopening a level reset the hand'
  await page.locator('[data-action="home"]').first.click();await page.locator('[data-action="endless"]').click();assert await page.locator('[data-action="confirmStart"]').count()==1;await page.locator('[data-action="close"]').last.click()
  # A second tab cannot silently overwrite the current adventure.
  other=await c.new_page();await other.goto(URL);await other.locator('[data-action="home"]').first.click();await other.locator('[data-action="daily"]').click()
  await page.wait_for_selector('[data-action="reloadSave"]');await page.locator('[data-action="reloadSave"]').click();assert await page.locator('.story-node').count()==100
  await c.close()
  # Timed helper expiry must restore the actual card art and waste rank immediately.
  c,base=await context()
  for level,helper,waiter in [(1,'wild',"document.querySelector('.hand-row .pile:nth-child(3) .face-rank').textContent!=='\u2605'"),(100,'thaw',"document.querySelectorAll('.ice-art').length>0")]:
   profile=await base.evaluate("""async ([level,helper])=>{let {defaults}=await import('./session.mjs');let {generate,Hand}=await import('./core.mjs');let p=defaults();p.Unlocked=level;let h=new Hand(generate(104729+level*997,level));h.use(helper);h.tick(helper==='wild'?11.5:7.5);p.Active=h.capture(104729+level*997,level,false);return p}""",[level,helper])
   ctx,target=await context(profile=profile);await target.locator('[data-action="home"]').first.click();await target.locator('[data-action="continue"]').click();await target.wait_for_function(waiter,timeout=2500);await target.screenshot(path=str(OUT/f'expired-{helper}.png'),full_page=True);await ctx.close()
  await c.close();await browser.close();assert errors==[],errors
  print('PASS: 10 painted environments; six viewport sizes; aligned route and six-digit labels; stable resize; favorite feedback; rapid draws; hand replacement; tab conflicts; timed Wild and Thaw visuals.')
asyncio.run(main())
