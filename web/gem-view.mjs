import {gemMarkup} from './gem-art.mjs';
const colors=['#bc6a74','#6e8fbb','#edb95d','#75a87e','#a481b4','#77b7bc'];
const icons=['♥','◆','✦','✿','●','⬟'];
const pause=ms=>new Promise(resolve=>setTimeout(resolve,ms));
export async function animateGemMatch(grid,first,second,frames,finalCells,reduced){
 const buttons=[...grid.querySelectorAll('.gem')];
 const cells=values=>values.forEach((v,i)=>{buttons[i].innerHTML=gemMarkup(v);buttons[i].style.background=`linear-gradient(145deg,${colors[v]},${colors[v]}cc)`});
 buttons.forEach(b=>{b.disabled=true;b.classList.remove('found','selected')});
 const rects=buttons.map(b=>b.getBoundingClientRect()),box=grid.getBoundingClientRect(),base={x:box.x+grid.clientLeft,y:box.y+grid.clientTop,width:grid.clientWidth,height:grid.clientHeight};
 let matched=[];
 for(const frame of frames){
  if(!grid.isConnected)return;
  if(frame.Kind==='swap'){
   if(!reduced){let a=rects[first],b=rects[second];await Promise.all([buttons[first].animate([{transform:'translate(0,0)'},{transform:`translate(${b.x-a.x}px,${b.y-a.y}px)`}],{duration:180,easing:'ease-in-out'}).finished,buttons[second].animate([{transform:'translate(0,0)'},{transform:`translate(${a.x-b.x}px,${a.y-b.y}px)`}],{duration:180,easing:'ease-in-out'}).finished])}
   cells(frame.Cells);
  }else if(frame.Kind==='match'){
   matched=frame.Matched;cells(frame.Cells);
   const svg=document.createElementNS('http://www.w3.org/2000/svg','svg');svg.classList.add('gem-links');svg.setAttribute('viewBox',`0 0 ${base.width} ${base.height}`);
   for(const i of matched){buttons[i].classList.add('matched');for(const j of [i+1,i+8])if(matched.includes(j)&&(j===i+8||Math.floor(j/8)===Math.floor(i/8))){let path=document.createElementNS(svg.namespaceURI,'path'),a=rects[i],b=rects[j];path.setAttribute('d',`M${a.x-base.x+a.width/2},${a.y-base.y+a.height/2} L${b.x-base.x+b.width/2},${b.y-base.y+b.height/2}`);svg.append(path)}}
   grid.append(svg);
   const reward=document.createElement('span');reward.className='gem-reward';reward.textContent=`+${matched.length*2*frame.Cascade} ●${frame.Cascade>1?' · ×'+frame.Cascade:''}`;reward.style.left=matched.reduce((sum,i)=>sum+rects[i].x-base.x+rects[i].width/2,0)/matched.length+'px';reward.style.top=matched.reduce((sum,i)=>sum+rects[i].y-base.y,0)/matched.length+'px';grid.append(reward);
   await pause(reduced?180:480);
   if(!reduced)await Promise.all(matched.map(i=>buttons[i].animate([{transform:'scale(1.12)',opacity:1},{transform:'scale(.12)',opacity:0}],{duration:170,easing:'ease-in'}).finished));
   svg.remove();reward.remove();matched.forEach(i=>buttons[i].classList.remove('matched'));
  }else if(frame.Kind==='drop'){
   cells(frame.Cells);
   if(!reduced){let falls=[];for(let col=0;col<8;col++){let kept=Array.from({length:8},(_,r)=>r*8+col).filter(i=>!matched.includes(i)),missing=8-kept.length;if(!missing)continue;for(let row=0;row<8;row++){let i=row*8+col,source=row<missing?row-missing:Math.floor(kept[row-missing]/8),distance=(source-row)*(rects[8].y-rects[0].y);falls.push(buttons[i].animate([{transform:`translateY(${distance}px)`,opacity:row<missing?.1:1},{transform:'translateY(0)',opacity:1}],{duration:350+col*12,easing:'cubic-bezier(.24,.7,.36,1)'}).finished)}}await Promise.all(falls)}
  }
 }
 cells(finalCells);buttons.forEach(b=>b.disabled=false);
}
