"""Navigate between activities, resume a hand, interrupt wheel and claim flights."""
import asyncio
from playwright.async_api import async_playwright
async def main():
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True)
  page=await browser.new_page(viewport={'width':1280,'height':800});errors=[]
  page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto('http://127.0.0.1:8765/')
  await page.evaluate("async()=>{const {Session}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const s=new Session();s.p.Unlocked=180;s.p.GuideSeen=[...features.map(f=>f.id),'cards'];s.start(1,false,321);s.save()}")
  await page.reload()
  await page.locator('header [data-action=home]').click()
  await page.locator('#app [data-action=map]').click()
  await page.locator('.map-dock [data-action="ev:album"]').click()
  await page.locator('.related-places [data-action="ev:missions"]').click()
  await page.locator('[data-action=menuBack]').click()
  assert await page.locator('.event-album').is_visible()
  await page.locator('.menu-resume').click()
  before=await page.evaluate("(()=>{const p=JSON.parse(localStorage.getItem('meadow-peaks-profile')).Active;return [p.Level,p.Seed,p.Removed,p.Stock,p.Waste,p.Score]})()")
  await page.locator('.play-actions [data-action=shop]').click()
  await page.locator('#modal [data-action=gems]').click()
  await page.locator('#app [data-action=continue]').click()
  after=await page.evaluate("(()=>{const p=JSON.parse(localStorage.getItem('meadow-peaks-profile')).Active;return [p.Level,p.Seed,p.Removed,p.Stock,p.Waste,p.Score]})()")
  assert before==after,'Shopping/garden navigation replaced hand'
  await page.locator('[data-action=home]').click()
  await page.locator('[data-action=map]').click()
  await page.locator('.map-dock [data-action=wheel]').click()
  await page.locator('[data-action=spinWheel]').click()
  await page.locator('.shell-close').click()
  await page.locator('[data-action=settings]').click()
  await page.wait_for_timeout(4500)
  assert await page.locator('[data-setting=Sound]').is_visible()
  assert await page.locator('[data-action=claimWheel]').count()==0
  assert await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).WheelSpinsUsed")==1
  await page.locator('.shell-close').click()
  await page.locator('.map-controls [data-action="ev:travel"]').click()
  await page.locator('[data-action=goto]').click()
  await page.locator('#goto-level').fill('450')
  await page.locator('[data-action=findLevel]').click()
  await page.wait_for_timeout(150)
  assert await page.locator('[data-action="preview:450"]').evaluate('e=>{const r=e.getBoundingClientRect();return r.top>0&&r.bottom<innerHeight&&r.right>0&&r.left<innerWidth}')
  assert not errors,errors
  await browser.close()
 print('PASS: activity Back routes, Resume cards, shop/garden round trip preserves hand, closing wheel mid-spin leaves settings untouched and consumes one spin.')
asyncio.run(main())
