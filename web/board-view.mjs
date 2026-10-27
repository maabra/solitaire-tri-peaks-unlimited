import {flyCard} from './card-flight.mjs';
import {motion} from './motion.mjs';
// Preserve the board and surviving card nodes during a move. No re-deal,
// reflow, or entrance animation is applied to untouched cards.
// Patch surviving cards in place instead of rebuilding the tableau on every move. Stable DOM nodes
// keep their positions and focus while a separate decorative copy flies toward the waste.
export function patchBoard(host,markup,incremental,reduced,effect=null){
 const template=document.createElement('template');template.innerHTML=markup;
 const board=host.querySelector('.board'),next=template.content;
 if(!incremental||!board){host.replaceChildren(next);if(!reduced){const stock=host.querySelector('[data-action="draw"]')?.getBoundingClientRect();if(stock)for(const [i,card]of [...host.querySelectorAll('.board .card')].entries()){const to=card.getBoundingClientRect(),dx=stock.x+stock.width/2-to.x-to.width/2,dy=stock.y+stock.height/2-to.y-to.height/2,angle=card.style.getPropertyValue('--angle')||'0deg';card.animate([{transform:`translate(${dx}px,${dy}px) rotate(-8deg) scale(.8)`,opacity:0},{transform:`translate(${dx*.8}px,${dy*.8}px) rotate(-5deg) scale(.9)`,opacity:1,offset:.15},{transform:`translate(0,0) rotate(${angle}) scale(1)`,opacity:1}],{duration:motion.deal,delay:Math.min(i,32)*motion.dealStagger,easing:'cubic-bezier(.2,.7,.3,1)',fill:'backwards'})}}return}

 const oldTableauCount=board.children.length;
 const oldScore=Number(host.querySelector('.run-stat:last-child strong')?.textContent)||0;
 const nextScore=Number(next.querySelector('.run-stat:last-child strong')?.textContent)||0;
 const fresh=next.querySelector('.board');
 board.style.cssText=fresh.style.cssText;board.className=fresh.className;
 for(const card of [...board.children])if(!fresh.querySelector(`[data-action="${card.dataset.action}"]`)){
  if(!reduced)flyCard(card,host.querySelector('.hand-row .pile:nth-child(3) .card'));
  if(!reduced&&nextScore>oldScore){const r=card.getBoundingClientRect(),label=document.createElement('span');label.className='card-score-flight';label.textContent='+'+(nextScore-oldScore);label.style.left=r.x+'px';label.style.top=r.y+'px';document.body.append(label);label.animate([{transform:'translateY(0)',opacity:1},{transform:'translateY(-45px)',opacity:0}],{duration:motion.score}).finished.finally(()=>label.remove());}
  card.remove();
 }
 for(const source of fresh.children){let card=board.querySelector(`[data-action="${source.dataset.action}"]`);
  if(!card){board.append(source.cloneNode(true));continue}
  const flipped=card.classList.contains('back')&&!source.classList.contains('back');
  const wasFocused=document.activeElement===card;
  card.className=source.className;card.disabled=source.disabled;card.title=source.title;
  card.dataset.suit=source.dataset.suit;
  card.setAttribute('aria-label',source.getAttribute('aria-label'));card.style.cssText=source.style.cssText;
  if(card.innerHTML!==source.innerHTML)card.innerHTML=source.innerHTML;
  if((flipped||effect==='shuffle'&&!source.classList.contains('back'))&&!reduced)card.animate([{transform:`rotate(${source.style.getPropertyValue('--angle')}) scaleX(.12)`},{transform:`rotate(${source.style.getPropertyValue('--angle')}) scaleX(1)`}],{duration:motion.flip,easing:'ease-out'});
  if(wasFocused&&!card.disabled)card.focus({preventScroll:true});
 }
 const stock=host.querySelector('[data-action="draw"]'),newStock=next.querySelector('[data-action="draw"]');
 const oldCount=Number(host.querySelector('.stock-count')?.textContent),nextCount=Number(next.querySelector('.stock-count')?.textContent);
 if(!reduced&&nextCount>oldCount)for(let i=0;i<Math.min(nextCount-oldCount,4);i++)flyCard(stock,stock,{offset:{x:80+i*20,y:-110},delay:i*motion.stockStagger,kind:'stock'});
 // Draw feedback is local: never replace or redeal the tableau.
 if(!reduced&&nextCount===oldCount-1&&fresh.children.length===oldTableauCount){
  const destination=host.querySelector('.hand-row .pile:nth-child(3) .card'),face=next.querySelector('.hand-row .pile:nth-child(3) .card');
  flyCard(stock,destination,{face,kind:'draw'});
 }
 stock.disabled=newStock.disabled;
 const waste=host.querySelector('.hand-row .pile:nth-child(3) .card');
 const newWaste=next.querySelector('.hand-row .pile:nth-child(3) .card');
 if(waste&&newWaste){waste.className=newWaste.className;waste.dataset.suit=newWaste.dataset.suit;if(waste.innerHTML!==newWaste.innerHTML)waste.innerHTML=newWaste.innerHTML}
 for(const selector of ['.run-stat:first-child','.run-stat:last-child','.stock-count','.hand-row .pile small','.subhead .meta','.star-meter','.subhead [data-action="favorite"]','.table-status','.tools','.play-actions']){
  const old=host.querySelector(selector),fresh=next.querySelector(selector);
  if(old&&fresh){if(old.innerHTML!==fresh.innerHTML)old.innerHTML=fresh.innerHTML;if(fresh.hasAttribute('aria-label'))old.setAttribute('aria-label',fresh.getAttribute('aria-label'))}
 }
 const notice=host.querySelector('.board-shell>.notice'),newNotice=next.querySelector('.board-shell>.notice');
 if(newNotice){if(notice)notice.replaceWith(newNotice);else host.querySelector('.board-shell').prepend(newNotice)}else notice?.remove();
}
