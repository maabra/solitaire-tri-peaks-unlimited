import {gameIcon} from './game-art.mjs';

const destinations={trail:['dice','Trail'],memory:['pairs','Pairs'],album:['album','Album'],missions:['missions','Picnic'],voyage:['boat','Voyage'],tournament:['trophy','Trophies'],hunt:['arrow','Hunt'],bingo:['bingo','Bingo'],regatta:['helm','Regatta'],balloon:['balloon','Flight'],travel:['travel','Travel'],milestones:['medal','Milestones'],chest:['chest','Chest'],postcard:['postcard','Postcards'],tickets:['ticket','Trail'],customize:['studio','Card studio'],hub:['compass','Activities'],eventPlay:['play','Play']};
const controls={shop:['bag','Shop'],gems:['gems','Garden'],continue:['play','Play'],map:['mapRoute','Map'],events:['compass','Activities'],guide:['ruleBook','Cards'],birdHelp:['pipGuide','Pip'],replay:['restart','Replay'],undo:['undo','Undo'],gemHint:['hint','Match'],starHunt:['stars','Stars'],export:['export','Export'],import:['import','Import'],fullscreen:['fullscreen','Full screen']};
export function visualButton(button,icon,label){
 const original=button.getAttribute('aria-label')||button.textContent.trim();
 if(!button.getAttribute('aria-label'))button.setAttribute('aria-label',original);
 if(!button.title)button.title=original;
 button.innerHTML=`${gameIcon(icon,30)}<span>${label}</span>`;
 button.classList.add('visual-control');
}
export function visualizeMenu(root){
 const event=root.querySelector('.event-panel');
 // Long instructions remain available inside the same page and through its i button.
 if(event){const notes=[...event.querySelectorAll(':scope > p,:scope > small')].filter(p=>!p.classList.contains('event-feedback')&&p.textContent.trim().length>90);
  if(notes.length){const details=document.createElement('details');details.className='field-notes';details.innerHTML='<summary><span aria-hidden="true">i</span> Details</summary>';notes.forEach(p=>details.append(p));event.append(details);}}
 root.querySelectorAll('button[data-action]').forEach(button=>{
  if(button.matches('.event-tile,.album-category,.info-button,.shell-close,.skin-card button')||button.closest('.hunt-board'))return;
  const action=button.dataset.action,[type,id]=action.split(':');
  if(type==='ev'&&destinations[id]){const [icon,label]=destinations[id];visualButton(button,icon,label);}
  else if(controls[action]&&!button.closest('.menu-navigation')){const [icon,label]=controls[action];visualButton(button,icon,label);}
 });
 root.querySelectorAll('.shop article').forEach(article=>{const description=article.querySelector('p');if(description)description.classList.add('helper-description');const buy=article.querySelector('[data-action^="buy:"]');if(buy){const title=article.querySelector('h3').cloneNode(true);title.querySelectorAll('button').forEach(e=>e.remove());buy.setAttribute('aria-label',buy.textContent.trim()==='Bag full'?`${title.textContent.trim()} bag is full`:`Buy ${title.textContent.trim()} for ${buy.textContent.trim().replace(/^Buy\s*/, '')} coins`);buy.title=buy.getAttribute('aria-label');}});
}
export function visualFlow(){return `<div class="picture-flow" aria-label="Play cards, earn rewards, grow your album">${[['play','Play'],['gift','Earn'],['album','Collect']].map(([icon,label],i)=>`${i?'<span class="flow-arrow" aria-hidden="true">›</span>':''}<span class="flow-step">${gameIcon(icon,48)}<small>${label}</small></span>`).join('')}</div>`;}
