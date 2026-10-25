import {gameIcon} from './game-art.mjs';
import {escapeHtml} from './ui-markup.mjs';

export function resourceBadge(value,icon,label){
 return `<span class="resource-badge" data-resource="${icon}" role="img" aria-label="${escapeHtml(label)}" title="${escapeHtml(label)}"><b>${escapeHtml(String(value))}</b>${gameIcon(icon,24)}</span>`;
}
export function swapHeartProgress(value){
 return `<span class="resource-goal" aria-label="${value} of 10 successful swaps toward 1 heart" title="10 successful swaps earn 1 heart">${resourceBadge(value+'/10','swap','Successful swaps: '+value+' of 10')}<span class="resource-arrow" aria-hidden="true">&#8594;</span>${resourceBadge(1,'heart','Reward: 1 heart')}</span>`;
}
const units={coin:'coins',heart:'heart',token:'token',arrow:'arrow',trophie:'trophy',star:'ratingStar',move:'swap',swap:'swap',roll:'dice',seed:'seed',pack:'pack',blossom:'flower',win:'trophy',draw:'bingo',mile:'helm',pair:'pairs'};
// Keep reward meanings in accessible labels, while replacing repetitive number/unit prose.
export function resourceTextParts(text){
 const pattern=/(?<![\w.])([+\u2212-]?\d[\d,]*(?:\/\d[\d,]*)?)\s+(?:shiny |extra |chapter |garden )?(coins?|hearts?|tokens?|arrows?|trophies|stars?|moves?|swaps?|rolls?|seeds?|packs?|blossoms?|wins?|draws?|miles?|pairs?)(?: (?:grown|left|available|gathered))?\b/gi;
 const parts=[];let from=0;
 for(const match of text.matchAll(pattern)){
  if(match.index>from)parts.push(text.slice(from,match.index));
  const unit=match[2].toLowerCase(),key=unit==='trophies'?'trophie':unit.replace(/s$/,'');
  parts.push({value:match[1],icon:units[key],label:match[0]});from=match.index+match[0].length;
 }
 if(from<text.length)parts.push(text.slice(from));return parts;
}
export function visualizeResources(root){
 if(!root)return;
 const walker=document.createTreeWalker(root,NodeFilter.SHOW_TEXT),nodes=[];
 while(walker.nextNode())nodes.push(walker.currentNode);
 for(const node of nodes){
  if(node.parentElement?.closest('.resource-badge,.resource-goal,.game-icon,script,style,textarea,option,[contenteditable],.card,.gem'))continue;
  const parts=resourceTextParts(node.textContent);if(!parts.some(p=>typeof p==='object'))continue;
  const fragment=document.createDocumentFragment();
  for(const part of parts){if(typeof part==='string')fragment.append(document.createTextNode(part));else{const template=document.createElement('template');template.innerHTML=resourceBadge(part.value,part.icon,part.label);fragment.append(template.content);}}
  node.replaceWith(fragment);
 }
}
export function watchResourceViews(roots){
 const observe=()=>roots.forEach(root=>observer.observe(root,{subtree:true,childList:true,characterData:true}));
 const observer=new MutationObserver(records=>{
  observer.disconnect();
  const changed=new Set(records.map(r=>r.target.nodeType===Node.TEXT_NODE?r.target.parentElement:r.target));
  changed.forEach(root=>{if(root?.isConnected)visualizeResources(root)});observe();
 });
 roots.forEach(visualizeResources);observe();return observer;
}
