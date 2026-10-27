import {storyLayout,storyName} from './story-layouts.mjs';
const names=['Five islands','Six islands','Seven islands','Eight islands','Clockwise spiral','Petal spiral','Open spiral','Tilted spiral','Diamond packets','Five bouquets','Six bouquets','Seven bouquets','Double river','Crossing ribbons','Rolling ribbons','Twin serpents'];
// Name the historical MP19 formation family without altering older level identities.
export function storyNameV2(level){const family=(level-1)%32;return family<16?storyName(level):`${names[family-16]} ${Math.floor((level-1)/32)+1}`}
// Build historical island and tilted-packet layouts used by compatible saved generations.
export function storyLayoutV2(level){const family=(level-1)%32,variation=Math.floor((level-1)/32);if(family<16)return storyLayout(level);const bend=((variation*48271+12345)%32749)/32749*24-12,p=[],/* Append an authored point or option to the current local collection. */
 add=(DrawX,DrawY,Angle)=>p.push({DrawX,DrawY,Angle});
if(family<20){const n=family-11;for(let i=0;i<n;i++)for(let j=0;j<4;j++){let tilt=(i%2===0?-1:1)*(14+(family-16)*7)+bend*.3;add(85+i*830/(n-1)+j*3,105+(i%2)*55+j*24+bend*Math.sin(i),tilt)}}
else if(family<24){for(let i=0;i<28;i++){let t=i/27,a=t*Math.PI*(2.1+(family-20)*.18)-Math.PI/2,rx=65+t*(250+bend),ry=25+t*80;add(500+Math.cos(a)*rx,155+Math.sin(a)*ry,a*180/Math.PI+90+(family-20)*9)}}
else if(family<28){const n=family-20,depth=n===4?6:n===5?5:4;for(let i=0;i<n;i++)for(let j=0;j<depth;j++)add(90+i*820/(n-1)+(j%2===0?-5:5),105+Math.sin(i*1.7)*45+j*20+bend,(i%2===0?1:-1)*(30+j*2)+(family-24)*3)}
else{for(let lane=0;lane<2;lane++)for(let i=0;i<14;i++){let t=i/13,w=t*Math.PI*(1.5+(family-28)*.4);add(85+t*830,90+lane*130+Math.sin(w+lane*.8)*(28+bend*.3),Math.cos(w)*(18+(family-28)*5))}}
// Test authored card footprints when constructing the explicit covering graph.
const overlap=(a,b)=>{const q=p[a],r=p[b],pa=q.Angle*Math.PI/180,qa=r.Angle*Math.PI/180;return [pa,pa+Math.PI/2,qa,qa+Math.PI/2].every(axis=>Math.abs((r.DrawX-q.DrawX)*Math.cos(axis)+(r.DrawY-q.DrawY)*Math.sin(axis))<35*(Math.abs(Math.cos(pa-axis))+Math.abs(Math.cos(qa-axis)))+49*(Math.abs(Math.sin(pa-axis))+Math.abs(Math.sin(qa-axis)))-3)};
return p.map((s,i)=>({X:i%10,Row:Math.floor(i/10),DrawX:Math.fround(s.DrawX),DrawY:Math.fround(s.DrawY),Angle:Math.fround(s.Angle),BlockedBy:p.flatMap((_,j)=>j>i&&overlap(i,j)?[j]:[])}))}
