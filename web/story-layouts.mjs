export const storyNames=['Three peaks','Royal fan','Moon ring','Twin fans','Butterfly','River ribbon','Diamond loop','Starburst','Five towers','Double arch','Crown','Braided trail','Sunrise','Eight towers','Hourglass','Serpent'];
// Return the historical story-family name retained for older generation compatibility.
export const storyName=level=>`${storyNames[(level-1)%16]} ${Math.floor((level-1)/16)+1}`;
// Build the original authored formation families used by historical deals.
export function storyLayout(level){
 const family=(level-1)%16,variation=Math.floor((level-1)/16),p=[];
 // Append an authored point or option to the current local collection.
 const add=(X,Y,Angle=0)=>p.push({DrawX:X,DrawY:Y,Angle});
 // Add a layered stack to the authored formation's point collection.
 const stack=(x,y,n,lean=0)=>{for(let i=0;i<n;i++)add(x+lean*i,y+i*21,lean*1.5)};
 // Place cards along an arc while retaining each card's intended rotation.
 const arc=(cx,cy,rx,ry,n,from,to,split=true)=>{let a=Array.from({length:n},(_,i)=>from+(to-from)*i/(n-1));if(split)a.sort((a,b)=>Math.abs(a)-Math.abs(b));for(let d of a){let t=d*Math.PI/180;add(cx+rx*Math.sin(t),cy-ry*Math.cos(t),d)}};
 const bend=((variation*48271+12345)%32749)/32749*24-12;
 if(family===0){for(let row=0;row<3;row++)for(let peak=0;peak<3;peak++)for(let j=0;j<=row;j++)add(500+(78+(2+peak*3-row*.5+j)*81-500)*(1+bend*.004),48+row*(55+bend*.15));for(let j=0;j<10;j++)add(500+(78+(.5+j)*81-500)*(1+bend*.004),48+3*(55+bend*.15))}
 else if([1,10,12].includes(family)){arc(500,170,(family===10?320:300)+bend,family===10?86:112,16,family===10?-80:-68,family===10?80:68);let n=family===12?5:3;for(let i=0;i<n;i++)stack(90+i*820/(n-1),195,n===5?3:4,(i%2===0?1:-1)*(variation%3))}
 else if([2,6,14].includes(family)){let n=20;for(let i=0;i<n;i++){let t=(i*360/n-90)*Math.PI/180,x,y;if(family===6){let a=i*4/n,e=Math.floor(a),f=a-e,c=[[500,46+bend*.3],[770+bend,155],[500,263-bend*.3],[230-bend,155],[500,46+bend*.3]];x=c[e][0]+(c[e+1][0]-c[e][0])*f;y=c[e][1]+(c[e+1][1]-c[e][1])*f}else{x=500+(family===14?Math.sin(t*2):Math.cos(t))*(230+bend);y=155+Math.sin(t)*100}add(x,y,t*180/Math.PI+90)}stack(92,100,4);stack(908,100,4)}
 else if([3,4,9].includes(family)){if(family===4){arc(380,170,220+bend,108,12,-112,-5);arc(620,170,220+bend,108,12,5,112)}else{arc(282,157,176+bend,family===9?65:95,12,-65,65);arc(718,157,176+bend,family===9?65:95,12,-65,65)}stack(110,207,3);stack(500,207,3);stack(890,207,3)}
 else if([5,11,15].includes(family)){for(let i=0;i<26;i++){let t=i/25,w=t*Math.PI*(family===15?3:2);add(85+t*830,155+Math.sin(w)*(70+bend),Math.cos(w)*25)}if(family===11){stack(150,210,3);stack(850,210,3)}}
 else if(family===7){for(let arm=0;arm<6;arm++)for(let i=0;i<5;i++){let a=(arm*60-90)*Math.PI/180;add(500+Math.cos(a)*(48+i*(48+bend*.25)),155+Math.sin(a)*(24+i*(20+bend*.1)),arm*60)}}
 else{let n=family===13?8:5+variation%3;for(let i=0;i<n;i++)stack(80+i*(840+bend)/(n-1),80+(i%2)*28,4+(i<variation%3?1:0),(i%2===0?1:-1)*(2+variation%4))}
 // Test authored card footprints when constructing the explicit covering graph.
 const overlap=(a,b)=>{let q=p[a],r=p[b],pa=q.Angle*Math.PI/180,qa=r.Angle*Math.PI/180;return [pa,pa+Math.PI/2,qa,qa+Math.PI/2].every(axis=>Math.abs((r.DrawX-q.DrawX)*Math.cos(axis)+(r.DrawY-q.DrawY)*Math.sin(axis))<35*(Math.abs(Math.cos(pa-axis))+Math.abs(Math.cos(qa-axis)))+49*(Math.abs(Math.sin(pa-axis))+Math.abs(Math.sin(qa-axis)))-3)};
 return p.map((s,i)=>({X:i%10,Row:Math.floor(i/10),DrawX:Math.fround(s.DrawX),DrawY:Math.fround(s.DrawY),Angle:Math.fround(s.Angle),BlockedBy:p.flatMap((_,j)=>j>i&&overlap(i,j)?[j]:[])}));
}
