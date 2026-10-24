import {gameIcon} from './game-art.mjs';
// Shared logical stage with the native AdventureMapArt renderer.
export const MAP_WIDTH=960,MAP_HEIGHT=6200;
const cache=new Map();
export function roadFor(chapter=1){if(cache.has(chapter))return cache.get(chapter);const phase=(chapter-1)%5*.18,centers=Array.from({length:100},(_,i)=>[480+225*Math.sin(i*.46+phase)+25*Math.sin(i*.14),MAP_HEIGHT-260-i*56]);const stops=centers.map(([x,y],i)=>[x+(i%2?100:-100),y]);const road={centers,stops};cache.set(chapter,road);if(cache.size>12)cache.delete(cache.keys().next().value);return road;}
export const route=roadFor(1).stops;
export function routeMarkup(chapter=1){const {centers,stops}=roadFor(chapter),d=path(centers);return `<svg class="map-trail" viewBox="0 0 ${MAP_WIDTH} ${MAP_HEIGHT}" preserveAspectRatio="none" aria-hidden="true">${centers.map((p,i)=>`<path class="trail-spur" d="M${p.join(',')} L${stops[i].join(',')}"/>`).join('')}<path class="trail-shadow" d="${d}"/><path class="trail-sand" d="${d}"/><path class="trail-stitches" d="${d}"/></svg>`;}
function path(points){let result='M'+points[0].join(',');for(let i=0;i<points.length-1;i++){const a=points[Math.max(0,i-1)],b=points[i],c=points[i+1],d=points[Math.min(points.length-1,i+2)];result+=` C${b[0]+(c[0]-a[0])/6},${b[1]+(c[1]-a[1])/6} ${c[0]-(d[0]-b[0])/6},${c[1]-(d[1]-b[1])/6} ${c.join(',')}`;}return result;}
export function menuIcon(kind){return gameIcon(kind,60)}
