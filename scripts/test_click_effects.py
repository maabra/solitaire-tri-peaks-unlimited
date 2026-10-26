"""Native pointer clicks keep the ripple on the card in centered/scrolled games."""
import asyncio,os
from pathlib import Path
from playwright.async_api import async_playwright
async def main():
 out=Path('artifacts/click-effects');out.mkdir(exist_ok=True)
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True)
  for width,height in [(1920,1080),(1440,900),(700,432)]:
   page=await b.new_page(viewport={'width':width,'height':height});await page.clock.install();await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')");await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'))
   ace=await page.evaluate('''async()=>{const {Session}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const s=new Session();s.p.Unlocked=3;s.p.GuideSeen=[...features.map(f=>f.id),'cards'];for(let seed=1;seed<500;seed++){s.start(3,false,seed,'MP21');const h=s.hand,i=h.Deal.Cards.findIndex((c,i)=>h.isExposed(i)&&h.face(i).Rank===1&&!h.Moves.includes(i));if(i>=0){s.save();return i}}throw Error('No invalid exposed Ace fixture')}''')
   await page.reload();await page.evaluate("()=>{const e=document.createElement('button');e.dataset.action='continue';document.body.append(e);e.click();e.remove()}");await page.clock.run_for(1200)
   card=page.locator(f'[data-action="card:{ace}"]');await card.evaluate('e=>e.getAnimations().forEach(a=>a.finish())');await card.scroll_into_view_if_needed();await page.clock.run_for(30);await card.evaluate('e=>e.getAnimations().forEach(a=>a.finish())');source=await card.bounding_box();x=source['x']+source['width']/2;y=source['y']+source['height']/2
   await page.mouse.click(x,y);await page.clock.run_for(16)
   ripple=page.locator('.tap-ripple').last;assert await ripple.count()==1,(width,source,await page.evaluate('([x,y])=>({hit:document.elementFromPoint(x,y)?.outerHTML,scroll:[scrollX,scrollY]})',[x,y]))
   r=await ripple.bounding_box();assert abs(r['x']+r['width']/2-x)<1 and abs(r['y']+r['height']/2-y)<1,(width,source,r)
   await page.screenshot(path=str(out/f'ace-click-{width}.png'));await page.mouse.move(0,0);await page.clock.run_for(1000)
   await card.evaluate('e=>e.getAnimations().forEach(a=>a.finish())');after=await card.bounding_box();assert all(abs(after[k]-source[k])<1 for k in ['x','y','width','height']),'Invalid click moved the card permanently'
   await page.emulate_media(reduced_motion='reduce');await page.mouse.click(x,y);assert await page.locator('.tap-ripple').count()==0
   await page.close()
  await b.close()
 print('PASS: native Ace clicks keep their ripple at the pointer in centered and scrolled games; invalid cards stay put and reduced motion suppresses the effect.')
asyncio.run(main())
