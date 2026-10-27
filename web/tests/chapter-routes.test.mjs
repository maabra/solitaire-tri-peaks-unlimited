// Verify route bounds, tile alignment and gate states across every illustrated chapter.
// Geometry assertions do not claim pixel-level obstacle detection within the paintings.
import assert from 'node:assert/strict';
import {chapterCorridors} from '../generated-content.mjs';
import {roadFor,terrainX,chapterGateState,MAP_WIDTH,MAP_HEIGHT,MEADOW_TILE_HEIGHT} from '../adventure-map.mjs';

assert.equal(chapterCorridors.length,100);
for(let chapter=1;chapter<=100;chapter++){
 const row=chapterCorridors[chapter-1],road=roadFor(chapter);
 assert.equal(row.length,17);
 assert.equal(road.stops.length,100);
 assert.ok(road.gate[1]+90<road.stops.at(-1)[1]-35,'Gate must clear the final stop');
 for(const [x,y] of road.points){
  assert.ok(x>=MAP_WIDTH*.12&&x<=MAP_WIDTH*.88&&y>=300&&y<MAP_HEIGHT);
  assert.ok(Math.abs(x-terrainX(chapter,y))<.01,'Road samples must follow the terrain');
 }
 for(const [x,y] of road.stops)assert.equal(x,terrainX(chapter,y));
 const end=chapter*100;
 assert.equal(chapterGateState({Unlocked:end,Stars:{}},chapter).open,false);
 assert.equal(chapterGateState({Unlocked:Math.min(10000,end+1),Stars:{[end]:1}},chapter).open,true);
 assert.equal(chapterGateState({Unlocked:10000,Stars:{}},chapter).next,chapter===100?null:chapter+1);
}
for(let y=2000;y<9000;y+=37)assert.ok(Math.abs(terrainX(1,y)-terrainX(1,y-MEADOW_TILE_HEIGHT))<1e-8,'Meadow route and tiled painting must repeat together');
console.log('PASS: 100 terrain corridors, centered stops, bounded interpolation, repeating meadow alignment and every locked/open/final chapter gate.');
