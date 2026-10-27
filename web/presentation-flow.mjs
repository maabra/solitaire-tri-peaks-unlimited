import {motion} from './motion.mjs';
import {gameIcon} from './game-art.mjs';
const reduced=()=>matchMedia('(prefers-reduced-motion: reduce)').matches||document.body.classList.contains('reduced');
export async function flyReward(from,to,icon='coins',count=6){
 if(!from||!to||reduced())return;
 const a=from.getBoundingClientRect(),b=to.getBoundingClientRect();
 await Promise.all(Array.from({length:Math.min(count,8)},async(_,i)=>{
  const el=document.createElement('span');el.className='reward-flight';el.innerHTML=gameIcon(icon,32);el.style.left=a.x+a.width/2-16+'px';el.style.top=a.y+a.height/2-16+'px';document.body.append(el);
  try{await el.animate([{transform:'translate(0,0) scale(.6)',opacity:0},{transform:`translate(${(i-3)*16}px,-45px) scale(1.2)`,opacity:1,offset:.25},{transform:`translate(${b.x+b.width/2-a.x-a.width/2}px,${b.y+b.height/2-a.y-a.height/2}px) scale(.65)`,opacity:1}],{duration:motion.reward,delay:i*motion.rewardStagger,easing:'ease-in-out'}).finished;}catch{/* Cancellation only skips decoration. */}finally{el.remove();}
 }));
}
// Presentation only. Rewards were already committed by Session's winning move.
// Completion can finish by animation or by the skip action. The caller checks that the
// same hand is still active before showing results, avoiding stale UI after navigation.
export async function presentCompletion(hand,stillCurrent,done){
 if(reduced()){if(stillCurrent())done();return;}
 const overlay=document.createElement('button');overlay.className='completion-stage';overlay.setAttribute('aria-label','Skip celebration');
 overlay.innerHTML=`<span class="stock-bonus-art">${Array.from({length:Math.min(hand.Stock.length,12)},(_,i)=>`<i style="--i:${i}">${gameIcon('ratingStar',36)}</i>`).join('')}</span><strong>${hand.Stock.length?'Stock bonus':'Beautifully played!'}</strong><small>Tap to skip</small>`;
 document.body.append(overlay);
 let finish;const skip=new Promise(resolve=>{finish=resolve});overlay.onclick=finish;
 const wait=ms=>Promise.race([new Promise(resolve=>setTimeout(resolve,ms)),skip]);
 try{if(hand.Stock.length)await wait(motion.completeStock);if(!stillCurrent())return;overlay.querySelector('strong').textContent='You win!';overlay.classList.add('celebrating');await wait(motion.completeWin);if(stillCurrent())done();}finally{overlay.remove();}
}
export function animateResults(root,stars,coins){
 const amounts=root.querySelectorAll('[data-result-coins]');
 if(reduced())return;
 for(const [i,star]of [...root.querySelectorAll('.result-star.earned')].entries())star.animate([{transform:'scale(.2)',opacity:0},{transform:'scale(1.35)',opacity:1,offset:.7},{transform:'scale(1)',opacity:1}],{duration:motion.star,delay:i*motion.starStagger,fill:'backwards'});
 const begin=performance.now();function frame(now){if(!root.isConnected)return;const t=Math.min(1,(now-begin)/motion.resultCount);for(const el of amounts)el.textContent=Math.round(coins*(1-(1-t)**3)).toLocaleString();if(t<1)requestAnimationFrame(frame);}requestAnimationFrame(frame);
}
