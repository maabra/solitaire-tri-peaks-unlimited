// The game has its own viewport. UI pixels stay UI pixels: never scale the app.
export function frameSize(){return {width:1280,height:720};}
export const gameWidth=()=>document.body.clientWidth;
export const gameHeight=()=>document.body.clientHeight;
if(typeof document!=='undefined'){
 const offsets=()=>{const r=document.body.getBoundingClientRect(),style=document.documentElement.style;style.setProperty('--game-offset-x',r.left+'px');style.setProperty('--game-offset-y',r.top+'px');};
 const fit=()=>{
  const size=frameSize(innerWidth,innerHeight),style=document.documentElement.style;
  style.setProperty('--game-width',size.width+'px');style.setProperty('--game-height',size.height+'px');
  style.setProperty('--game-left',Math.max(0,(document.documentElement.clientWidth-size.width)/2)+'px');style.setProperty('--game-top',Math.max(0,(document.documentElement.clientHeight-size.height)/2)+'px');
  offsets();
 };
 fit();addEventListener('resize',fit);new ResizeObserver(fit).observe(document.documentElement);
 addEventListener('scroll',offsets,true);
 const backdrop=()=>{const image=document.querySelector('.map-scenery')?.style.backgroundImage||document.querySelector('.landscape')?.style.backgroundImage;if(image)document.documentElement.style.setProperty('--game-backdrop',image);};
 new MutationObserver(backdrop).observe(document.body,{subtree:true,childList:true,attributes:true,attributeFilter:['style']});
}
