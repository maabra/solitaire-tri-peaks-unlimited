import assert from 'node:assert/strict';
import {Session,ProfileStore,validateProfile,encodeDeal,parseDeal} from '../session.mjs';
import {AdventureHand,adventureDeal} from '../adventure-hand.mjs';
import {clone,K} from '../core.mjs';
import {rarity} from '../adventure-systems.mjs';
// Create a minimal card fixture with optional mechanic fields for targeted rule checks.
const card=(Rank,Kind=0,extra={})=>({Rank,Suit:0,Kind,Strength:1,TemporaryStock:false,...extra});
// Build a small fully exposed fixture so the test isolates mechanics rather than cover geometry.
const board=cards=>({Cards:cards,Layout:cards.map((c,i)=>({X:i,Row:0,BlockedBy:[]})),Waste:card(5),Stock:[card(7),card(8)],TemporaryRules:true,AdventureRules:true});
let now=Date.UTC(2026,9,24),memory=new Map(),adapter={getItem:/* Read from the test's isolated in-memory storage instead of a real player's browser save. */
 k=>memory.get(k)??null,setItem:/* Write into the test's isolated storage to exercise persistence and conflict handling. */
 (k,v)=>memory.set(k,v)},s=new Session(new ProfileStore(adapter),undefined,()=>now),e=s.events;
