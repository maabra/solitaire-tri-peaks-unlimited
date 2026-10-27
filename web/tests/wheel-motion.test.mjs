import assert from 'node:assert/strict';
import {wheelTargetAngle} from '../wheel-motion.mjs';

// Every clockwise prize center must land at twelve o'clock after rotating the disc.
let previous=0;
for(let round=0;round<12;round++)for(let index=0;index<8;index++){
 const angle=wheelTargetAngle(index,previous);
 assert.equal((angle+index*45)%360,0,`Prize ${index} misses the fixed pointer`);
 assert.ok(angle-previous>=1800&&angle-previous<2520,'Each spin needs five complete turns before landing');
 previous=angle;
}
for(const index of [-1,8,1.5,NaN])assert.throws(()=>wheelTargetAngle(index));
console.log('PASS: every prize and repeated spin lands below the fixed pointer after at least five turns.');
