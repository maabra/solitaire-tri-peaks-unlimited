"""Purchases visibly confirm the exact +1 without changing transaction or scroll behavior."""
import asyncio,os
from pathlib import Path
from playwright.async_api import async_playwright

# Open the real shop so purchase feedback runs after the session has saved the transaction.
async def open_shop(page):
 await page.evaluate("()=>{const b=document.createElement('button');b.dataset.action='shop';document.body.append(b);b.click();b.remove()}")

# Exercise animated/static feedback, rapid repeat purchases and buying the last affordable item.
async def main():
 out=Path('artifacts/shop-feedback');out.mkdir(exist_ok=True)
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True)
  for width,height,reduced in [(1440,900,False),(1440,900,True),(700,432,True)]:
   page=await browser.new_page(viewport={'width':width,'height':height},reduced_motion='reduce' if reduced else 'no-preference');errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
   await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
   await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'))
   await page.evaluate("async()=>{const {Session}=await import('./session.mjs'),{features}=await import('./journey-guide.mjs');const s=new Session();s.p.Unlocked=3;s.p.Coins=225;s.p.GuideSeen=features.map(f=>f.id);s.save()}");await page.reload();await open_shop(page)
   card=page.locator('[data-shop-helper=wild]');await card.scroll_into_view_if_needed()
   await card.locator('.shop-buy').hover();await page.wait_for_timeout(300)
   before=await card.bounding_box();await card.locator('.shop-buy').click()
   await page.locator('.shop-purchase-message').filter(has_text='+1 added to your bag').wait_for()
   card=page.locator('[data-shop-helper=wild]');assert await card.locator('[data-owned]').inner_text()=='3'
   assert await card.locator('.shop-buy').inner_text()=='Bought!'
   assert await card.locator('.shop-purchase-pop').inner_text()=='+1'
   assert await card.bounding_box()==before,'Receipt moved the product card'
   assert await card.locator('.shop-purchase-pop').evaluate("(e,reduced)=>reduced?getComputedStyle(e).animationName==='none':e.getAnimations().length>0",reduced)
   assert await card.locator('.shop-buy').evaluate("e=>{const a=e.getBoundingClientRect(),b=e.closest('article').getBoundingClientRect();return a.left>=b.left&&a.right<=b.right&&a.top>=b.top&&a.bottom<=b.bottom}")
   await page.wait_for_timeout(250)
   assert await card.locator('.shop-purchase-pop').evaluate('e=>e.scrollWidth<=e.clientWidth'),'+1 wraps or clips'
   await page.screenshot(path=str(out/f'bought-{width}-{reduced}.png'))
   # A second click during Bought! is another intentional purchase, not a second grant from the animation.
   await card.locator('.shop-buy').click();assert await page.locator('[data-owned=wild]').inner_text()=='4'
   await page.locator('[data-shop-helper=wild] .shop-buy').click();assert await page.locator('[data-owned=wild]').inner_text()=='5'
   assert await page.locator('[data-shop-helper=wild] .shop-buy').is_disabled()
   await page.wait_for_timeout(1900)
   assert await page.locator('.shop-purchase-pop').count()==0
   assert await page.locator('[data-shop-helper=wild] .shop-buy').inner_text()!='Bought!'
   saved=await page.evaluate("async()=>{const {Session}=await import('./session.mjs');const s=new Session();return [s.p.Coins,s.inventory('wild')]}")
   assert saved==[0,5],saved
   if width==1440 and not reduced:
    # Confirmations for different helpers coexist, and their timers restore current prices independently.
    await page.evaluate("async()=>{const {Session}=await import('./session.mjs');const s=new Session();s.p.Coins=1000;s.save()}");await page.reload();await open_shop(page)
    await page.locator('[data-action="buy:wild"]').click();await page.locator('[data-action="buy:extra"]').click()
    assert await page.locator('.shop-purchased').count()==2
    assert await page.locator('.shop-purchased .shop-buy').all_text_contents()==['Bought!','Bought!']
    await page.wait_for_timeout(1900);assert await page.locator('.shop-purchased').count()==0
    assert '75' in await page.locator('[data-action="buy:wild"]').inner_text()
    assert '60' in await page.locator('[data-action="buy:extra"]').inner_text()
   assert not errors,errors;await page.close()
  await browser.close()
 print('PASS: Bought!/floating +1 and owned count agree; cards stay still; repeated buys grant exactly one each; unaffordable buys disable; feedback cleans up and honors reduced motion.')
asyncio.run(main())
