// Check semantic resource labels and quantities without relying on a browser screenshot.
import assert from 'node:assert/strict';
import {resourceTextParts,resourceBadge,swapHeartProgress} from '../resource-view.mjs';
assert.deepEqual(resourceTextParts('90 coins grown ? +1 heart').filter(p=>typeof p==='object'),[
 {value:'90',icon:'coins',label:'90 coins grown'}, {value:'+1',icon:'heart',label:'+1 heart'}]);
assert.equal(resourceTextParts('5/10 swaps')[0].value,'5/10');
assert.equal(resourceTextParts('1,087 coins')[0].value,'1,087');
assert.equal(resourceTextParts('20 trophies')[0].icon,'trophy');
for(const text of ['Level 90','00:59:58','Version 2.4.1','Match 3 or more','No hearts left'])assert.deepEqual(resourceTextParts(text),[text]);
assert.ok(resourceBadge(90,'coins','90 coins').includes('aria-label="90 coins"'));
assert.ok(swapHeartProgress(5).includes('5/10'));
console.log('PASS: resource quantities, signed rewards, fractions, accessible labels and non-resource text.');
