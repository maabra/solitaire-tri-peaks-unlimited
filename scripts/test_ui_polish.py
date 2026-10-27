"""Shared menu flow, text containment, top-layer help, motion and saved-hand continuity."""
import asyncio,json
from pathlib import Path
from playwright.async_api import async_playwright
OUT=Path('artifacts/polish-screenshots');OUT.mkdir(exist_ok=True)
# Scenario: Shared menu flow, text containment, top-layer help, motion and saved-hand continuity. Fail on violated assertions rather than accepting screenshots alone.
async def main():
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True)
  context=await browser.new_context(viewport={'width':1280,'height':800})
  await context.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  page=await context.new_page();errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
  await page.goto('http://127.0.0.1:8765/')
  await page.evaluate("async()=>{const {defaults}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');let p=defaults();p.HighestPlayedLevel=2500;p.ChapterGifts100=[1];p.GuideSeen=[...features.map(f=>f.id),'cards',...Array.from({length:16},(_,i)=>'mechanic'+(i+1))];localStorage.setItem('meadow-peaks-profile',JSON.stringify(p));}")
  await page.reload()
  await page.locator('[data-action="preview:1"]').click();await page.locator('[data-action="level:1"]').click();await page.wait_for_selector('.painted-table')
  hand=await page.evaluate("(()=>{const a=JSON.parse(localStorage.getItem('meadow-peaks-profile')).Active;return [a.Level,a.Seed,a.Removed,a.Stock,a.Waste]})()")
  await page.locator('#app [data-action=shop]').click()
  for width,height in [(1280,800),(800,600),(390,844),(320,640)]:
   await page.set_viewport_size({'width':width,'height':height})
   for name,action in [('shop','shop'),('settings','settings'),('wheel','wheel'),('journal','journal'),('guide','guide'),('events','events')]:
    print('Checking',name,width,flush=True)
    # Header controls remain behind a modal; dispatch the same action through a temporary real button.
    await page.evaluate("action=>{const b=document.createElement('button');b.dataset.action=action;document.querySelector('#modal').append(b);b.click();b.remove();}",action)
    await page.wait_for_timeout(260)
    assert await page.locator('.menu-navigation').count()==1,name
    if name=='events':
     assert await page.locator('.event-panel').evaluate('e=>getComputedStyle(e).display')=='block'
     assert await page.locator('.event-tile').first.evaluate('e=>e.clientWidth')>(110 if width>700 else 85)
    assert await page.locator('.shell-close').is_visible(),name
    assert await page.locator('#modal .info-button').evaluate_all('buttons=>buttons.every(e=>Math.abs(e.getBoundingClientRect().width-e.getBoundingClientRect().height)<2)'),(name,width,'distorted info button')
    clipping=await page.locator('#modal').evaluate("""root=>[...root.querySelectorAll('button,h2,h3,strong')].filter(e=>e.getClientRects().length&&!e.closest('.activity-tabs')&&(e.scrollWidth>e.clientWidth+2||e.scrollHeight>e.clientHeight+3)).map(e=>[e.tagName,e.innerText,e.scrollWidth,e.clientWidth,e.scrollHeight,e.clientHeight])""")
    assert not clipping,(name,width,clipping)
    overflow=await page.locator('#modal').evaluate("root=>{let r=root.getBoundingClientRect();return [...root.querySelectorAll('*')].filter(e=>e.getClientRects().length&&e.getBoundingClientRect().right>r.right+2).slice(0,10).map(e=>[e.tagName,e.className,e.innerText])}")
    assert await page.locator('#modal').evaluate('e=>e.scrollWidth<=e.clientWidth+2'),(name,width,overflow)
    title=await page.locator('.panel-heading h2').inner_text()
    await page.screenshot(path=str(OUT/f'before-{name}-{width}.png'))
    # Explanations appear only where the activity needs one.
    if await page.locator('.panel-heading .info-button').count():
     await page.locator('.panel-heading .info-button').click()
     assert await page.locator('#context-help').evaluate('e=>e.open')
     assert await page.locator('#help-title').is_visible()
     await page.keyboard.press('Escape');assert not await page.locator('#context-help').evaluate('e=>e.open')
     assert await page.locator('.panel-heading h2').inner_text()==title
    await page.screenshot(path=str(OUT/f'{name}-{width}.png'))
  # Navigation returns to the same hand, without changing or consuming cards.
  await page.locator('.menu-navigation [data-action=continue]').click();assert await page.locator('.board').count()==1
  after=await page.evaluate("(()=>{const a=JSON.parse(localStorage.getItem('meadow-peaks-profile')).Active;return [a.Level,a.Seed,a.Removed,a.Stock,a.Waste]})()")
  assert hand==after,'Menu navigation changed the saved hand'
  await page.locator('.helper-info').first.click();assert await page.locator('#context-help').evaluate('e=>e.open');await page.locator('.help-done').click()
  await page.wait_for_timeout(1100);await page.screenshot(path=str(OUT/'gameplay-320.png'),full_page=True)
  await page.set_viewport_size({'width':1280,'height':800});await page.locator('#app [data-action=events]').click()
  assert await page.locator('.menu-arrival').first.evaluate('e=>getComputedStyle(e).animationName')=='menu-item'
  await page.emulate_media(reduced_motion='reduce');assert await page.locator('.menu-arrival').first.evaluate('e=>getComputedStyle(e).animationName')=='none'
  await page.locator('#modal [data-action="ev:regatta"]').click()
  assert await page.locator('.field-notes').count()==1
  await page.locator('.field-notes summary').click()
  assert 'CPU' in await page.locator('.field-notes').inner_text()
  assert await page.locator('.field-notes').evaluate('e=>e.open')
  await page.locator('.panel-heading .info-button').click();assert 'Consecutive' in await page.locator('#context-help').inner_text()
  await page.keyboard.press('Escape')
  assert not errors,errors;await browser.close()
 print('PASS: six shared menus at four sizes, text containment, accessible layered help, close controls, motion preferences and saved-hand navigation.')
asyncio.run(main())
