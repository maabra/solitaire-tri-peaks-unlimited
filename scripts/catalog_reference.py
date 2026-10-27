"""Inventory the reference's level data for study, never for game packaging."""
import json
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor,as_completed
from urllib.request import urlopen
from collections import Counter

cache=Path('artifacts/reference-study/cache/levels');cache.mkdir(parents=True,exist_ok=True)
# Read the referenced input into the catalog representation used by the inspection script.
def read(level):
 path=cache/f'level{level}.json'
 try:
  if not path.exists():
   path.write_bytes(urlopen(f'https://sg.gamedistribution.com/solitaire-tripeaks/resource/levels/level{level}.json',timeout=15).read())
  data=json.loads(path.read_text());settings=data['LevelSetting'][0]
  cards=[c for name,rows in data.items() if name.startswith('Card') for c in rows]
  return {'level':level,'settings':settings,'cards':len(cards),'hazards':dict(Counter(c.get('hazard_type','0') for c in cards))}
 except Exception as e:return {'level':level,'error':str(e)}
results=[]
with ThreadPoolExecutor(max_workers=8) as pool:
 for done in as_completed([pool.submit(read,n) for n in range(1,561)]):
  results.append(done.result())
  if len(results)%100==0:print('Studied level files:',len(results),flush=True)
results.sort(key=lambda r:r['level'])
Path('artifacts/reference-study/catalog.json').write_text(json.dumps(results,indent=2))
good=[r for r in results if 'error' not in r]
print('Loaded:',len(good),'Unavailable:',len(results)-len(good))
print('Stage counts:',dict(Counter(r['settings'].get('Stages') for r in good)))
print('Card range:',min(r['cards'] for r in good),max(r['cards'] for r in good))
print('First hazard examples:',{h:next(r['level'] for r in good if h in r['hazards']) for h in sorted({h for r in good for h in r['hazards']})})
print('First multiple-stage levels:',[r['level'] for r in good if int(r['settings'].get('Stages',1))>1][:16])
print('Goal combinations:',dict(Counter((r['settings'].get('WinStreak'),r['settings'].get('WinClear')) for r in good)))
