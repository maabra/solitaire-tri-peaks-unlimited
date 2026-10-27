"""Card ranks use clean glyph fills, valid font weights and room for two digits."""
import asyncio,os
from pathlib import Path
from playwright.async_api import async_playwright
# Scenario: Card ranks use clean glyph fills, valid font weights and room for two digits. Fail on violated assertions rather than accepting screenshots alone.
async def main():
 out=Path('artifacts/card-lettering');out.mkdir(exist_ok=True)
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True);page=await b.new_page(viewport={'width':1440,'height':900},reduced_motion='reduce');await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')");await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'));await page.locator('.map-controls [data-action=previewCurrent]').click();await page.locator('[data-action="level:1"]').click();await page.evaluate('document.fonts.ready')
  assert await page.locator('.card .face-rank').first.get_attribute('data-rank') is not None
  await page.evaluate('''()=>{
   const source=document.querySelector('.board .card:not(.back)'),host=document.createElement('div');host.id='rank-gallery';host.style.cssText='display:grid;grid-template-columns:repeat(13,76px);gap:10px;padding:20px;background:#fff6df';
   for(let suit=0;suit<4;suit++)for(let rank=1;rank<=13;rank++){
    const card=source.cloneNode(true),text=['','A','2','3','4','5','6','7','8','9','10','J','Q','K'][rank];card.style.cssText='position:relative;left:auto;top:auto;transform:none;--cw:76px;--ch:105px;width:76px;height:105px';card.className='card';card.dataset.suit=suit;card.removeAttribute('data-action');card.querySelector('.rank').textContent=text;const face=card.querySelector('.face-rank');face.textContent=text;face.dataset.rank=rank;card.querySelector('.face-suit').textContent=['\u2660','\u2665','\u2666','\u2663'][suit];card.querySelector('.obstacle')?.remove();host.append(card);
   }
   document.querySelector('#app').replaceChildren(host);
  }''')
  result=await page.locator('#rank-gallery .face-rank').evaluate_all("es=>es.every(e=>{const s=getComputedStyle(e),r=e.getBoundingClientRect(),card=e.closest('.card').getBoundingClientRect();return s.webkitTextStrokeWidth==='0px'&&s.textShadow==='none'&&Number(s.fontWeight)<=800&&r.left>=card.left&&r.right<=card.right&&e.scrollWidth<=e.clientWidth+1})")
  assert result,'Rank outline, synthetic weight or clipped digits remain'
  await page.screenshot(path=str(out/'all-ranks.png'));await b.close()
 print('PASS: all 13 ranks in four suits have no outline/shadow seams, supported font weight and unclipped digits.')
asyncio.run(main())
