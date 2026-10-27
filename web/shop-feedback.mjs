import {gameIcon} from './game-art.mjs';
import {motionAllowed} from './celebration-effects.mjs';
import {resourceBadge} from './resource-view.mjs';

const receipts=new WeakMap();

// Update the existing shelf after a saved transaction. Keeping these DOM nodes preserves
// scroll, focus and card positions instead of replaying the menu's entrance animation.
export function refreshShopStock(root,session,catalog){
 const wallet=root.querySelector('.shop-wallet [data-resource=coins]');
 if(wallet)wallet.outerHTML=resourceBadge(session.p.Coins,'coins',session.p.Coins+' coins available');
 for(const article of root.querySelectorAll('[data-shop-helper]')){
  const helper=catalog.find(h=>h.Id===article.dataset.shopHelper);
  if(!helper)continue;
  const count=session.inventory(helper.Id),full=count>=2147483647,button=article.querySelector('.shop-buy');
  article.querySelector('[data-owned]').textContent=count;
  const html=full?'Bag full':'Buy '+resourceBadge(helper.Price,'coins',helper.Price+' coins');
  const label=full?helper.Name+' bag is full':'Buy '+helper.Name+' for '+helper.Price+' coins';
  button.disabled=full||session.p.Coins<helper.Price;
  const active=receipts.get(article);
  if(active){active.html=html;active.label=label;}
  else{button.innerHTML=html;button.setAttribute('aria-label',label);}
 }
}

// Present only an already-saved purchase. Feedback never spends coins or grants helpers,
// and stays inside the card so a scrolled shelf cannot clip a screen-positioned effect.
export function showPurchaseFeedback(root,id,name){
 const article=root.querySelector(`[data-shop-helper="${CSS.escape(id)}"]`);
 if(!article)return;
 const button=article.querySelector('.shop-buy'),icon=article.querySelector('.icon');
 const previous=receipts.get(article);
 if(previous){clearTimeout(previous.timer);previous.pop.remove();}
 const receipt={html:previous?.html??button?.innerHTML,label:previous?.label??button?.getAttribute('aria-label'),pop:null,timer:null};
 article.classList.add('shop-purchased');
 article.classList.toggle('shop-purchase-static',!motionAllowed());
 const pop=document.createElement('span');pop.className='shop-purchase-pop';pop.textContent='+1';pop.setAttribute('aria-hidden','true');
 receipt.pop=pop;receipts.set(article,receipt);
 icon?.append(pop);
 if(button){button.innerHTML=gameIcon('check',22)+'<span>Bought!</span>';button.setAttribute('aria-label',name+' bought, one added to your bag');}
 const status=root.querySelector('.shop-instructions');
 if(status){status.classList.add('shop-purchase-message');requestAnimationFrame(()=>{if(status.isConnected)status.textContent=name+' bought! +1 added to your bag.';});}
 // Keep the confirmation readable without disabling another legitimate purchase.
 // Repeated purchases replace this card's timer; another item's receipt remains visible.
 receipt.timer=setTimeout(()=>{
  pop.remove();article.classList.remove('shop-purchased','shop-purchase-static');
  receipts.delete(article);
  if(button?.isConnected){button.innerHTML=receipt.html;if(receipt.label==null)button.removeAttribute('aria-label');else button.setAttribute('aria-label',receipt.label);}
 },1800);
}
