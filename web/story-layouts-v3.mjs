import {storyLayoutV2,storyNameV2} from './story-layouts-v2.mjs';
const names=['Open circle','Twin rings','Infinity weave','Clover loops','Square courtyard','Nested boxes','Diamond windows','Triple arches','Moon arches','Sunburst','Flower wreath','Butterfly wings','Braided ribbons','Woven basket','Garden lattice','Stepping terraces','Eight alcoves','Six fans','Seven lanterns','Spiral staircase','Double spiral','Orbiting moons','Heart garden','Crown bridge','Crossed arches','Honeycomb','Star frame','Ripple pond','Hourglass','Castle windows','Linked diamonds','Zigzag promenade'];
export const formationFamilies=64;
// Name the historical 64-family formation selected by the level identifier.
export function storyNameV3(level){if(level<5)return storyNameV2(level);const family=((level-5)*17)%64;return `${family>=32?'Interwoven ':''}${names[family%32]}`;}
// Build the MP22/MP24 formation and explicit cover graph from its deterministic variation.
export function storyLayoutV3(level){
 if(level<5)return storyLayoutV2(level);
 const family=((level-5)*17)%64,shape=family%32,variation=Math.floor((level-5)/64),count=level<15?24:[28,32,36][variation%3],points=[];
 let state=(level*48271+239)%2147483647;const /* Advance the local deterministic variation generator without using global randomness. */
 rnd=()=>{state=state*16807%2147483647;return state/2147483647;};
 const bend=rnd()*18-9,flip=variation%2?-1:1;
 // Append an authored point or option to the current local collection.
 const add=(x,y,a=0)=>points.push({DrawX:500+(x-500)*flip,DrawY:y,Angle:a*flip});
 // Transform a family-specific point into the authored formation coordinates.
 function shapePoint(kind,t){const a=t*Math.PI*2,s=Math.sin(a),c=Math.cos(a);switch(kind){
  case 0:return [500+300*c,180+115*s,a*180/Math.PI+90];
  case 1:{const side=t<.5?-1:1,b=t% .5*4*Math.PI;return [500+side*200+155*Math.cos(b),180+100*Math.sin(b),Math.sin(b)*25];}
  case 2:return [500+335*s,180+110*Math.sin(2*a),Math.cos(a)*25];
  case 3:{const r=190+100*Math.cos(3*a);return [500+r*c,180+r*.38*s,a*180/Math.PI];}
  case 4:return [500+300*Math.sign(c)*Math.sqrt(Math.abs(c)),180+110*Math.sign(s)*Math.sqrt(Math.abs(s)),Math.abs(c)>.75?0:90];
  case 5:{const ring=t<.5?1:.62,b=(t% .5)*4*Math.PI;return [500+300*ring*Math.sign(Math.cos(b)),180+110*ring*Math.sin(b),0];}
  case 6:return [500+320*c,180+115*s,45+(t% .25)*20];
  case 7:{const lane=Math.min(2,Math.floor(t*3)),u=t*3-lane;return [170+lane*330+Math.cos(u*Math.PI)*125,255-Math.sin(u*Math.PI)*155,Math.cos(u*Math.PI)*25];}
  case 8:return [175+t*650,175-Math.abs(Math.sin(t*Math.PI*2))*130+(t>.5?95:0),Math.sin(a)*18];
  case 9:{const ray=Math.floor(t*8),u=t*8-ray,b=ray*Math.PI/4;return [500+Math.cos(b)*(90+u*240),180+Math.sin(b)*(35+u*90),b*180/Math.PI];}
  case 10:{const r=240+45*Math.cos(7*a);return [500+r*c,180+r*.42*s,Math.sin(7*a)*22];}
  case 11:return [500+310*s,180+100*Math.sin(2*a)*(t<.5?1:-1),c*32];
  case 12:return [110+t*780,180+90*Math.sin(t*Math.PI*4),Math.cos(t*Math.PI*4)*28];
  case 13:{const lane=Math.floor(t*4),u=t*4-lane;return [170+u*660,70+lane*70+Math.sin(u*Math.PI*2+lane)*22,lane%2?15:-15];}
  case 14:{const lane=Math.floor(t*4),u=t*4-lane;return [230+lane*180+Math.sin(u*Math.PI)*65,55+u*250,lane%2?25:-25];}
  case 15:{const step=Math.floor(t*8),u=t*8-step;return [150+step*100,95+(step%3)*60+u*55,step%2?9:-9];}
  case 16:case 17:case 18:{const n=kind===16?8:kind===17?6:7,island=Math.min(n-1,Math.floor(t*n)),u=t*n-island;return [95+island*810/(n-1)+(u-.5)*40,120+(island%2)*80+u*65,(u-.5)*75];}
  case 19:{const r=55+t*280;return [500+Math.cos(a*1.7)*r,180+Math.sin(a*1.7)*r*.4,a*70];}
  case 20:{const side=t<.5?-1:1,u=t% .5*2,r=30+u*135,b=u*Math.PI*3;return [500+side*210+Math.cos(b)*r,180+Math.sin(b)*r*.65,b*25];}
  case 21:{const island=Math.floor(t*3),u=t*3-island;return [225+island*275+Math.cos(u*Math.PI*2)*110,175+Math.sin(u*Math.PI*2)*95,u*50];}
  case 22:return [500+20*(16*s*s*s),180-8*(13*c-5*Math.cos(2*a)-2*Math.cos(3*a)-Math.cos(4*a)),s*20];
  case 23:return [120+t*760,220-115*Math.abs(Math.sin(t*Math.PI*3)),Math.cos(t*Math.PI*3)*25];
  case 24:return [500+320*Math.sin(a),180+100*Math.cos(3*a),Math.sin(2*a)*25];
  case 25:{const row=Math.floor(t*4),u=t*4-row;return [170+u*660+(row%2)*35,70+row*70,30];}
  case 26:{const r=Math.floor(t*10)%2?135:320;return [500+r*c,180+r*.35*s,Math.sin(a)*30];}
  case 27:{const ring=t<.5?1:.58,b=t% .5*4*Math.PI;return [500+320*ring*Math.cos(b),180+110*ring*Math.sin(b),Math.cos(b)*22];}
  case 28:return [500+310*c,180+100*Math.sin(2*a),Math.sin(a)*40];
  case 29:{const window=Math.floor(t*4),u=t*4-window;return [170+window*220+Math.cos(u*Math.PI*2)*75,180+Math.sin(u*Math.PI*2)*100,0];}
  case 30:{const lane=Math.floor(t*3),u=t*3-lane,b=u*Math.PI*2;return [220+lane*280+Math.cos(b)*150,180+Math.sin(b)*95,45];}
  default:return [110+t*780,180+100*(2/Math.PI)*Math.asin(Math.sin(t*Math.PI*5)),Math.cos(t*Math.PI*5)*18];
 }}
 for(let i=0;i<count;i++){let [x,y,a]=shapePoint(shape,i/count);if(family>=32){const secondary=shapePoint((shape+7)%32,i/count);x=x*.65+secondary[0]*.35;y=y*.62+secondary[1]*.38+(i%2?20:-20);a=a*.45+(i%2?13:-13);}add(x,y+bend*Math.sin(i),level<15?a*.12:a*.35);}
 // Interleave draw order so loops reveal several usable edges, not a single chain.
 const order=[...points.keys()].sort((a,b)=>(a%4)-(b%4)||a-b),p=order.map(i=>points[i]);
 // Test authored card footprints when constructing the explicit covering graph.
 const overlap=(q,r)=>{const pa=q.Angle*Math.PI/180,qa=r.Angle*Math.PI/180;return [pa,pa+Math.PI/2,qa,qa+Math.PI/2].every(axis=>Math.abs((r.DrawX-q.DrawX)*Math.cos(axis)+(r.DrawY-q.DrawY)*Math.sin(axis))<35*(Math.abs(Math.cos(pa-axis))+Math.abs(Math.cos(qa-axis)))+49*(Math.abs(Math.sin(pa-axis))+Math.abs(Math.sin(qa-axis)))-3);};
 return p.map((s,i)=>({X:i%10,Row:Math.floor(i/10),DrawX:Math.fround(s.DrawX),DrawY:Math.fround(s.DrawY),Angle:Math.fround(s.Angle),BlockedBy:p.flatMap((q,j)=>j>i&&overlap(s,q)?[j]:[])}));
}
