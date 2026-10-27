// Fit complete formations at representative viewport sizes so rotated cards remain within their tableau.
import assert from 'node:assert/strict';
import {fitTableau} from '../tableau-viewport.mjs';
import {storyLayoutV3} from '../story-layouts-v3.mjs';
import {motion} from '../motion.mjs';
import {extraWinRewards} from '../win-rewards.mjs';
for(let level=1;level<=256;level++){
 const slots=storyLayoutV3(level);
 for(const [width,height,compact]of [[1050,800,false],[700,600,false],[348,844,true],[278,640,true]]){
  const view=fitTableau(slots,width,height,compact);
  for(const [i,s]of slots.entries()){
   const p=view.positions[i],a=s.Angle*Math.PI/180;
   const rx=(Math.abs(Math.cos(a))*view.cw+Math.abs(Math.sin(a))*view.ch)/2;
   const ry=(Math.abs(Math.sin(a))*view.cw+Math.abs(Math.cos(a))*view.ch)/2;
   const x=p.left+view.cw/2,y=p.top+view.ch/2;
   assert.ok(x-rx>=-1e-8&&x+rx<=width+1e-8,`${level}: horizontal bounds`);
   assert.ok(y-ry>=-1e-8&&y+ry<=view.height+1e-8,`${level}: vertical bounds`);
  }
 }
}
assert.ok(motion.play<200&&motion.flip<200);
// The You win phase now runs twice as long; stock accounting keeps its original timing.
assert.equal(motion.completeWin,840);
assert.ok(motion.completeStock+motion.completeWin<=1200);
assert.deepEqual(extraWinRewards({hearts:3,packs:[1,2],arrows:7}).map(p=>p.amount),[3,2,7]);
console.log('PASS: 256 complete formations fit four stage sizes; fast motion and exact extra-prize quantities.');
