import assert from 'node:assert/strict';
import {storyLayoutV4,storyNameV4,formationFamiliesV4} from '../story-layouts-v4.mjs';
import {adventureDeal,AdventureHand} from '../adventure-hand.mjs';
import {Session,ProfileStore,encodeDeal,parseDeal,validateProfile} from '../session.mjs';
import {RankShuffle} from '../rank-shuffle.mjs';
import {goalProgress} from '../hand-progress.mjs';
import {fitTableau} from '../tableau-viewport.mjs';

assert.equal(formationFamiliesV4,128);
assert.deepEqual([1,2,3,4].map(l=>storyLayoutV4(l).length),[13,14,18,20]);
const shapes=new Set();
for(let level=5;level<133;level++){
 const slots=storyLayoutV4(level);shapes.add(JSON.stringify(slots));
 assert.ok(slots.length>=20&&slots.length<=40,level);
 for(const [i,slot]of slots.entries())assert.ok(slot.BlockedBy.every(j=>j>i&&j<slots.length));
 for(const width of [278,700,1050]){const fit=fitTableau(slots,width,800,width<700);assert.ok(fit.cw>0&&fit.height>0);}
}
assert.equal(shapes.size,128);
for(const level of [1,2,3,4,5,25,40,60,75,100,300,1200,2500,5000,8000,3650,5650,7000,8600,9600,10000,1095,2595,6095,9095,9995]){
 const deal=adventureDeal(104729+level,level,false,'MP26'),hand=new AdventureHand(deal);
 assert.equal(deal.Formation,storyNameV4(level));
 for(const step of deal.Solution){if(step.Index<0){hand.tick(2);assert.ok(hand.draw());}else assert.ok(hand.play(step.Index));}
 assert.ok(hand.Won,level);assert.equal(goalProgress(hand).cleared,goalProgress(hand).total);
 const code={Generation:'MP26',Level:level,Seed:104729+level,Endless:false};assert.deepEqual(await parseDeal(await encodeDeal(code)),code);
}
const session=new Session(new ProfileStore(null));session.p.Unlocked=1200;session.p.Hearts=999;session.start(1200,false,12345);
assert.ok(session.helperDiscovered('shuffle'));session.add('shuffle',2);
const hand=session.hand,identities=hand.Deal.Cards.map(c=>[c.Kind,c.Pack,c.Trophy,c.Arrow,c.Goal]);
const before=session.inventory('shuffle');assert.ok(session.use('shuffle'));
assert.equal(session.inventory('shuffle'),before-1);assert.ok(hand.Moves.length);
assert.deepEqual(hand.Deal.Cards.map(c=>[c.Kind,c.Pack,c.Trophy,c.Arrow,c.Goal]),identities);
assert.equal(hand.history.length,0);assert.ok(!session.use('shuffle'));assert.equal(session.inventory('shuffle'),before-1);
session.save();assert.ok(validateProfile(session.p));const restored=new AdventureHand(session.p.Active.DealtBoard,session.p.Active);
assert.deepEqual(restored.s.Faces,hand.s.Faces);assert.deepEqual(restored.Moves,hand.Moves);
session.tick(6);session.hand.Removed.fill(true);assert.ok(!RankShuffle.available(session.hand));
const goalHand=new AdventureHand(adventureDeal(10000,40,false,'MP26'));
assert.ok(goalProgress(goalHand).total<goalHand.Deal.Cards.length);
console.log('PASS: 128 formations, short onboarding, MP26 witnesses/codes, shuffle identity preservation, cooldown, save recovery and objective-specific counts.');

for(const [level,count]of [[1095,60],[2595,80],[6095,104],[9095,120]]){const session=new Session(new ProfileStore(null));session.p.Unlocked=level;session.start(level,false,12345);assert.equal(session.hand.Deal.Cards.length,count);assert.ok(validateProfile(session.p));}
