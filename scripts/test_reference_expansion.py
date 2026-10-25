"""Exercise Shuffle, scenic/table selection and short-laptop control bounds."""
import asyncio
from playwright.async_api import async_playwright

async def main():
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True)
  page=await b.new_page(viewport={'width':800,'height':600},reduced_motion='reduce');errors=[]
  page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto('http://127.0.0.1:8765/')
  await page.evaluate("""async()=>{const {Session}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const {helpers}=await import('./core.mjs');const s=new Session();s.p.Unlocked=50000;s.p.Hearts=999;s.p.Coins=1000;s.p.GuideSeen=[...features.map(f=>f.id),'cards',...Array.from({length:17},(_,i)=>'mechanic'+i),...helpers.filter(h=>h.Id!=='shuffle').map(h=>'helper-'+h.Id)];s.add('shuffle',2);s.start(50000,false,12345);s.save()}""")
  await page.reload();await page.locator('[data-action=previewCurrent]').click();await page.locator('[data-action="level:50000"]').click();await page.wait_for_timeout(300)
  assert await page.evaluate("document.body.classList.contains('scenic-play')")
  assert await page.evaluate("document.querySelector('.play-actions').getBoundingClientRect().bottom<=innerHeight"),await page.locator('.play-actions').bounding_box()
  assert await page.locator('.board .covered[title="Covered card"]').count()>0
  await page.locator('[data-action="helper:shuffle"]').click()
  assert await page.locator('.bird-guide').is_visible()
  assert await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile')).Items.shuffle")==2
  await page.locator('[data-action=birdDismiss]').click();await page.locator('[data-action="helper:shuffle"]').click()
  state=await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile'))")
  assert state['Items']['shuffle']==1
  assert state['Active']['Mechanics']['Faces']
  assert await page.locator('[data-action="helper:shuffle"]').is_disabled()
  await page.locator('header [data-action=settings]').click()
  await page.locator('[data-action="surface:table"]').click()
  assert not await page.evaluate("document.body.classList.contains('scenic-play')")
  await page.locator('[data-action="surface:scene"]').click()
  assert await page.evaluate("document.body.classList.contains('scenic-play')")
  await page.locator('.shell-close').click()
  await page.reload();await page.locator('[data-action=previewCurrent]').click();await page.locator('[data-action="level:50000"]').click()
  restored=await page.evaluate("JSON.parse(localStorage.getItem('meadow-peaks-profile'))")
  assert restored['Active']['Mechanics']['Faces']==state['Active']['Mechanics']['Faces']
  assert restored['Items']['shuffle']==1 and restored['Hearts']==state['Hearts']
  assert not errors,errors;await b.close()
 print('PASS: laptop controls visible, covered labels private, Shuffle introduction/use/cooldown, scenery/table switching and saved ranks/hearts preserved.')
asyncio.run(main())
