"""Debug-jump through reference levels; these are observations, not earned progress."""
import json
from pathlib import Path
from PIL import Image,ImageDraw

# Capture reference scenes and their observed layout details for visual review.
async def study(page):
 import sys
 if '--menus' in sys.argv:
  await menus(page);return
 out=Path('artifacts/reference-study/gallery');out.mkdir(exist_ok=True)
 catalog=json.loads(Path('artifacts/reference-study/catalog.json').read_text())
 report=[];levels=[3,10,40,50,51,61,100,150,200,241,301,400,481,560]
 for level in levels:
  bg=int(catalog[level-1]['settings']['Background']);world=1 if bg<=7 else 2+(bg-8)//6
  await page.evaluate("""async ({level,world})=>{clearTimeout(window.TimerCheck);const data=await fetch('resource/map/map'+world+'.json').then(r=>r.json());game.cache.addJSON('mapjson'+world,null,data);window.map_n=world;window.level=level;window.all_level=Array(560).fill(3);window.all_level_col=560;window.coins=5000;window.lifes=5;game.state.start('Game')}""",{'level':level,'world':world})
  await page.wait_for_timeout(5500)
  observation=await page.evaluate("""()=>{const text=[],cards=[],controls=[];function visit(o){if(o.worldVisible!==false&&o.visible&&o.alpha>0){if(typeof o.text==='string')text.push(o.text);if(o.inputEnabled){const x=o.world.x-game.camera.x,y=o.world.y-game.camera.y;if(x>=0&&x<1280&&y>=0&&y<800)controls.push({frame:o.frameName,x,y});if(/card/.test(o.frameName??''))cards.push({value:o.value,hazard:o.hazard_type,frame:o.frameName,angle:o.angle})}}for(const c of o.children??[])visit(c)}visit(game.world);return {text,cards,controls}}""")
  report.append({'level':level,**observation})
  await page.screenshot(path=str(out/f'level-{level}.png'))
  print('Observed reference level',level,'cards',len(observation['cards']),'text',observation['text'][-3:],flush=True)
 (out/'observations.json').write_text(json.dumps(report,indent=2))
 sheet=Image.new('RGB',(1280,4*225),'#efe0bc');draw=ImageDraw.Draw(sheet)
 for i,level in enumerate(levels):
  im=Image.open(out/f'level-{level}.png');im.thumbnail((320,200));x=i%4*320;y=i//4*225;sheet.paste(im,(x,y+22));draw.text((x+8,y+5),f'Reference level {level}',fill='#352d33')
 sheet.save(out/'overview.jpg')

# Inspect the available reference menus and retain their captured visual states.
async def menus(page):
 out=Path('artifacts/reference-study/menus');out.mkdir(exist_ok=True)
 await page.evaluate("""()=>{clearTimeout(window.TimerCheck);window.map_n=1;window.level=20;window.level_finish=20;window.first_start=0;window.all_level=Array(560).fill(3);window.all_level_col=560;window.coins=5000;window.keys=10;game.state.start('Map')}""")
 await page.wait_for_timeout(8000)
 await page.mouse.click(973,86);await page.wait_for_timeout(1000)
 await page.screenshot(path=str(out/'map.png'))
 await page.mouse.click(1160,70);await page.wait_for_timeout(1700)
 await page.screenshot(path=str(out/'wheel.png'))
 controls=await page.evaluate("""()=>{const out=[];function walk(o){if(o.worldVisible!==false&&o.visible&&o.alpha>0){const x=o.world?.x-game.camera.x,y=o.world?.y-game.camera.y;if(x>=0&&x<1280&&y>=0&&y<800&&(o.inputEnabled||o.text))out.push({frame:o.frameName,text:o.text,x,y})}for(const c of o.children??[])walk(c)}walk(game.world);return out}""")
 (out/'controls.json').write_text(json.dumps(controls,indent=2));print('Menu controls:',controls,flush=True)
 await page.mouse.click(974,86);await page.wait_for_timeout(600)
 await page.mouse.click(30,521);await page.wait_for_timeout(1100)
 await page.screenshot(path=str(out/'collection.png'))
