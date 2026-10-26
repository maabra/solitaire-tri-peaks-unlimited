import {gameWidth,gameHeight} from './game-frame.mjs';
// Fit a hand once per viewport, never from its changing score/effects/helper state.
export class PlayViewport {
 constructor(){this.hand=null;this.viewport='';this.layout=null;}
 fit(root,hand){
  const viewport=gameWidth()+'x'+gameHeight(),shell=root.querySelector('.board-shell'),tools=root.querySelector('.tools');
  if(!shell)return;
  if(this.hand!==hand||this.viewport!==viewport){
   this.hand=hand;this.viewport=viewport;
   root.style.zoom='';root.style.width='';root.style.maxWidth='';shell.style.minHeight='';if(tools)tools.style.minHeight='';
   const height=shell.getBoundingClientRect().height,toolsHeight=tools?.getBoundingClientRect().height??0;
   const scale=1;
   this.layout={scale,height,toolsHeight};
  }
  const {scale,height,toolsHeight}=this.layout;
  // Reapply cached dimensions after returning from menus, without measuring again.
  if(gameWidth()<700){root.style.zoom='';root.style.width='';root.style.maxWidth='';}
  else{root.style.zoom=String(scale);root.style.width='100%';root.style.maxWidth=1180/scale+'px';}
  shell.style.minHeight=height+'px';if(tools)tools.style.minHeight=toolsHeight+'px';
 }
}
