import asyncio,json,mimetypes
from urllib.request import urlopen
from pathlib import Path
from urllib.parse import quote
from playwright.async_api import async_playwright
# Open the saved reference in an isolated browser and inspect its resource failures and interactive states.
async def main():
 root=Path('examples/website version');saved=next(root.glob('*10_files'))/'a_data'/'a_M6vp.htm';url='http://127.0.0.1:8766/'+quote(saved.relative_to(root).as_posix())
 async with async_playwright() as p:
  b=await p.chromium.launch(channel='msedge',headless=True);page=await b.new_page(viewport={'width':1250,'height':781});errors=[];bad=[]
  page.on('pageerror',lambda e:errors.append(str(e)));page.on('response',lambda r:bad.append((r.status,r.url)) if r.status>=400 else None)
  # Serve cached reference resources locally, rejecting paths that escape the cache directory.
  async def route(r):
   url=r.request.url
   if '/resource/' in url and url.startswith('http://127.0.0.1:8766/'):
    relative=url.split('/resource/',1)[1].split('?')[0]
    from urllib.parse import unquote
    relative=unquote(relative)
    base=Path('artifacts/reference-study/cache').resolve();file=(base/relative).resolve()
    if not file.is_relative_to(base):await r.abort();return
    try:
     if not file.exists():
      data=await asyncio.to_thread(lambda:urlopen('https://sg.gamedistribution.com/solitaire-tripeaks/resource/'+quote(relative,safe='/'),timeout=20).read())
      file.parent.mkdir(parents=True,exist_ok=True);file.write_bytes(data)
     await r.fulfill(status=200,body=file.read_bytes(),content_type=mimetypes.guess_type(file.name)[0] or 'application/octet-stream')
    except Exception as e:
     print('Resource failed',relative,str(e),flush=True);await r.abort()
   elif url.startswith('http://127.0.0.1:8766/') or url.startswith('data:'):await r.continue_()
   else:await r.abort()
  await page.route('**/*',route)
  await page.goto(url,wait_until='domcontentloaded');await page.wait_for_function("window.game?.state?.current==='Game'",timeout=30000);await page.wait_for_timeout(15000)
  await page.evaluate("()=>{for(const c of document.querySelectorAll('canvas'))if(c!==window.game.canvas)c.remove();document.querySelectorAll('#pr-backdrop-container').forEach(e=>e.remove());scrollTo(0,0);game.scale.refresh()}")
  # The archived page cannot receive hosting/ad callbacks. Satisfy navigation
  # callbacks locally for study only; no SDK code or assets enter our game.
  await page.evaluate("""()=>{const original=sdkHandler.trigger.bind(sdkHandler);sdkHandler.trigger=(event,data,...args)=>{if(['beforePlayButtonDisplay','playButtonPressed','save'].includes(event)){setTimeout(()=>data?.callback?.call(window),0);return;}return original(event,data,...args)}}""")
  import sys
  if '--gallery' in sys.argv:
   from reference_gallery import study
   await study(page);await b.close();return
  Path('artifacts/reference-study').mkdir(exist_ok=True)
  await page.screenshot(path='artifacts/reference-study/saved-game-local.png')
  for name,x,y in [('three',246,427),('four',375,375),('five',500,355),('six',625,335),('seven',875,375),('eight',750,355),('nine',1000,410)]:
   await page.mouse.click(x,y)
   if name=='three':
    for delay in [80,140,220,400]:
     await page.wait_for_timeout(delay);await page.screenshot(path=f'artifacts/reference-study/move-{delay}.png')
   await page.wait_for_timeout(2400)
   await page.screenshot(path=f'artifacts/reference-study/step-{name}.png')
  for rank in [10,11,12,13,1,2]:
   point=await page.evaluate("""rank=>{let found;function visit(o){if(o.inputEnabled&&o.value===rank&&o.worldVisible!==false)found={x:o.world.x*1250/1280,y:o.world.y*781/800};for(const c of o.children??[])visit(c)}visit(game.world);return found}""",rank)
   if point:await page.mouse.click(point['x'],point['y'])
   await page.wait_for_timeout(2400)
   await page.screenshot(path=f'artifacts/reference-study/finish-{rank}.png')
  await page.wait_for_timeout(3000)
  await page.screenshot(path='artifacts/reference-study/saved-game-complete.png')
  await page.wait_for_timeout(18000)
  await page.screenshot(path='artifacts/reference-study/results-settled.png')
  print('Result controls:',await page.evaluate("""()=>{const out=[];function visit(o){if(o.worldVisible!==false&&o.visible&&o.alpha>0&&(o.text||o.inputEnabled))out.push({frame:o.frameName,text:o.text,x:Math.round(o.world?.x??o.x),y:Math.round(o.world?.y??o.y),value:o.value});for(const c of o.children??[])visit(c)}visit(game.world);return out}"""))
  await page.mouse.click(625,614);await page.wait_for_timeout(4000)
  await page.screenshot(path='artifacts/reference-study/after-results.png')
  print('Next controls:',await page.evaluate("""()=>{const out=[];function visit(o){if(o.worldVisible!==false&&o.visible&&o.alpha>0&&(o.text||o.inputEnabled))out.push({frame:o.frameName,text:o.text,x:Math.round(o.world?.x??o.x),y:Math.round(o.world?.y??o.y)});for(const c of o.children??[])visit(c)}visit(game.world);return out}"""))
  await page.wait_for_timeout(8000)
  await page.screenshot(path='artifacts/reference-study/map-ready.png')
  # Interact with the reference frame and save the resulting state for inspection.
  async def click_frame(pattern):
   point=await page.evaluate("""pattern=>{let found;const re=new RegExp(pattern);function visit(o){if(!found&&o.inputEnabled&&o.worldVisible!==false&&o.visible&&o.alpha>0&&re.test(o.frameName??''))found={x:(o.world.x-game.camera.x)*1250/1280,y:(o.world.y-game.camera.y)*781/800};for(const c of o.children??[])visit(c)}visit(game.world);return found}""",pattern)
   if point:await page.mouse.click(point['x'],point['y']);return True
   return False
  await click_frame('^current_level_node');await page.wait_for_timeout(1800)
  await page.screenshot(path='artifacts/reference-study/level-two-preview.png')
  print('Preview opened; state:',await page.evaluate('game.state.current'))
  await page.mouse.click(625,575);await page.wait_for_timeout(10000)
  await page.screenshot(path='artifacts/reference-study/level-two.png')
  await click_frame('button_setting');await page.wait_for_timeout(700)
  await page.screenshot(path='artifacts/reference-study/settings.png')
  print('Final errors:',json.dumps(errors[:8]))
  print('Input hit:',await page.evaluate("()=>({element:document.elementFromPoint(246,427)?.outerHTML.slice(0,180),paused:game.paused,offset:{x:game.scale.offset.x,y:game.scale.offset.y},pointer:{x:game.input.activePointer.x,y:game.input.activePointer.y}})"));print('Active game state:',await page.evaluate("game.state.current"))
  await b.close()
asyncio.run(main())
