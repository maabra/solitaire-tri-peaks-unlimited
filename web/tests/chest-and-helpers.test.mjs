// Exercise chest thresholds, helper ownership and reward persistence independently of animations.
import assert from 'node:assert/strict';
import {chestTiers,chestTierFor,chestReward} from '../chest-tiers.mjs';
import {extraWinRewards} from '../win-rewards.mjs';
import {Hand,K} from '../core.mjs';
import {levelPresentation} from '../level-presentation.mjs';
import {pipExpressions} from '../pip-view.mjs';
for(const [i,t]of chestTiers.entries()){assert.equal(chestTierFor(t.stars-1),i);assert.equal(chestTierFor(t.stars),i+1);assert.equal(chestReward(t.stars).hearts,1);assert.ok(chestReward(t.stars).pack<=5);}
assert.equal(chestReward(9),null);assert.equal(chestTierFor(300),6);
assert.deepEqual(extraWinRewards({coins:100,stars:3}),[]);assert.deepEqual(extraWinRewards({coins:100,packs:[2],arrows:3}).map(x=>x.icon),['pack','arrow']);
const d={TemporaryRules:true,Cards:[{Rank:6,Suit:0,Kind:K.Normal},{Rank:7,Suit:0,Kind:K.Normal}],Layout:[{X:0,Row:0},{X:2,Row:0}],Stock:[],Waste:{Rank:5,Suit:0},OpeningEffect:''};
let h=new Hand(d);assert.ok(h.use('extra'));h.tick(10000);assert.equal(h.Stock.length,5);h.draw();h.tick(10000);assert.equal(h.Stock.length,4);assert.ok(h.undo());assert.equal(h.Stock.length,5);
h=new Hand(d);assert.ok(h.use('wild'));h.tick(10000);assert.equal(h.Waste.Rank,0);assert.ok(!h.use('wild'));assert.ok(h.play(1));assert.equal(h.Waste.Rank,7);assert.ok(h.undo());assert.equal(h.Waste.Rank,0);
assert.equal(levelPresentation(25).kind,'bonus');assert.equal(levelPresentation(10).kind,'hard');assert.equal(levelPresentation(20).kind,'expert');assert.equal(levelPresentation(120).kind,'diamonds');assert.equal(pipExpressions.length,16);
console.log('PASS: six chest thresholds, bounded prizes, automatic base rewards, persistent Wild/rescue stock, level identities and 16 Pip expressions.');
