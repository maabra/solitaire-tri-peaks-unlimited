import {storyLayoutV3,storyNameV3} from './story-layouts-v3.mjs';

const names=['Garden packets','Terraced courtyard','Descending steps','Open royal fan',
 'Clover crosses','Village windows','Twin crescents','Ribbon stairs',
 'Three bouquets','Garden bridge','Diamond gardens','Four corners',
 'Crown promenade','Lantern avenue','Woven terraces','Butterfly fans'];
export const formationFamiliesV4=128;

// Convert authored points into slots and compute their cover links before the hand is dealt.
function compile(points){
 // Test authored card footprints when constructing the explicit covering graph.
 const overlap=(q,r)=>{const a=q.Angle*Math.PI/180,b=r.Angle*Math.PI/180;
  return [a,a+Math.PI/2,b,b+Math.PI/2].every(t=>Math.abs((r.DrawX-q.DrawX)*Math.cos(t)+(r.DrawY-q.DrawY)*Math.sin(t))<
   37*(Math.abs(Math.cos(a-t))+Math.abs(Math.cos(b-t)))+52*(Math.abs(Math.sin(a-t))+Math.abs(Math.sin(b-t)))-1);
 };
 return points.map((p,i)=>({...p,X:i%10,Row:Math.floor(i/10),BlockedBy:points.flatMap((q,j)=>j>i&&overlap(p,q)?[j]:[])}));
}
// Build packet/fan formation families with deliberate covering rather than arbitrary scatter.
function structured(family,variation,opening=0){
 const p=[],/* Append an authored point or option to the current local collection. */
 add=(x,y,angle=0)=>p.push({DrawX:x,DrawY:y,Angle:angle});
 // Place a layered packet of cards with consistent spacing and rotation.
 const packet=(x,y,n=4,lean=0)=>{for(let j=0;j<n;j++)add(x+lean*j,y+j*27,lean*.8);};
 // Place a curved spread of cards whose overlaps will be compiled into cover links.
 const fan=(x,y,n,rx,ry,from=-60,to=60)=>{for(let i=0;i<n;i++){const a=from+(to-from)*i/(n-1),t=a*Math.PI/180;add(x+Math.sin(t)*rx,y-Math.cos(t)*ry,a*.42);}};
 if(opening===1){fan(500,180,6,320,80,-54,54);fan(500,253,7,390,105,-60,60);}
 else if(opening===2){for(const [r,n]of [4,3,2,3,2].entries())for(let i=0;i<n;i++)add(500+(i-(n-1)/2)*125,60+r*57);}
 else if(opening===3){for(let i=0;i<3;i++){packet(190+i*310,85,3);fan(190+i*310,220,3,100,36,-55,55);}}
 else if(opening===4){for(let i=0;i<4;i++)packet(155+i*230,80+(i%2)*45,5,i%2?-3:3);}
 else switch(family){
 case 0:{const n=5+variation%4;for(let i=0;i<n;i++)packet(90+i*820/(n-1),70+(i%2)*48,4+(variation===3?1:0),i%2?-3:3);break;}
 case 1:case 14:{const cols=5+variation%3,rows=4;for(let r=0;r<rows;r++)for(let c=0;c<cols;c++)add(115+c*770/(cols-1)+(r%2)*28,50+r*65,family===14?(r%2?5:-5):0);break;}
 case 2:{for(let r=0;r<5;r++){const n=7-r;for(let c=0;c<n;c++)add(500+(c-(n-1)/2)*112,50+r*57,0);}break;}
 case 3:{fan(500,180,11,425,105,-65,65);fan(500,245,12,455,100,-67,67);break;}
 case 4:{for(let island=0;island<3;island++){const x=185+island*315;add(x-58,155);add(x+58,155);add(x,94);add(x,214);packet(x,130,3);}break;}
 case 5:{for(let i=0;i<4;i++){const x=150+i*232;packet(x-24,55,3);packet(x+24,55,3);add(x,175);add(x,232);}break;}
 case 6:case 15:{fan(280,225,12,195,120,-105,105);fan(720,225,12,195,120,-105,105);if(family===15){packet(80,110,3);packet(920,110,3);}break;}
 case 7:{for(let lane=0;lane<2;lane++)for(let i=0;i<12;i++)add(110+i*70,55+lane*135+i*8,i%3===0?6:0);break;}
 case 8:{for(let i=0;i<3;i++){packet(190+i*310,48,3);fan(190+i*310,210,7,135,70,-65,65);}break;}
 case 9:{for(let i=0;i<5;i++)packet(130+i*185,140-Math.sin(i*Math.PI/4)*85,4);for(let i=0;i<5;i++)add(130+i*185,270);break;}
 case 10:{for(let island=0;island<2;island++)for(let ring=0;ring<2;ring++)for(let i=0;i<6;i++){const a=i*Math.PI/3;add(280+island*440+Math.cos(a)*(135-ring*25),165+Math.sin(a)*(105-ring*30),30);}break;}
 case 11:{for(const x of [190,810])for(const y of [55,220]){packet(x-45,y,3);packet(x+45,y,3);add(x,y+65);}break;}
 case 12:{fan(500,230,13,430,165,-68,68);for(let i=0;i<5;i++)packet(145+i*177,215,3);break;}
 case 13:{for(let i=0;i<7;i++){packet(115+i*128,60+(i%2)*85,4);add(115+i*128,270);}break;}
 }
 if(!opening){const mirror=variation%2?-1:1;for(const q of p){q.DrawX=500+(q.DrawX-500)*mirror;q.Angle*=mirror;q.DrawY+=(variation-1.5)*Math.sin(q.DrawX*.01)*5;}}
 return compile(p);
}
// Name the current formation family, including the occasional long-garden variant.
export function storyNameV4(level){
 if(level>=1000&&level%100===95)return 'Long garden · '+longCardCount(level)+' cards';
 if(level<=4)return ['Pip’s first fan','Little courtyard','Three bouquets','Four little gardens'][level-1];
 const family=(level-5)%128;if(family<64)return names[family%16]+['',' · Twin paths',' · Terraces',' · Festival'][Math.floor(family/16)];
 return storyNameV3(level);
}
// Build the current formation with explicit cover relationships and deterministic variation.
export function storyLayoutV4(level){
 if(level>=1000&&level%100===95){
  const count=longCardCount(level),columns=level<2500?6:8,depth=count/columns,points=[];
  for(let layer=0;layer<depth;layer++)for(let c=0;c<columns;c++)points.push({DrawX:100+c*800/(columns-1),DrawY:60+layer*13+(c%2)*35,Angle:c%2?-3:3});
  return compile(points);
 }
 if(level<=4)return structured(0,0,level);
 const family=(level-5)%128;if(family>=64)return storyLayoutV3(level);
 return structured(family%16,Math.floor(family/16)+Math.floor((level-5)/128)%4);
}
// Select the authored long-garden size only at the designated campaign milestones.
export const longCardCount=level=>level<2500?60:level<6000?80:level<9000?104:120;
