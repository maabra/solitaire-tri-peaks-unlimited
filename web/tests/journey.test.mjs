import assert from 'node:assert/strict';
import {Session,ProfileStore,defaults,validateProfile,parseDeal,encodeDeal} from '../session.mjs';
import {storyLayoutV3,storyNameV3} from '../story-layouts-v3.mjs';
import {featureUnlocked,journeyOptions} from '../journey-guide.mjs';
import {chapterTitles} from '../art-catalog.mjs';
import {initializeWardrobe,validateWardrobe} from '../customization.mjs';
import {roadFor,MAP_WIDTH,MAP_HEIGHT} from '../adventure-map.mjs';
const hashes=new Set();for(let l=5;l<69;l++){const slots=storyLayoutV3(l);assert.ok(slots.length>=24&&slots.length<=40);assert.ok(slots.every((s,i)=>Number.isFinite(s.DrawX)&&s.DrawX>0&&s.DrawX<1000&&s.DrawY>-20&&s.DrawY<400&&s.BlockedBy.every(j=>j>i&&j<slots.length)));hashes.add(JSON.stringify(slots));}assert.equal(hashes.size,64);
assert.equal(new Set(chapterTitles).size,51);for(const chapter of [1,2,51,4800]){const road=roadFor(chapter);assert.equal(road.stops.length,100);for(let i=0;i<100;i++){const [x,y]=road.stops[i];assert.ok(x>0&&x<MAP_WIDTH&&y>0&&y<MAP_HEIGHT);assert.equal(y,road.centers[i][1]);assert.equal(Math.sign(x-road.centers[i][0]),i%2?1:-1);if(i)assert.ok(y<road.stops[i-1][1]);}}
let s=new Session(new ProfileStore(null));s.start(1);assert.equal(s.hand.Deal.Generation,'MP22');assert.ok(!featureUnlocked(s.p,'album'));s.p.Unlocked=15;assert.ok(featureUnlocked(s.p,'album'));assert.ok(journeyOptions(s).some(x=>x.id==='album'&&x.badge>0));
let p=defaults();p.Unlocked=101;for(let i=1;i<=100;i++)p.Stars[i]=3;const wardrobe=initializeWardrobe(p);assert.deepEqual(wardrobe.Unlocked,[0,1]);assert.ok(featureUnlocked(p,'customize'));assert.ok(validateProfile(p));assert.throws(()=>validateWardrobe({...wardrobe,Back:2}));
s=new Session(new ProfileStore(null));s.p.Unlocked=100;for(let i=1;i<100;i++)s.p.Stars[i]=1;s.start(100);for(const step of s.hand.Deal.Solution){if(step.Index<0){s.hand.tick(2);assert.ok(s.draw());}else assert.ok(s.play(step.Index));}assert.ok(s.hand.Won);assert.ok(s.p.Customization.Unlocked.includes(1));assert.equal(s.p.Customization.Unlocked.filter(n=>n===1).length,1);assert.ok(s.lastReward.chapterGift);
for(const Generation of ['MP21','MP22']){const d={Level:100,Seed:204429,Endless:false,Generation};assert.deepEqual(await parseDeal(await encodeDeal(d)),d);}
console.log('PASS: 64 formation families, 51 original chapter names, alternating road stops, progressive destinations, retrospective skins, chapter unlock rewards and MP21/MP22 codes.');
