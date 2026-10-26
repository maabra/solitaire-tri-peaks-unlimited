// The game has its own viewport. UI pixels stay UI pixels: never scale the app.
export function frameSize(width,height){
 if(width<=700)return {width,height:Math.min(720,height)};
 const w=Math.min(1280,width,height*16/9);
 return {width:w,height:w*9/16};
}
export const gameWidth=()=>document.body.clientWidth;
export const gameHeight=()=>document.body.clientHeight;
if(typeof document!=='undefined'){
 const fit=()=>{
  const size=frameSize(innerWidth,innerHeight),style=document.documentElement.style;
  style.setProperty('--game-width',size.width+'px');style.setProperty('--game-height',size.height+'px');
  style.setProperty('--game-left',(innerWidth-size.width)/2+'px');style.setProperty('--game-top',(innerHeight-size.height)/2+'px');
 };
 fit();addEventListener('resize',fit);
 const backdrop=()=>{const image=document.querySelector('.map-scenery')?.style.backgroundImage||document.querySelector('.landscape')?.style.backgroundImage;if(image)document.documentElement.style.setProperty('--game-backdrop',image);};
 new MutationObserver(backdrop).observe(document.body,{subtree:true,childList:true,attributes:true,attributeFilter:['style']});
}
