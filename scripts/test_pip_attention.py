"""Pip clears the picture frame; ambient badges/glints have independent cadence."""
import asyncio,os
from pathlib import Path
from playwright.async_api import async_playwright
async def main():
 out=Path('artifacts/pip-attention');out.mkdir(exist_ok=True)
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True);page=await b.new_page(viewport={'width':1440,'height':900})
  await page.clock.install();await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes');window.attentionEvents=[];let nextId=0;const original=Element.prototype.animate;Element.prototype.animate=function(...args){if(this.matches('.new-feature-badge,.ambient-glint')){this.dataset.testAttentionId??=String(++nextId);window.attentionEvents.push({id:this.dataset.testAttentionId,badge:this.matches('.new-feature-badge'),at:performance.now()})}return original.apply(this,args)}")
  await page.goto(os.environ.get('MEADOW_TEST_URL','http://127.0.0.1:8765/'))
  await page.evaluate("async()=>{const {Session}=await import('./session.mjs');const s=new Session();s.p.Unlocked=12000;s.p.HighestPlayedLevel=12000;s.p.GuideSeen=[];s.save()}");await page.reload();await page.clock.run_for(100)
  assert await page.locator('.map-companion').evaluate("e=>{const frame=document.querySelector('.map-picture-frame').getBoundingClientRect(),body=document.body.getBoundingClientRect();return [...e.querySelectorAll('.bird-help,p')].every(n=>{const r=n.getBoundingClientRect();return r.left>=frame.right+4&&r.right<=body.right-12&&r.top>=body.top+12&&r.bottom<=body.bottom-12})}")
  assert await page.locator('.map-companion .pip-sprite').evaluate("e=>getComputedStyle(e).width==='100px'")
  delays=await page.locator('[data-attention-delay]').evaluate_all('es=>es.map(e=>Number(e.dataset.attentionDelay))');assert len(delays)>4 and len(set(delays))>3 and all(4000<=n<14000 for n in delays)
  await page.screenshot(path=str(out/'pip-outside-picture.png'))
  await page.clock.run_for(28000)
  events=await page.evaluate('window.attentionEvents');assert any(e['badge'] for e in events) and any(not e['badge'] for e in events),events
  assert len(set(e['at'] for e in events))>3,'All attention effects synchronized'
  new_delays=await page.locator('[data-attention-delay]').evaluate_all('es=>es.map(e=>Number(e.dataset.attentionDelay))');assert any(a!=b for a,b in zip(delays,new_delays)),'Repeat intervals were not randomized'
  await page.evaluate("document.body.classList.add('reduced')");await page.clock.run_for(20);before=await page.evaluate('window.attentionEvents.length');await page.clock.run_for(15000);assert before==await page.evaluate('window.attentionEvents.length'),'Reduced motion did not stop ambient effects'
  await b.close()
 print('PASS: full-size Pip sits outside the map frame; badges and glints use independent random waits and reduced motion stops them.')
asyncio.run(main())
