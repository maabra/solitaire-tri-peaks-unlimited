import {gameIcon} from './game-art.mjs';
import {motionAllowed} from './celebration-effects.mjs';
// Choose an independent delay so neighboring attention effects do not pulse together.
export const attentionDelay=(random=Math.random)=>4000+Math.floor(random()*10000);
// Each badge/glint owns its next timeout; rerenders do not restart its neighbors.
// Give each visible badge its own timer rather than using a global animation tick.
// Independent schedules keep exclamation marks and glints from pulsing in unison.
export function installAmbientAttention(){
 const records=new Map(),media=matchMedia('(prefers-reduced-motion: reduce)');
 const selector='.story-play,.tool:not(:disabled),.new-feature-badge,.map-icon:has(.new-feature-badge),.event-tile:has(.new-feature-badge),.tool:has(.new-feature-badge)';
 // Check viewport visibility and occlusion before spending an attention animation on a hidden badge.
 const visible=node=>{const r=node.getBoundingClientRect();if(!r.width||!r.height||r.bottom<0||r.right<0||r.top>innerHeight||r.left>innerWidth)return false;const hit=document.elementFromPoint(r.left+r.width/2,r.top+r.height/2);return !!hit&&(node.contains(hit)||node.parentElement?.contains(hit));};
 // Schedule only one pending attention effect per element and honor the reduced-motion preference.
 function schedule(node,record){if(record.timer||!node.isConnected||!motionAllowed())return;const delay=attentionDelay();node.dataset.attentionDelay=delay;record.timer=setTimeout(()=>{record.timer=null;if(!node.isConnected)return;if(motionAllowed()&&!document.querySelector('.completion-stage')&&visible(node)){
   if(node.matches('.new-feature-badge,.story-play')){
    const a=node.animate([{transform:'translateY(0) rotate(0deg)'},{transform:'translateY(-6px) rotate(-9deg)',offset:.35},{transform:'translateY(-2px) rotate(7deg)',offset:.7},{transform:'translateY(0) rotate(0deg)'}],{duration:620,easing:'ease-in-out'});record.animations=[a];
   }else{
    const glint=document.createElement('span');glint.className=node.matches('.tool')?'ambient-glint helper-lightning':'ambient-glint';glint.setAttribute('aria-hidden','true');glint.innerHTML=node.matches('.tool')?'&#9889;':gameIcon('stars',15);glint.style.left=(25+Math.random()*50)+'%';glint.style.top=(8+Math.random()*24)+'%';node.append(glint);
    const a=glint.animate([{opacity:0,transform:'scale(.2) rotate(-30deg)'},{opacity:1,transform:'scale(1.15) rotate(15deg)',offset:.45},{opacity:0,transform:'scale(.25) rotate(75deg)'}],{duration:760,easing:'ease-in-out'});record.animations=[a];a.finished.catch(()=>{}).finally(()=>glint.remove());
   }
  }schedule(node,record);},delay);}
 // Discover newly rendered badges and release records for elements removed by navigation.
 function scan(){
  for(const [node,r]of records)if(!node.isConnected||!node.matches(selector)){clearTimeout(r.timer);r.animations?.forEach(a=>a.cancel());records.delete(node);}
  for(const node of document.querySelectorAll(selector)){if(!records.has(node))records.set(node,{timer:null});schedule(node,records.get(node));}
 }
 // React to the system motion setting by updating active attention schedules.
 function motionChange(){for(const [node,r]of records){clearTimeout(r.timer);r.timer=null;if(!motionAllowed()){r.animations?.forEach(a=>a.cancel());r.animations=[];}}scan();}
 const observer=new MutationObserver(scan);observer.observe(document.body,{childList:true,subtree:true});
 const preferences=new MutationObserver(motionChange);preferences.observe(document.body,{attributes:true,attributeFilter:['class']});
 media.addEventListener('change',motionChange);document.addEventListener('visibilitychange',motionChange);scan();
 return ()=>{observer.disconnect();preferences.disconnect();media.removeEventListener('change',motionChange);document.removeEventListener('visibilitychange',motionChange);for(const r of records.values()){clearTimeout(r.timer);r.animations?.forEach(a=>a.cancel());}records.clear();};
}