// Album: rarity guarantee, exact duplicate conversion, idempotent completion rewards.
for(let tier=1;tier<=5;tier++){e.a.Album.Packs=[tier];const result=e.openPack();assert.equal(result.length,3);assert.equal(result[0].rarity,tier);assert.ok(result.every(r=>r.rarity<=tier));}
e.a.Album.Cards=Object.fromEntries(Array.from({length:100},(_,i)=>[i,1]));e.a.Album.Packs=[5];let tokens=e.a.Album.Tokens;const results=e.openPack();assert.equal(e.a.Album.Tokens-tokens,results.reduce((n,r)=>n+r.rarity*5,0));
for(let i=0;i<10;i++){assert.ok(e.claimCategory(i));assert.equal(e.claimCategory(i),false);}assert.equal(e.a.Album.Complete,true);const coins=s.p.Coins;assert.equal(e.claimCategory(0),false);assert.equal(s.p.Coins,coins);assert.ok(e.nextAlbum());assert.equal(Object.keys(e.a.Album.Cards).length,0);
assert.equal(e.buyPack(0),false);e.a.Album.Tokens=240;assert.ok(e.buyPack(5));assert.equal(e.a.Album.Tokens,0);assert.equal(e.buyPack(5),false);
// Daily rolling 24h, only 3 visible, at most 6 claims.
for(const key of ['wins','clears','draws','spent','stars','packs'])e.record(key,1000);
let claimed=0;while(e.missions.length){assert.ok(e.missions.length<=3);const id=e.missions[0].id;assert.ok(e.claimMission(id));assert.equal(e.claimMission(id),false);claimed++;}assert.equal(claimed,6);now+=86400000-1;e.refresh();assert.equal(e.missions.length,0);now++;e.refresh();assert.equal(e.missions.length,3);
// Monthly incremental costs: 5, 6, 7, ... wins. Claims cannot repeat.
assert.deepEqual([0,1,2,3].map(i=>e.passTarget(i)),[5,11,18,26]);e.a.Pass.Wins=11;assert.ok(e.claimPass(0));assert.ok(e.claimPass(1));assert.equal(e.claimPass(1),false);assert.equal(e.claimPass(2),false);now=Date.UTC(2026,10,1);e.refresh();assert.equal(e.a.Pass.Wins,0);assert.equal(e.a.Pass.Claimed.length,0);
// Offline regatta: new wins, resets, and independent threshold rewards.
for(let i=0;i<5;i++)e.onWin(2,true);assert.equal(e.a.Regatta.Distance,86);e.onWin(3,false);assert.equal(e.a.Regatta.Distance,86);e.onAbandon();assert.equal(e.a.Regatta.Streak,0);assert.ok(e.claimRegatta(0));assert.equal(e.claimRegatta(0),false);assert.equal(e.crews.filter(c=>c.you).length,1);
// Three-win balloon run banks arrows; no second entry in the same week.
assert.ok(e.startBalloon());e.arrow(3);e.onWin(1);e.arrow(2);e.onWin(1);e.arrow(1);e.onWin(1);assert.equal(e.a.Balloon.Active,false);assert.equal(e.a.Hunt.Arrows,9);assert.equal(e.startBalloon(),false);
e.a.Hunt.Arrows=100;for(let i=0;i<81;i++){assert.ok(e.shoot(i));if(i<80)assert.equal(e.shoot(i),null);}assert.equal(e.a.Hunt.Round,1);assert.deepEqual(e.a.Hunt.Open,[]);assert.equal(e.shoot(-1),null);assert.equal(e.shoot(81),null);
e.a.Bingo.Tokens=25;let drawn=new Set(),lines=0;for(let i=0;i<25;i++){const r=e.bingoDraw();assert.ok(!drawn.has(r.cell));drawn.add(r.cell);lines+=r.won;}assert.equal(lines,12);assert.equal(e.a.Bingo.Round,1);assert.equal(e.bingoDraw(),null);
for(let i=1;i<=100;i++)s.p.Stars[i]=3;assert.equal(e.chapterStars(1),300);assert.equal(e.chapterStars(2),0);assert.ok(e.claimChest(1));assert.equal(e.claimChest(1),false);assert.ok(e.claimPostcard(1));assert.equal(e.claimPostcard(1),false);
assert.ok(e.timedSpin());assert.equal(e.timedSpin(),null);s.save();let restored=new Session(new ProfileStore(adapter),undefined,()=>now);assert.deepEqual(restored.events.a,e.a);assert.ok(validateProfile(restored.p));
// Ice needs matching interaction twice, and undo restores thaw state.
let h=new AdventureHand(board([card(6,K.Ice),card(9)]));assert.ok(h.play(0));assert.equal(h.Removed[0],false);assert.equal(h.Waste.Rank,5);assert.ok(h.s.IceThawed.includes(0));assert.ok(h.undo());assert.deepEqual(h.s.IceThawed,[]);assert.ok(h.play(0));assert.ok(h.play(0));assert.equal(h.Removed[0],true);
// TNT clears only currently exposed cards, never the newly revealed layer.
let d=board([card(10),card(2),card(12)]);d.Layout[0].BlockedBy=[1];h=new AdventureHand(d);assert.ok(h.use('tnt'));assert.deepEqual(h.Removed,[false,true,true]);assert.equal(h.use('tnt'),true);assert.equal(h.Won,true);
h=new AdventureHand(board([card(12,K.Lock),card(9)]));assert.ok(h.use('joker',0));assert.equal(h.Waste.Rank,12);assert.equal(h.Removed[0],true);assert.equal(h.use('joker',-1),false);
h=new AdventureHand(board([card(6,K.Up,{Motion:1}),card(9)]));h.draw();assert.equal(h.face(0).Rank,7);h.tick(2);assert.ok(h.use('peek'));assert.equal(h.face(0).Rank,8);assert.ok(h.undo()===false);
h=new AdventureHand(board([card(6,16),card(9)]));assert.equal(h.can(0),false);assert.ok(h.draw());assert.equal(h.s.Turns%2,1);h.Waste=card(5);assert.ok(h.play(0));
h=new AdventureHand(board([card(6,15),card(9),card(11)]));assert.ok(h.play(0));assert.ok(h.Moves.length>0);assert.ok(Object.keys(h.s.Faces).length>=2);
h=new AdventureHand(board([card(6),card(7),card(8),card(9),card(10),card(11)]));let stock=h.Stock.length;for(let i=0;i<5;i++)assert.ok(h.play(i));assert.equal(h.Stock.length,stock+2);
// Five-action trophy expiry, undo-safe pending rewards, and persistence of live faces.
h=new AdventureHand(board([card(6,0,{Pack:3,Trophy:50}),card(9)]));assert.ok(h.play(0));assert.deepEqual(h.s.Earned.Packs,[3]);assert.equal(h.s.Earned.Trophies,50);h.undo();assert.deepEqual(h.s.Earned.Packs,[]);
h=new AdventureHand(board([card(6,0,{Trophy:50}),card(9)]));h.s.Turns=5;assert.ok(h.play(0));assert.equal(h.s.Earned.Trophies,0);
for(const level of [1,15,25,40,60,75,100,400,1100,1400,480000]){const deal=adventureDeal(105726,level);h=new AdventureHand(deal);const run=h.capture(105726,level,false),copy=new AdventureHand(clone(deal),run);assert.deepEqual(copy.capture(105726,level,false),run);}
const reference={Level:1400,Seed:105726,Endless:false,Generation:'MP21'};assert.deepEqual(await parseDeal(await encodeDeal(reference)),reference);
console.log('PASS: album rarity/duplicates, 24h missions, monthly costs, regatta, balloon arrows, all hunt tiles, bingo lines, 100-level chapter rewards, timed wheel, persistence, ice/TNT/joker/motion/fog/tornado/streak and undo-safe rewards.');

// Malformed event imports recover the wallet; generated failure leaves the hand intact.
let safe=new Session(new ProfileStore(null));safe.start(1);const oldHand=safe.hand;assert.throws(()=>safe.start(1,false,-1));assert.equal(safe.hand,oldHand);
let damaged=clone(restored.p);damaged.Coins=987;damaged.Adventure.Hunt.Open=[1,1];memory.set('meadow-peaks-profile',JSON.stringify(damaged));let recovered=new Session(new ProfileStore(adapter),undefined,()=>now);assert.equal(recovered.p.Coins,987);assert.deepEqual(recovered.events.a.Hunt.Open,[]);
console.log('PASS: malformed event recovery retains the wallet; invalid starts preserve the existing hand.');

// Destructive helpers collect keys/gifts instead of making lower locks impossible.
for(const helper of ['tnt','joker','remove']){let d=board([card(9,K.Lock),card(2,K.Key),card(12,K.Gift)]);d.Layout[0].BlockedBy=[1];const h=new AdventureHand(d);assert.ok(h.use(helper,1));assert.equal(h.s.Keys,1);assert.equal(h.s.HasKey,true);assert.ok(h.allows(h.face(0)));}
console.log('PASS: TNT, Joker and Remove preserve key/lock progression.');
