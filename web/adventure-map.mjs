import {gameIcon} from './game-art.mjs';
// Shared web/Windows map geometry. Reserve room for the full stop, stars and shadow.
export const MAP_WIDTH=960,MAP_HEIGHT=9400;
import {chapterCorridors} from './generated-content.mjs';
const cache=new Map();
export const MEADOW_TILE_HEIGHT=MAP_WIDTH*1672/940;
// Use the same bottom-anchored tile coordinates as the meadow background CSS.
export function terrainX(chapter,y){
 const index=((chapter-1)%chapterCorridors.length+chapterCorridors.length)%chapterCorridors.length;
 const t=index===0?((y-MAP_HEIGHT)%MEADOW_TILE_HEIGHT+MEADOW_TILE_HEIGHT)%MEADOW_TILE_HEIGHT/MEADOW_TILE_HEIGHT:Math.max(0,Math.min(1,y/MAP_HEIGHT));
 const row=chapterCorridors[index],position=t*(row.length-1),i=Math.min(row.length-2,Math.floor(position)),f=position-i;
 // Smoothstep stays inside the authored corridor: no Catmull-Rom overshoot.
 const blend=f*f*(3-2*f);return MAP_WIDTH*(row[i]+(row[i+1]-row[i])*blend)/100;
}
export function roadFor(chapter=1){
 if(cache.has(chapter))return cache.get(chapter);
 const centers=Array.from({length:100},(_,i)=>{const y=MAP_HEIGHT-260-i*88;return [terrainX(chapter,y),y]});
 const stops=centers.map(p=>[...p]),gate=[terrainX(chapter,355),300];
 const points=Array.from({length:440},(_,i)=>{const y=MAP_HEIGHT-260-i*20;return [terrainX(chapter,y),y]});
 points.push([gate[0],355]);const road={centers,stops,points,gate};cache.set(chapter,road);if(cache.size>12)cache.delete(cache.keys().next().value);return road;
}
// Gate entry is navigation after a saved chapter win, not another reward transaction.
// Unlocked progress keeps gates open for returning players; the final one leads to Endless.
export function chapterGateState(profile,chapter){
 const end=chapter*100,final=chapter===chapterCorridors.length;
 return {end,final,open:!!profile.Stars[end]||profile.Unlocked>end,next:final?null:chapter+1};
}
export const route=roadFor(1).stops;
export function routeMarkup(chapter=1){const {points,stops}=roadFor(chapter),d=path(points);return `<svg class="map-trail" viewBox="0 0 ${MAP_WIDTH} ${MAP_HEIGHT}" preserveAspectRatio="none" aria-hidden="true"><defs><pattern id="road-stone-${chapter}" width="144" height="144" patternUnits="userSpaceOnUse"><image href="assets/road-stone-border.png" width="144" height="144" preserveAspectRatio="xMidYMid slice"/></pattern></defs><path class="trail-edge-shadow" d="${d}"/><path class="trail-shadow" d="${d}" style="stroke:url(#road-stone-${chapter})"/><g class="stop-landing-shadows">${stops.map(([x,y])=>`<ellipse cx="${x}" cy="${y+11}" rx="47" ry="34"/>`).join('')}</g><path class="trail-sand" d="${d}"/><path class="trail-stitches" d="${d}"/><g class="stop-landings">${stops.map(([x,y],i)=>`<g data-landing="${i}"><ellipse class="stop-landing" cx="${x}" cy="${y+5}" rx="46" ry="35"/><ellipse class="stop-landing-rim" cx="${x}" cy="${y+5}" rx="40" ry="29"/></g>`).join('')}</g></svg>`;}
function path(points){return points.map(([x,y],i)=>`${i?'L':'M'}${x.toFixed(2)},${y}`).join(' ');}
export function menuIcon(kind){return gameIcon(kind,60)}
