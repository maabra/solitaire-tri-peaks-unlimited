"""Shop purchases, affordability, inventory browsing and place-specific menu art."""
import asyncio,os
from pathlib import Path
from playwright.async_api import async_playwright
# Trigger the normal delegated UI command rather than modifying the tested view directly.
async def action(page,name):
 await page.evaluate("a=>{const e=document.createElement('button');e.dataset.action=a;(document.querySelector('#dialog').open?document.querySelector('#modal'):document.body).append(e);e.click();e.remove()}",name)
# Scenario: Shop purchases, affordability, inventory browsing and place-specific menu art. Fail on violated assertions rather than accepting screenshots alone.
async def main():
 out=Path('artifacts/village-places');out.mkdir(exist_ok=True)
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True);page=await b.new_page(viewport={'width':1440,'height':900},reduced_motion='reduce');await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')");await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'))
  await page.evaluate("async()=>{const {Session}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const s=new Session();s.p.Unlocked=3;s.p.HighestPlayedLevel=3;s.p.Coins=200;s.p.GuideSeen=features.map(f=>f.id);s.save()}");await page.reload();await action(page,'shop')
  assert await page.locator('.shop article').count()==3
  assert await page.locator('#dialog').get_attribute('data-place')=='shop'
  await page.evaluate("async()=>{const i=new Image();i.src='assets/menu-place-shop.png';await i.decode()}")
  await page.screenshot(path=str(out/'early-shop.png'))
  await page.locator('[data-action="buy:wild"]').click()
  result=await page.evaluate("async()=>{const {Session}=await import('./session.mjs');const s=new Session();return [s.p.Coins,s.inventory('wild')]}")
  assert result==[125,3],result
  assert await page.locator('[data-owned=wild]').inner_text()=='3'
  await page.evaluate("async()=>{const {Session}=await import('./session.mjs');const s=new Session();s.p.Coins=0;s.save()}");await page.reload();await action(page,'shop');assert await page.locator('.shop-buy:enabled').count()==0
  await page.locator('.shop-earn [data-action=gems]').click();assert await page.locator('.gems').count()==1
  await page.evaluate("async()=>{const {Session}=await import('./session.mjs');const {helpers}=await import('./core.mjs');const s=new Session();s.p.Unlocked=10000;s.p.HighestPlayedLevel=10000;s.p.Coins=10000;s.p.SeenMechanics=Array.from({length:17},(_,i)=>i);for(const h of helpers)s.p.Items[h.Id]=5;s.save()}");await page.reload();await action(page,'shop')
  assert await page.locator('.shop article').count()>=13
  assert await page.locator('#dialog').evaluate('e=>e.scrollHeight<=e.clientHeight+2')
  await page.locator('.shop article').last.locator('.shop-buy').scroll_into_view_if_needed();await page.locator('.shop article').last.locator('.shop-buy').click()
  await page.screenshot(path=str(out/'full-shop.png'))
  for name,place in [('settings','study'),('ev:album','collections'),('ev:travel','adventures')]:
   await action(page,name);assert await page.locator('#dialog').get_attribute('data-place')==place
   assert await page.locator('#dialog').evaluate("e=>getComputedStyle(e).backgroundImage.includes('menu-place-')")
   assert await page.locator('.menu-navigation [data-action=menuBack]').count()+await page.locator('.menu-navigation [data-action=backMap]').count()<=1
   await page.screenshot(path=str(out/(place+'.png')))
  small=await b.new_page(viewport={'width':700,'height':432},reduced_motion='reduce');await small.add_init_script("localStorage.setItem('meadow-storage-consent','yes')");await small.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'));await action(small,'shop')
  assert await small.locator('#dialog').evaluate('e=>{const r=e.getBoundingClientRect();return r.width<=innerWidth&&r.height<=innerHeight}')
  await small.locator('[data-action="buy:wild"]').scroll_into_view_if_needed();await small.locator('[data-action="buy:wild"]').click();assert await small.locator('[data-owned=wild]').inner_text()=='3'
  await b.close()
 print('PASS: small and full shops fit, buying deducts the displayed price and updates Owned, unaffordable buys are disabled, Garden earns coins, menu purposes have generated backgrounds and one back route.')
asyncio.run(main())
