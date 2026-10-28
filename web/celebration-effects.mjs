// Combine the saved preference and system setting before creating decorative motion.
export const motionAllowed=()=>!document.body.classList.contains('reduced')&&!matchMedia('(prefers-reduced-motion: reduce)').matches&&!document.hidden;
// Track transient feedback so a preference change immediately stops existing effects.
const feedback=new Set();
function animateFeedback(node,frames,options){const a=node.animate(frames,options);feedback.add(a);a.finished.catch(()=>{}).finally(()=>feedback.delete(a));return a;}
function stopFeedback(){if(!motionAllowed())for(const a of feedback)a.cancel();}
const preferenceObserver=new MutationObserver(stopFeedback);
preferenceObserver.observe(document.body,{attributes:true,attributeFilter:['class']});
const motionPreference=matchMedia('(prefers-reduced-motion: reduce)');
motionPreference.addEventListener('change',()=>{if(motionPreference.matches)for(const a of feedback)a.cancel();});
document.addEventListener('visibilitychange',stopFeedback);
// Apply brief button feedback only when decorative motion is allowed.
export function bounce(node,negative=false){if(!node||!motionAllowed())return;if(node.closest('body>header')){animateFeedback(node,[{filter:'brightness(1)'},{filter:'brightness(1.18)',offset:.4},{filter:'brightness(1)'}],{duration:300,easing:'ease-out'});return;}animateFeedback(node,negative?[{translate:'0'},{translate:'-5px'},{translate:'5px'},{translate:'0'}]:[{scale:'1'},{scale:'1.16',offset:.35},{scale:'.96',offset:.7},{scale:'1'}],{duration:negative?260:440,easing:'ease-out'});}
// Emit short-lived decorative sparks around an element without changing game state.
export function sparkle(node,count=8){if(!node||!motionAllowed())return;const r=node.getBoundingClientRect();for(let i=0;i<Math.min(12,count);i++){const s=document.createElement('span'),angle=i/count*Math.PI*2,distance=28+(i%3)*10;s.className='button-sparkle';s.textContent=i%2?'✦':'●';s.style.left=r.x+r.width/2+'px';s.style.top=r.y+r.height/2+'px';s.style.color=['#ffe17d','#f6a6ca','#b2e8de'][i%3];document.body.append(s);animateFeedback(s,[{transform:'translate(-50%,-50%) scale(.2)',opacity:1},{transform:`translate(${Math.cos(angle)*distance}px,${Math.sin(angle)*distance}px) scale(.8) rotate(90deg)`,opacity:0}],{duration:620,delay:i*14,easing:'ease-out'}).finished.catch(()=>{}).finally(()=>s.remove());}}
