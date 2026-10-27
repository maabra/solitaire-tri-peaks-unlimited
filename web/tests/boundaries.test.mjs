import assert from 'node:assert/strict';
import {Session,ProfileStore,validateProfile} from '../session.mjs';
import {helpers} from '../core.mjs';
const MAX=2147483647;
const storage=new Map(),adapter={getItem:/* Read from the test's isolated in-memory storage instead of a real player's browser save. */
 k=>storage.get(k)??null,setItem:/* Write into the test's isolated storage to exercise persistence and conflict handling. */
 (k,v)=>storage.set(k,v)};
const s=new Session(new ProfileStore(adapter),()=> '2026-10-26');
s.p.Coins=MAX;s.daily();assert.equal(s.p.Coins,MAX);s.spinWheel(0);assert.equal(s.p.Coins,MAX);
s.p.Garden=MAX;s.gardenGift();assert.equal(s.p.Coins,MAX);validateProfile(s.p);
s.p.Unlocked=10000;s.p.HighestPlayedLevel=10000;s.p.SeenMechanics=Array.from({length:17},(_,i)=>i);
for(const helper of helpers){s.add(helper.Id,MAX-s.inventory(helper.Id));const before=s.p.Coins;assert.throws(()=>s.buy(helper.Id),/full/);assert.equal(s.p.Coins,before);s.add(helper.Id,2);assert.equal(s.inventory(helper.Id),MAX);}
s.save();assert.equal(new Session(new ProfileStore(adapter)).p.Wilds,MAX);
const a=s.events.a;a.Album.Packs=Array(10000).fill(1);a.Album.Tokens=MAX;assert.equal(s.events.buyPack(1),false);assert.equal(a.Album.Tokens,MAX);
a.Album.Cards=Object.fromEntries(Array.from({length:100},(_,i)=>[i,1000000]));a.Album.Packs=[5];a.Daily.Values.packs=MAX;s.events.openPack();assert.equal(a.Album.Tokens,MAX);assert.ok(Object.values(a.Album.Cards).every(n=>n<=1000000));
a.Hunt.Arrows=MAX;a.Bingo.Tokens=MAX;s.events.reward({arrows:1,bingo:1});s.save();
a.Regatta.Streak=MAX;a.Regatta.Distance=MAX;a.Pass.Wins=MAX;s.side.a.Wins=1000000;
s.start(1,true,12345);s.hand.Score=MAX;s.hand.Streak=MAX;s.hand.BestStreak=MAX;s.p.EndlessWins=MAX;
for(const step of s.hand.Deal.Solution)step.Index<0?(s.tick(s.hand.s.DrawCooldown),s.draw()):s.play(step.Index);
assert.ok(s.hand.Won);assert.equal(s.hand.Score,MAX);assert.equal(s.p.Garden,MAX);assert.equal(s.p.EndlessWins,MAX);validateProfile(s.p);s.save();
assert.equal(new Session(new ProfileStore(adapter)).p.Garden,MAX);
console.log('PASS: maximum wallets, bags, packs, duplicate stickers, activity counters and restored hand scores remain saveable; full bags cannot spend coins.');

a.Hunt.Used=MAX;a.Hunt.Round=MAX;a.Hunt.Arrows=2;a.Hunt.Open=Array.from({length:80},(_,i)=>i);s.events.shoot(80);assert.equal(a.Hunt.Round,MAX);assert.equal(a.Hunt.Used,MAX);
a.Bingo.Round=MAX;a.Bingo.Tokens=1;a.Bingo.Drawn=Array.from({length:24},(_,i)=>i);s.events.bingoDraw();assert.equal(a.Bingo.Round,MAX);
a.Album.Complete=true;a.Album.Index=MAX;assert.equal(s.events.nextAlbum(),false);validateProfile(s.p);s.save();
