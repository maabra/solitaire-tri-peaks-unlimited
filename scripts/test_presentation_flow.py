"""Actual win/reveal/claim and wheel pointer flows; no extra reward grants."""
import asyncio
from playwright.async_api import async_playwright
async def main():
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True)
  page=await browser.new_page(viewport={'width':1280,'height':800});errors=[]
  page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto('http://127.0.0.1:8765/')
  last=await page.evaluate("""async()=>{const {Session}=await import('./session.mjs');const s=new Session();s.start(1,false,12345);const path=s.hand.Deal.Solution;for(const step of path.slice(0,-1)){if(step.Index<0){s.draw();s.tick(2);}else if(!s.play(step.Index))throw Error('Fixture move failed');}s.p.GuideSeen=['welcome','cards','garden','missions','wheel','chest','postcard'];s.hand.s.Earned.Packs=[1];s.save();return path.at(-1).Index;}""")
  await page.reload();await page.locator('[data-action=previewCurrent]').click();await page.locator('[data-action="level:1"]').click()
  await page.locator(f'[data-action="card:{last}"]').click()
  await page.locator('.completion-stage').wait_for();await page.locator('.completion-stage').click()
  await page.locator('[data-action=showWinRewards]').wait_for()
  await page.wait_for_timeout(1100)
  await page.screenshot(path='artifacts/study-results.png')
  before=await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Coins")
  await page.locator('[data-action=showWinRewards]').click()
  assert await page.locator('#claim-win').is_disabled()
  await page.locator('[data-action=revealAllGifts]').click()
  assert await page.locator('#claim-win').is_enabled()
  assert not await page.locator('[data-action=revealAllGifts]').is_visible()
  for width,height in [(1280,800),(390,844),(320,640)]:
   await page.set_viewport_size({'width':width,'height':height})
   assert await page.locator('#dialog').evaluate('e=>e.scrollWidth<=e.clientWidth+2')
  await page.screenshot(path='artifacts/study-gifts-320.png')
  await page.set_viewport_size({'width':1280,'height':800})
  await page.locator('#claim-win').dblclick()
  await page.locator('.map-viewport').wait_for()
  after=await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Coins")
  assert before==after,(before,after)
  await page.evaluate("const b=document.createElement('button');b.dataset.action='wheel';b.id='test-wheel';document.body.append(b)")
  await page.locator('#test-wheel').evaluate('e=>e.click()')
  await page.locator('[data-action=spinWheel]').click()
  assert await page.locator('.prize-wheel').evaluate("e=>getComputedStyle(e).transform==='none'")
  await page.locator('[data-action=claimWheel]').wait_for(timeout=10000)
  await page.screenshot(path='artifacts/study-wheel.png')
  assert await page.locator('.wheel-needle').evaluate("e=>getComputedStyle(e).transform!=='none'")
  await page.locator('[data-action=claimWheel]').click()
  # Leaving during the coin flight must not close a subsequently opened menu.
  last=await page.evaluate("""async()=>{const {Session}=await import('./session.mjs');const s=new Session();s.start(1,false,12345);const path=s.hand.Deal.Solution;for(const step of path.slice(0,-1)){if(step.Index<0){s.draw();s.tick(2);}else s.play(step.Index);}s.hand.s.Earned.Packs=[1];s.save();return path.at(-1).Index;}""")
  await page.reload()
  # Level 1 is on this chapter; chapter search now belongs to Travel Hub.
  await page.locator('[data-action="preview:1"]').click();await page.locator('[data-action="level:1"]').click()
  await page.locator(f'[data-action="card:{last}"]').click();await page.locator('.completion-stage').click()
  await page.locator('[data-action=showWinRewards]').click();await page.locator('[data-action=revealAllGifts]').click()
  await page.locator('#claim-win').click();await page.locator('.shell-close').click();await page.locator('[data-action=settings]').click()
  await page.wait_for_timeout(1100)
  assert await page.locator('[data-setting=Sound]').is_visible(),'Old Claim closed the new settings menu'
  await page.emulate_media(reduced_motion='reduce')
  assert await page.evaluate("async()=>{const {presentCompletion}=await import('./presentation-flow.mjs');let calls=0;await presentCompletion({Stock:[1,2]},()=>true,()=>calls++);return calls===1&&!document.querySelector('.completion-stage')}")
  assert not errors,errors
  await browser.close()
 print('PASS: real final move, skip, results, reward reveal, double Claim preserves balance, stationary wheel and rotating pointer.')
asyncio.run(main())
