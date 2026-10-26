// The game has its own viewport. UI pixels stay UI pixels: never scale the app.
export function frameSize(){return {width:1280,height:720};}
export const gameWidth=()=>document.body.clientWidth;
export const gameHeight=()=>document.body.clientHeight;
if(typeof document!=='undefined'){
 // Fixed HUD children do not reliably inherit their map's native wheel target.
 // Route wheel input to the nearest scroll area and keep it there at its ends.
 document.addEventListener('wheel',event=>{
  if(event.ctrlKey||event.defaultPrevented)return;
  const target=event.target instanceof Element?event.target:null;
  if(!target)return;
  const menu=target.closest('.map-dock');
  let area=menu?.querySelector('.map-dock-scroll');
  if(!area)for(let node=target;node&&node!==document.body;node=node.parentElement){
   const style=getComputedStyle(node);
   if(/auto|scroll/.test(style.overflowY)&&node.scrollHeight>node.clientHeight+1||/auto|scroll/.test(style.overflowX)&&node.scrollWidth>node.clientWidth+1){area=node;break;}
  }
  area??=target.closest('.map-viewport');
  if(!area)return;
  const unit=event.deltaMode===1?32:event.deltaMode===2?area.clientHeight:1;
  let x=event.deltaX*unit,y=event.deltaY*unit;
  if(event.shiftKey&&!x){x=y;y=0;}
  if(area.scrollHeight<=area.clientHeight+1&&area.scrollWidth>area.clientWidth+1){x+=y;y=0;}
  area.scrollLeft+=x;area.scrollTop+=y;
  event.preventDefault();
 },{passive:false});
 const offsets=()=>{const r=document.body.getBoundingClientRect(),style=document.documentElement.style;style.setProperty('--game-offset-x',r.left+'px');style.setProperty('--game-offset-y',r.top+'px');};
 const fit=()=>{
  const size=frameSize(innerWidth,innerHeight),style=document.documentElement.style;
  style.setProperty('--game-width',size.width+'px');style.setProperty('--game-height',size.height+'px');
  style.setProperty('--game-left',Math.max(0,(innerWidth-size.width)/2)+'px');style.setProperty('--game-top',Math.max(0,(innerHeight-size.height)/2)+'px');
  const margin=Math.min(Math.max(0,(innerWidth-size.width)/2),Math.max(0,(innerHeight-size.height)/2));
  style.setProperty('--frame-corner-size',Math.min(88,Math.max(20,margin*1.5))+'px');
  const crest=Math.min(48,Math.max(14,margin*1.25));style.setProperty('--frame-crest-height',crest+'px');style.setProperty('--frame-crest-width',crest*3+'px');
  offsets();
 };
 const decorations=document.createElement('div');decorations.id='frame-ornaments';decorations.setAttribute('aria-hidden','true');decorations.innerHTML='<i class="corner tl"></i><i class="corner tr"></i><i class="corner bl"></i><i class="corner br"></i><i class="crest top"></i><i class="crest bottom"></i>';document.documentElement.append(decorations);
 fit();addEventListener('resize',fit);new ResizeObserver(fit).observe(document.documentElement);
 addEventListener('scroll',offsets,true);
 const backdrop=()=>{const image=document.querySelector('.map-scenery')?.style.backgroundImage||document.querySelector('.landscape')?.style.backgroundImage;if(image)document.documentElement.style.setProperty('--game-backdrop',image);};
 new MutationObserver(backdrop).observe(document.body,{subtree:true,childList:true,attributes:true,attributeFilter:['style']});
}
