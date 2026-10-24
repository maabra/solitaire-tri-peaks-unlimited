import {gameIcon} from './game-art.mjs';
// Shared logical stage with the native AdventureMapArt renderer.
export const route=[[330,650],[430,632],[530,650],[630,620],[720,575],[750,510],[670,475],[570,490],[470,470],[390,425],[355,360],[420,310],[520,320],[620,340],[720,310],[820,270],[875,210],[775,190],[670,215],[560,190]];
export function routeMarkup(){return `<svg class="map-trail" viewBox="0 0 1280 800" preserveAspectRatio="none" aria-hidden="true"><path class="trail-shadow" d="${path()}"/><path class="trail-sand" d="${path()}"/><path class="trail-stitches" d="${path()}"/></svg>`}
function path(){let result='M'+route[0].join(',');for(let i=0;i<route.length-1;i++){const a=route[Math.max(0,i-1)],b=route[i],c=route[i+1],d=route[Math.min(route.length-1,i+2)];result+=` C${b[0]+(c[0]-a[0])/6},${b[1]+(c[1]-a[1])/6} ${c[0]-(d[0]-b[0])/6},${c[1]-(d[1]-b[1])/6} ${c.join(',')}`}return result}
export function menuIcon(kind){return gameIcon(kind,60)}
