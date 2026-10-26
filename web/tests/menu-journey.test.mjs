import {test} from 'node:test';
import assert from 'node:assert/strict';
import {MenuJourney} from '../menu-journey.mjs';
import {activityGroups,activityGroup} from '../activity-groups.mjs';
test('returning to a place collapses loops and preserves its scroll',()=>{
 const j=new MenuJourney(()=>true);
 j.visit('events',false);j.visit('ev:album',true,20);j.visit('ev:trail',true,140);
 j.visit('ev:album',true,45);
 assert.deepEqual(j.routes,[{action:'events',scroll:20},{action:'ev:album',scroll:140}]);
 assert.equal(j.back().action,'events');assert.equal(j.back(),null);
});
test('repainting current route and ignoring actions do not create history',()=>{
 const j=new MenuJourney(a=>a.startsWith('ev:'));
 j.visit('ev:trail',false);j.visit('ev:trail',true);j.visit('roll',true);
 assert.equal(j.routes.length,1);j.visit('ev:album',false);assert.equal(j.routes.length,1);
});
test('each activity has one parent and album submenus inherit collections',()=>{
 const ids=activityGroups.flatMap(g=>g[2]);assert.equal(ids.length,new Set(ids).size);
 assert.equal(activityGroup('pack'),activityGroup('album'));
 assert.equal(activityGroup('tickets')[0],'Play & explore');
});
