// Preserve the board and surviving card nodes during a move. No re-deal,
// reflow, or entrance animation is applied to untouched cards.
export function patchBoard(host,markup,incremental,reduced){
 const template=document.createElement('template');template.innerHTML=markup;
 const board=host.querySelector('.board'),next=template.content;
 if(!incremental||!board){host.replaceChildren(next);if(!reduced)for(const card of host.querySelectorAll('.board .card')){card.classList.add('entering');setTimeout(()=>card.classList.remove('entering'),500)}return}
 const fresh=next.querySelector('.board');
 for(const card of [...board.children])if(!fresh.querySelector(`[data-action="${card.dataset.action}"]`)){
  card.remove();
 }
 for(const source of fresh.children){let card=board.querySelector(`[data-action="${source.dataset.action}"]`);
  if(!card){board.append(source.cloneNode(true));continue}
  const flipped=card.classList.contains('back')&&!source.classList.contains('back');
  const wasFocused=document.activeElement===card;
  card.className=source.className;card.disabled=source.disabled;card.title=source.title;
  card.setAttribute('aria-label',source.getAttribute('aria-label'));card.style.cssText=source.style.cssText;
  if(card.innerHTML!==source.innerHTML)card.innerHTML=source.innerHTML;
  if(flipped&&!reduced)card.animate([{transform:`rotate(${source.style.getPropertyValue('--angle')}) scaleX(.12)`},{transform:`rotate(${source.style.getPropertyValue('--angle')}) scaleX(1)`}],{duration:260,easing:'ease-out'});
  if(wasFocused&&!card.disabled)card.focus({preventScroll:true});
 }
 const stock=host.querySelector('[data-action="draw"]'),newStock=next.querySelector('[data-action="draw"]');
 stock.disabled=newStock.disabled;
 const waste=host.querySelector('.hand-row .pile:last-of-type .card');
 const newWaste=next.querySelector('.hand-row .pile:last-of-type .card');
 if(waste&&newWaste){waste.className=newWaste.className;if(waste.innerHTML!==newWaste.innerHTML)waste.innerHTML=newWaste.innerHTML}
 for(const selector of ['.run-stat:first-child','.run-stat:last-child','.stock-count','.hand-row .pile small','.subhead .meta','.tools','.play-actions']){
  const old=host.querySelector(selector),fresh=next.querySelector(selector);
  if(old&&fresh&&old.innerHTML!==fresh.innerHTML)old.innerHTML=fresh.innerHTML;
 }
 const notice=host.querySelector('.board-shell>.notice'),newNotice=next.querySelector('.board-shell>.notice');
 if(newNotice){if(notice)notice.replaceWith(newNotice);else host.querySelector('.board-shell').prepend(newNotice)}else notice?.remove();
}
