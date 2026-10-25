"""Audio settings persistence, synthesis output, pause and voice cleanup."""
import asyncio
from playwright.async_api import async_playwright
async def main():
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True)
  page=await b.new_page();errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
  await page.add_init_script("localStorage.setItem('meadow-storage-consent','yes')")
  await page.goto('http://127.0.0.1:8765/')
  await page.locator('[data-action=settings]').click()
  await page.locator('[data-setting=Sound]').check();await page.locator('[data-setting=Music]').check()
  await page.locator('[data-audio-volume=MusicVolume]').fill('22');await page.locator('[data-audio-volume=MusicVolume]').dispatch_event('change')
  assert await page.evaluate("(()=>{const p=JSON.parse(localStorage.getItem('meadow-peaks-profile'));return p.Sound&&p.Music&&p.MusicVolume===.22})()")
  stats=await page.evaluate("""async()=>{const {MeadowAudio}=await import('./audio-engine.mjs');const settings={Sound:true,Music:true,EffectsVolume:.5,MusicVolume:.3};const a=new MeadowAudio(()=>settings);await a.unlock();for(const name of ['card','draw','gem','helper','win','coin','reveal','wheel','error'])a.play(name);const voices=a.voices.size;a.pause();const paused=a.timer===null;a.dispose();const ctx=new OfflineAudioContext(1,44100*4,44100);const score=new MeadowAudio(()=>settings);score.context=ctx;score.music=ctx.createGain();score.music.gain.value=.3;score.music.connect(ctx.destination);score.next=.05;score.schedule(3);const buffer=await ctx.startRendering(),data=buffer.getChannelData(0);let sum=0,peak=0;for(const v of data){sum+=v*v;peak=Math.max(peak,Math.abs(v));}document.removeEventListener('visibilitychange',score.hidden);return {voices,paused,rms:Math.sqrt(sum/data.length),peak,remaining:score.voices.size}}""")
  assert stats['voices']>10 and stats['paused'] and stats['rms']>.0001 and stats['peak']<1 and stats['remaining']==0,stats
  await page.reload();await page.locator('[data-action=settings]').click()
  assert await page.locator('[data-setting=Music]').is_checked()
  assert await page.locator('[data-audio-volume=MusicVolume]').input_value()=='22'
  assert not errors,errors
  await b.close()
 print('PASS: sound/music toggles and levels persist; nine cues synthesize; music produces unclipped audio; pause stops scheduler and voices clean up.')
asyncio.run(main())
