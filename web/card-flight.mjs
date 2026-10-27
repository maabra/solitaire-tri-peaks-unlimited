import {motion} from './motion.mjs';
// Screen rectangles must be converted to the centered game's coordinates.
// The flight is decoration only: it never blocks the next legal move.
export function flyCard(source,target,{face=source,offset={x:0,y:0},delay=0,kind='play'}={}){
 if(!source||!target)return;
 const a=source.getBoundingClientRect(),b=target.getBoundingClientRect(),stage=document.body.getBoundingClientRect();
 const style=getComputedStyle(source),w=parseFloat(style.width)||a.width,h=parseFloat(style.height)||a.height;
 const left=a.x+a.width/2-w/2-stage.x+offset.x,top=a.y+a.height/2-h/2-stage.y+offset.y;
 const dx=b.x+b.width/2-(a.x+a.width/2+offset.x),dy=b.y+b.height/2-(a.y+a.height/2+offset.y);
 // The clone is visual only: strip interaction and identity so it cannot intercept a click
 // or duplicate an accessible card while the real hand has already advanced.
 const copy=face.cloneNode(true);copy.removeAttribute('data-action');copy.removeAttribute('id');copy.removeAttribute('title');
 copy.classList.remove('entering','hint-card','guide-highlight');copy.classList.add('card-flight');copy.dataset.flight=kind;copy.disabled=true;copy.tabIndex=-1;copy.setAttribute('aria-hidden','true');
 copy.style.cssText=`position:fixed;left:${left}px;top:${top}px;width:${w}px;height:${h}px;--cw:${w}px;z-index:11000;pointer-events:none;opacity:1;transform-origin:center`;
 document.body.append(copy);
 const angle=parseFloat(source.style.getPropertyValue('--angle'))||0,turn=kind==='stock'?720:360;
 const animation=copy.animate([
  {transform:`perspective(700px) rotateY(0deg) rotate(${angle}deg) scale(1)`,opacity:1,filter:'drop-shadow(0 0 2px #ffe8a5)'},
  {transform:`perspective(700px) translate(${dx*.32}px,${dy*.28-42}px) rotateY(180deg) rotate(${angle+turn*.45}deg) scale(1.1)`,opacity:1,filter:'drop-shadow(0 0 14px #ffe8a5)',offset:.4},
  {transform:`perspective(700px) translate(${dx*.82}px,${dy*.78-14}px) rotateY(320deg) rotate(${turn*.85}deg) scale(.95)`,opacity:1,filter:'drop-shadow(0 0 8px #fff0ad)',offset:.8},
  {transform:`perspective(700px) translate(${dx}px,${dy}px) rotateY(360deg) rotate(${turn}deg) scale(${Math.min(1.1,b.width/w)})`,opacity:0,filter:'drop-shadow(0 0 1px #ffe8a5)'}
 ],{duration:motion.cardFlight,delay,easing:'cubic-bezier(.2,.65,.3,1)'});
 animation.finished.catch(()=>{}).finally(()=>copy.remove());
 for(let i=0;i<6;i++){
  const spark=document.createElement('span');spark.className='flight-spark';spark.textContent=i%2?'✦':'✧';spark.setAttribute('aria-hidden','true');spark.style.left=left+w/2+'px';spark.style.top=top+h/2+'px';document.body.append(spark);
  spark.animate([{transform:'scale(.2)',opacity:0},{transform:`translate(${dx*.45+(i-3)*8}px,${dy*.4-24-i*3}px) scale(1)`,opacity:1,offset:.5},{transform:`translate(${dx}px,${dy}px) scale(.1)`,opacity:0}],{duration:motion.cardFlight,delay:delay+i*18,easing:'ease-out'}).finished.catch(()=>{}).finally(()=>spark.remove());
 }
 return animation;
}
