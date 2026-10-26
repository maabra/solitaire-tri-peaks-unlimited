"""Regression for the supplied recording, flight coordinates, popup and full UI."""
import asyncio
from pathlib import Path
from playwright.async_api import async_playwright

async def main():
 out=Path('artifacts/recorded-regression');out.mkdir(exist_ok=True)
 async with async_playwright() as p:
  browser=await p.chromium.launch(channel='msedge',headless=True)
  page=await browser.new_page(viewport={'width':1440,'height':900})
  errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto('http://127.0.0.1:8765/')
  await page.evaluate("""async()=>{const {Session}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const {helpers}=await import('./core.mjs');const s=new Session();s.p.Unlocked=2;s.p.GuideSeen=[...features.map(f=>f.id),'cards',...helpers.map(h=>'helper-'+h.Id)];s.start(2,false,106723,'MP21');s.save()}""")
  await page.reload()
  await page.locator('.map-controls [data-action=previewCurrent]').click()
  await page.locator('#modal').evaluate("async()=>{const i=new Image();i.src='assets/level-notice-board.png';await i.decode()}")
  await page.wait_for_timeout(300)
  assert await page.locator('#dialog').evaluate('e=>{const r=e.getBoundingClientRect();return r.width<=521&&r.height<=600&&e.scrollHeight<=e.clientHeight+2}')
  await page.screenshot(path=str(out/'compact-notice.png'))
  await page.locator('[data-action="level:2"]').click()
  await page.wait_for_timeout(700)
  assert await page.locator('.board .card').count()==28
  assert await page.locator('#app').evaluate('e=>e.scrollHeight<=e.clientHeight+2')
  assert await page.evaluate("""()=>{const status=document.querySelector('.table-status').getBoundingClientRect(),stock=document.querySelector('[data-action=draw]').getBoundingClientRect(),footer=document.querySelector('.play-actions').getBoundingClientRect(),labels=[...document.querySelectorAll('.hand-row .pile small')].map(e=>e.getBoundingClientRect());return status.bottom+2<=stock.top&&labels.every(r=>r.bottom+2<=footer.top)}"""),'Stock/status/footer overlap'
  result=await page.evaluate(r"""async()=>{
   const {Session}=await import('./session.mjs');const s=new Session();const move=s.hand.Moves[0];
   const source=document.querySelector('[data-action="card:'+move+'"]'),a=source.getBoundingClientRect(),target=document.querySelector('.hand-row .pile:nth-child(3) .card').getBoundingClientRect();
   const survivors=new Map([...document.querySelectorAll('.board .card')].filter(e=>e!==source).map(e=>[e.dataset.action,{node:e,rect:e.getBoundingClientRect().toJSON()}]));window.regressionSurvivors=survivors;
   source.click();const flight=document.querySelector('[data-flight=play]');if(!flight)return {exists:false};
   const animation=flight.getAnimations()[0];animation.pause();const frames=animation.effect.getKeyframes(),stage=document.body.getBoundingClientRect();
   const x=parseFloat(flight.style.left)+parseFloat(flight.style.width)/2+stage.left,y=parseFloat(flight.style.top)+parseFloat(flight.style.height)/2+stage.top;
   const end=frames.at(-1).transform,m=end.match(/translate\(([-.\d]+)px,\s*([-.\d]+)px\)/);
   const result={exists:true,origin:Math.abs(x-a.x-a.width/2)<1&&Math.abs(y-a.y-a.height/2)<1,lands:!!m&&Math.abs(x+Number(m[1])-target.x-target.width/2)<1&&Math.abs(y+Number(m[2])-target.y-target.height/2)<1,spins:frames.some(f=>f.transform.includes('rotate(360deg)'))};
   animation.play();return result;
  }""")
  assert all(result.values()),result
  await page.wait_for_timeout(800)
  assert await page.locator('.card-flight,.flight-spark').count()==0
  assert await page.evaluate("""()=>[...regressionSurvivors].every(([id,{node,rect}])=>{const e=document.querySelector('[data-action="'+id+'"]');if(!e)return true;const r=e.getBoundingClientRect();return e===node&&['x','y','width','height'].every(k=>Math.abs(r[k]-rect[k])<.6)})""")
  await page.screenshot(path=str(out/'recorded-deal.png'))
  assert await page.locator('#frame-ornaments').evaluate('e=>{const a=e.querySelector(".tl").getBoundingClientRect(),b=document.body.getBoundingClientRect();return a.left<b.left&&a.top<b.top}')
  await page.emulate_media(reduced_motion='reduce')
  await page.locator('[data-action=draw]').click()
  assert await page.locator('.card-flight,.flight-spark').count()==0
  await page.evaluate("""async()=>{const {Session}=await import('./session.mjs');const {features}=await import('./journey-guide.mjs');const {helpers}=await import('./core.mjs');const s=new Session();s.p.Unlocked=50095;s.p.HighestPlayedLevel=480000;s.p.GuideSeen=[...features.map(f=>f.id),'cards',...Array.from({length:17},(_,i)=>'mechanic'+i),...helpers.map(h=>'helper-'+h.Id)];for(const h of helpers)s.p.Items[h.Id]=5;s.start(50095,false,12345);s.save()}""")
  await page.reload();await page.locator('.map-controls [data-action=previewCurrent]').click();await page.locator('[data-action="level:50095"]').click()
  assert await page.locator('#app').evaluate('e=>e.scrollHeight<=e.clientHeight+2')
  assert await page.locator('.tools').evaluate('e=>e.scrollHeight<=e.clientHeight+2'),'Full helper inventory is clipped'
  assert await page.locator('.tools button,.play-actions button,.subhead').evaluate_all('es=>es.every(e=>{const a=e.getBoundingClientRect(),b=document.body.getBoundingClientRect();return a.top>=b.top+6&&a.bottom<=b.bottom-6&&a.left>=b.left&&a.right<=b.right})')
  await page.locator('[data-action="helper:remove"]').click()
  assert await page.locator('.board-shell>.notice').is_visible()
  assert await page.locator('#app').evaluate('e=>e.scrollHeight<=e.clientHeight+2'),'Helper prompt adds a row below frame'
  await page.locator('[data-action=cancelTarget]').click()
  await page.locator('[data-action="helper:peek"]').click()
  assert await page.locator('#peek').is_visible(),'Peek information is hidden'
  assert await page.locator('#peek').evaluate('e=>e.textContent.includes("Next draws")')
  await page.screenshot(path=str(out/'long-level-full-inventory.png'))
  await page.locator('.play-actions [data-action=shop]').click()
  await page.screenshot(path=str(out/'full-helper-shop.png'))
  shop=await page.locator('#dialog').evaluate('e=>({height:e.clientHeight,scroll:e.scrollHeight})')
  assert shop['scroll']<=shop['height']+2,shop
  assert not errors,errors
  await browser.close()
 print('PASS: recorded deal, local spinning flights, stable survivors, reduced motion, compact notice, exterior flowers, long-level controls, helper prompts, Peek and full shop.')

asyncio.run(main())
