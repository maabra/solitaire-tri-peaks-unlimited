// Verify that every retained chapter has its original linked art assets before packaging.
// This catches missing files that pure engine tests cannot detect.
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
import {campaign} from '../web/generated-content.mjs';
import {TOTAL} from '../web/core.mjs';
import {chapterTitles} from '../web/art-catalog.mjs';
import {CHAPTER_SET_COUNT,collectionIndex} from '../web/chapter-collection.mjs';
assert.equal(CHAPTER_SET_COUNT,100);assert.equal(TOTAL,CHAPTER_SET_COUNT*100);assert.equal(campaign.chapterTitles.length,CHAPTER_SET_COUNT);assert.equal(new Set(chapterTitles).size,100);assert.equal(collectionIndex(101),0);
const assets=['map-meadow-720p.png','road-stone-border.png','level-notice-board.png'];for(let n=0;n<17;n++)for(const kind of ['portrait-worlds','portrait-extra'])assets.push(`${kind}-${String(n).padStart(2,'0')}.png`);for(let n=0;n<4;n++)for(const kind of ['chapter-backs','chapter-tables'])assets.push(`${kind}-${n}.png`);
for(const name of assets){const data=readFileSync(new URL('../web/assets/'+name,import.meta.url));assert.equal(data.subarray(1,4).toString(),'PNG',name);assert.ok(data.readUInt32BE(16)>=500&&data.readUInt32BE(20)>=500,name);}
console.log('PASS: 100 uniquely named linked chapter sets and all required original image sheets exist.');
