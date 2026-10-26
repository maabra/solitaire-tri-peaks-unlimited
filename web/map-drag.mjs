// Pan only the map surface. Scrollbars and all HUD furniture keep native input.
export function installMapDrag(viewport){
 let drag=null;
 const furniture='.map-dock,.map-controls,.chapter-ribbon,.map-companion,.bird-guide,button,a,input,select,textarea,[role="scrollbar"]';
 viewport.addEventListener('pointerdown',e=>{
  if(e.pointerType==='touch'||e.button!==0||!e.isPrimary||e.target.closest(furniture))return;
  if(e.target!==viewport&&!e.target.closest('.map-camera,.map-scenery,.map-ambient'))return;
  const field=viewport.querySelector('.map-picture-frame')?.getBoundingClientRect();
  if(field&&(e.clientX<field.left+10||e.clientX>field.right-10||e.clientY<field.top+10||e.clientY>field.bottom-10))return;
  const r=viewport.getBoundingClientRect();
  if(e.clientX>=r.left+viewport.clientLeft+viewport.clientWidth||e.clientY>=r.top+viewport.clientTop+viewport.clientHeight)return;
  drag={id:e.pointerId,x:e.clientX,y:e.clientY,left:viewport.scrollLeft,top:viewport.scrollTop};
  viewport.setPointerCapture(e.pointerId);
 });
 viewport.addEventListener('pointermove',e=>{
  if(!drag||e.pointerId!==drag.id)return;
  if(!(e.buttons&1)){drag=null;return;}
  viewport.scrollLeft=drag.left+drag.x-e.clientX;viewport.scrollTop=drag.top+drag.y-e.clientY;
 });
 const stop=()=>{drag=null;};
 viewport.addEventListener('pointerup',stop);viewport.addEventListener('pointercancel',stop);viewport.addEventListener('lostpointercapture',stop);
}
