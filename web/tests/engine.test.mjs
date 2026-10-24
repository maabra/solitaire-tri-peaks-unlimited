import {readFileSync} from 'node:fs';
import assert from 'node:assert/strict';
import {generate,Hand,GemGame,K} from '../core.mjs';
import {Session,ProfileStore,defaults,validateProfile,encodeDeal,parseDeal} from '../session.mjs';
const fixtures=JSON.parse(readFileSync(new URL('../../shared/engine-fixtures.json',import.meta.url)));
const geometry=x=>JSON.parse(JSON.stringify(x,(k,v)=>["DrawX","DrawY","Angle"].includes(k)&&typeof v==="number"?Math.fround(v):v));
for(let f of [...fixtures.Deals,...fixtures.StoryDeals]){let actual=generate(f.Seed,f.Level,f.Deal.Generation);assert.deepEqual(geometry(actual),geometry(f.Deal),`Windows/browser deal parity: ${f.Level}`);let h=new Hand(actual);for(let s of actual.Solution)assert.ok(s.Index<0?(h.tick(h.s.DrawCooldown),h.draw()):h.play(s.Index),`Legal solution at ${f.Level}: ${s.Index}`);assert.ok(h.Won);let run=h.capture(f.Seed,f.Level,false);assert.deepEqual(new Hand(actual,run).capture(f.Seed,f.Level,false),run)}
for(let f of fixtures.Gems){let g=new GemGame(f.Seed);for(let frame of f.Frames){assert.ok(g.play(...g.hint()));assert.deepEqual({Cells:g.Cells,Earned:g.Earned,Moves:g.Moves},frame,'Native gem-game parity')}}
let storage=new Map(),adapter={getItem:k=>storage.get(k)??null,setItem:(k,v)=>storage.set(k,v)},store=new ProfileStore(adapter),session=new Session(store,()=> '2026-10-24');session.start(1);for(let step of session.hand.Deal.Solution)step.Index<0?(session.tick(session.hand.s.DrawCooldown),session.draw()):session.play(step.Index);assert.ok(session.hand.Won);assert.equal(session.p.Unlocked,2);let coins=session.p.Coins;session.complete();assert.equal(session.p.Coins,coins,'Reward only once');assert.equal(new Session(store).p.Unlocked,2);assert.equal(session.daily(),75);assert.equal(session.daily(),0);session.remix();assert.equal(session.p.FreshDealCooldown,30);assert.equal(session.remix(),false);session.tick(31);assert.equal(session.p.FreshDealCooldown,0);
let d={TemporaryRules:true,Cards:[{Rank:6,Suit:0,Kind:K.Normal},{Rank:7,Suit:0,Kind:K.Normal}],Layout:[{X:0,Row:0},{X:2,Row:0}],Stock:[],Waste:{Rank:5,Suit:0},OpeningEffect:''},h=new Hand(d);assert.ok(h.use('extra'));assert.equal(h.Stock.length,5);h.draw();h.tick(11);assert.equal(h.Stock.length,0);h.undo();assert.equal(h.Stock.length,0,'Undo cannot revive expired rescue cards');
for(let endless of [false,true]){let d={Level:1,Seed:105726,Endless:endless},code=await encodeDeal(d);assert.deepEqual(await parseDeal(code),d);assert.equal(await parseDeal(code.slice(0,-1)+'Z'),null)}
assert.throws(()=>validateProfile({...defaults(),Coins:-1}));assert.throws(()=>validateProfile({...defaults(),FreshDealCooldown:Infinity}));
console.log(`PASS: ${fixtures.Deals.length+fixtures.StoryDeals.length} Windows/browser deal fixtures, 60 native gem turns, legal winning paths, saves, rewards, timers and deal codes.`);

let cooldown=new Hand(d);cooldown.Stock=[{Rank:8,Suit:0},{Rank:9,Suit:0}];assert.ok(cooldown.draw());assert.equal(cooldown.draw(),false);cooldown.undo();assert.equal(cooldown.draw(),false);cooldown.tick(2);assert.ok(cooldown.draw());

let time=1000000000000,gardenStore=new ProfileStore(adapter),garden=new Session(gardenStore,()=>"2026-10-24",()=>time);garden.openGems();let pair=garden.gems.hint();garden.playGems(...pair);assert.equal(garden.openGems(true),false);time+=3599999;assert.equal(garden.openGems(true),false);let reopened=new Session(gardenStore,()=>"2026-10-24",()=>time);assert.deepEqual(reopened.gems.Cells,garden.gems.Cells);assert.equal(reopened.gems.Moves,19);assert.equal(reopened.openGems(true),false);time++;assert.equal(reopened.openGems(true),true);assert.equal(reopened.gardenRefreshRemaining,3600);

let discovery=new Session(new ProfileStore(null));discovery.start(1);assert.equal(discovery.helperDiscovered("thaw"),false);assert.equal(discovery.helperDiscovered("scout"),false);discovery.p.Unlocked=100;discovery.start(100);assert.equal(discovery.helperDiscovered("thaw"),true);assert.equal(discovery.helperRelevant("thaw"),true);discovery.start(1);assert.equal(discovery.helperRelevant("thaw"),false);

for(let index=0;index<8;index++){let memory=new Map(),storage={getItem:k=>memory.get(k)??null,setItem:(k,v)=>memory.set(k,v)},s=new Session(new ProfileStore(storage));assert.equal(s.wheelSpins,1);let prize=s.spinWheel(index);assert.equal(prize.index,index);assert.equal(s.p.Coins,300+(prize.coins??0));if(prize.item)assert.equal(s.inventory(prize.item),prize.item==='remove'?2:3);assert.equal(s.spinWheel(index),null);let restored=new Session(new ProfileStore(storage));assert.equal(restored.wheelSpins,0);assert.equal(restored.spinWheel(index),null);restored.p.Garden=4;assert.equal(restored.wheelSpins,0);restored.p.Garden=5;assert.equal(restored.wheelSpins,1);assert.ok(restored.spinWheel(index));assert.equal(restored.wheelSpins,0)}
assert.throws(()=>validateProfile({...defaults(),WheelSpinsUsed:2}));
console.log('PASS: all eight wheel prizes, earned-spin thresholds and persisted consumption.');

let conflictMemory=new Map(),conflictAdapter={getItem:k=>conflictMemory.get(k)??null,setItem:(k,v)=>conflictMemory.set(k,v)};
let writer=new Session(new ProfileStore(conflictAdapter),()=>"2026-10-24"),stale=new Session(new ProfileStore(conflictAdapter),()=>"2026-10-24");
assert.equal(writer.daily(),75);assert.throws(()=>stale.daily(),/another window/);assert.equal(stale.store.conflicted,true);assert.equal(new Session(new ProfileStore(conflictAdapter)).p.Coins,375);
for(const Generation of ['MP18','MP19'])for(const Endless of [false,true]){let deal={Level:17,Seed:121678,Endless,Generation};assert.deepEqual(await parseDeal(await encodeDeal(deal)),deal)}
console.log('PASS: stale-tab saves cannot overwrite newer rewards; old and new deal-code versions round trip.');

let corruptMemory=new Map(),corruptAdapter={getItem:k=>corruptMemory.get(k)??null,setItem:(k,v)=>corruptMemory.set(k,v)},corrupt={...defaults(),Coins:777,Unlocked:100,Active:{Level:100,Removed:[]}};
corruptMemory.set('meadow-peaks-profile',JSON.stringify(corrupt));let recovered=new Session(new ProfileStore(corruptAdapter));assert.equal(recovered.p.Coins,777);assert.equal(recovered.p.Unlocked,100);assert.equal(recovered.hand,null);assert.ok(recovered.store.warning);
let invalidGarden={...defaults(),Coins:789,ActiveGarden:{Seed:42,Swaps:[{First:0,Second:63}]}};corruptMemory.set('meadow-peaks-profile',JSON.stringify(invalidGarden));let safeGarden=new Session(new ProfileStore(corruptAdapter));assert.equal(safeGarden.p.Coins,789);assert.equal(safeGarden.gems,null);assert.ok(safeGarden.store.warning);
const beforeInvalid=corruptMemory.get('meadow-peaks-profile');assert.throws(()=>safeGarden.store.save({...defaults(),Coins:NaN}));assert.equal(corruptMemory.get('meadow-peaks-profile'),beforeInvalid);
console.log('PASS: damaged hands and impossible saved gem swaps preserve the wallet; invalid saves never replace the existing save.');

let html=readFileSync(new URL('../index.html',import.meta.url),'utf8'),project=readFileSync(new URL('../../src/MeadowPeaks.csproj',import.meta.url),'utf8'),version=project.match(/<Version>([^<]+)<\/Version>/)[1];assert.ok(html.includes('name="meadow-build" content="'+version+'"'),'Desktop and web version stamps match');
console.log('PASS: Windows and web build versions match.');
