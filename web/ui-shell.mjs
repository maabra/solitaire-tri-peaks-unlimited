import {explanations, shortRules} from './generated-content.mjs';
import {activityGroup} from './activity-groups.mjs';
import {decorateMenuScene} from './menu-scenes.mjs';
import {visualizeMenu} from './visual-cues.mjs';
import {gameIcon,kindIcon} from './game-art.mjs';
import {features,mechanicLessons} from './journey-guide.mjs';
import {helpers} from './core.mjs';

export function infoButton(topic,label='How this works'){return `<button type="button" class="info-button" data-ui-info="${topic}" aria-label="${label}" title="${label}">i</button>`;}
function helpData(topic){if(topic.startsWith('mechanic')){const kind=Number(topic.slice(8)),lesson=mechanicLessons[kind];if(lesson)return [lesson[0],lesson[1],'Watch the badge on this card.',kindIcon[kind]];}if(topic==='overview')topic='hub';if(topic.startsWith('helper-')){const h=helpers.find(h=>h.Id===topic.slice(7));if(h)return [h.Name,h.Description,'Buy with coins in the helper shop. Each use consumes one; effects are temporary.',h.Id];}const f=features.find(f=>f.id===topic);return explanations[topic]??(f?[f.title,f.text,'Continue playing cards to earn progress, then return here to collect or use it.',f.icon]:explanations.default);}
function illustratedSteps(topic,icon){const paths={cards:[['cardProgress','Draw'],['play','One up / down'],['ratingStar','Build a streak']],gems:[['gems','Match 3'],['coins','Earn coins'],['bag','Choose helpers']],garden:[['gems','Match 3'],['coins','Earn coins'],['bag','Choose helpers']],hunt:[['arrow','Use an arrow'],['gift','Reveal a tile'],['pack','Keep the prize']],bingo:[['play','Win hands'],['bingo','Draw numbers'],['gift','Complete a line']],regatta:[['play','New wins'],['helm','Move your boat'],['gift','Reach rewards']],tournament:[['trophy','Collect badges'],['play','Win the hand'],['gift','Claim rewards']],balloon:[['balloon','Start a flight'],['play','Win 3 hands'],['arrow','Earn arrows']],pack:[['pack','Open a pack'],['album','Find stickers'],['token','Trade duplicates']],tokenShop:[['album','Duplicates'],['token','Earn tokens'],['pack','Choose a pack']]};let steps=paths[topic];if(topic==='mechanic1')steps=[['play','First match'],[icon,'Thaw'],['play','Match again']];if(topic==='mechanic3')steps=[['key','Collect keys'],['lock','Open locks'],['play','Collect the card']];return steps?`<div class="help-steps">${steps.map(([art,label],i)=>`<div class="help-step">${gameIcon(art,43)}<strong>${label}</strong></div>${i<steps.length-1?'<span class="help-step-arrow" aria-hidden="true">›</span>':''}`).join('')}</div>`:'';}
export function installHelp(){
 const help=document.createElement('dialog');help.id='context-help';help.className='context-help';help.setAttribute('aria-labelledby','help-title');document.body.append(help);
 document.addEventListener('click',e=>{const info=e.target.closest('[data-ui-info]');if(info){e.preventDefault();e.stopImmediatePropagation();const [title,text,flow,icon]=helpData(info.dataset.uiInfo);help.innerHTML=`<button class="help-close" data-ui-dismiss aria-label="Close explanation">×</button><div class="help-art">${gameIcon(icon,100)}<span class="help-spark" aria-hidden="true">✦</span></div><h2 id="help-title">${title}</h2>${illustratedSteps(info.dataset.uiInfo,icon)}<p class="help-rule">${shortRules[info.dataset.uiInfo]??text}</p><button class="primary help-done" data-ui-dismiss>${gameIcon('check',25)} Got it</button>`;if(!help.open)help.showModal();help.querySelector('.help-done').focus();return;}if(e.target.closest('[data-ui-dismiss]'))help.close();},true);
 help.addEventListener('click',e=>{if(e.target===help){const r=help.getBoundingClientRect();if(e.clientX<r.left||e.clientX>r.right||e.clientY<r.top||e.clientY>r.bottom)help.close();}});
}
export function frameModal(modal){
 modal.closest('dialog').querySelector(':scope > .close')?.remove();
 // Compact notices share the level board; roomy menus share a nine-slice board.
 const compact=modal.querySelector('.level-preview,.victory,.locked-feature-preview')||modal.querySelector('.win-gifts')&&modal.querySelectorAll('.earned-gift').length<=4||!modal.querySelector('.event-panel,.shop,.wheel-panel,.lives-panel,.guide-grid,.favorite-list,[data-setting]')&&modal.textContent.length<500&&modal.querySelectorAll('input,button').length<=4;
 modal.closest('dialog').dataset.frame=compact?'notice':'menu';
 const event=modal.querySelector('.event-panel'),classes=event?.className??'';
 const topic=modal.querySelector('[data-help-topic]')?.dataset.helpTopic??(event?classes.split(' ').find(c=>c.startsWith('event-')&&c!=='event-panel')?.slice(6)??'hub':modal.querySelector('.shop')?'shop':modal.querySelector('.wheel-panel')?'wheel':modal.querySelector('[data-setting]')?'settings':modal.querySelector('.favorite-list')?'journal':modal.querySelector('.guide-grid')?'guide':modal.querySelector('.level-preview')?'cards':'default');
 const heading=modal.querySelector('h2');if(heading){heading.classList.remove('panel-ribbon');const wrap=document.createElement('div');wrap.className='panel-heading';heading.before(wrap);wrap.innerHTML=`<span class="heading-art">${gameIcon(modal.querySelector('.lives-panel')?'heart':modal.querySelector('.victory')?'ratingStar':helpData(topic)[3],48)}</span>`;wrap.append(heading);if(!modal.querySelector('.locked-feature-preview')&&['hunt','bingo','regatta','balloon','tournament','tickets','pack','tokenShop'].includes(topic))wrap.insertAdjacentHTML('beforeend',infoButton(topic));}
 modal.querySelector('.event-back')?.remove();
 const tabs=event?.querySelector(':scope > .activity-tabs'),title=event?.querySelector(':scope > .panel-heading');if(tabs&&title)title.after(tabs);
 const content=document.createElement('div');content.className='dialog-content';while(modal.firstChild)content.append(modal.firstChild);modal.append(content);
 const nav=document.createElement('nav');nav.className='menu-navigation';nav.setAttribute('aria-label','Game places');nav.innerHTML=modal.querySelector('[data-action="backMap"],.victory,.win-gifts')?'':'<button data-action="backMap" class="menu-map-back">Back to map</button>';nav.insertAdjacentHTML('beforeend','<button class="close shell-close" data-action="close" aria-label="Close menu">×</button>');modal.prepend(nav);
 const group=activityGroup(topic);if(group){const parent=document.createElement('button');parent.dataset.action='events';parent.className='menu-parent';parent.textContent='Adventures';nav.prepend(parent);const location=document.createElement('span');location.className='menu-location';location.textContent='Adventures / '+group[0];nav.append(location);}

 // Explanations belong beside the item they describe, without nesting buttons.

 modal.querySelectorAll('.event-hub .event-tile,.album-category,.reward-track>article,.skin-card').forEach((el,i)=>{el.style.setProperty('--item-delay',Math.min(i,10)*25+'ms');el.classList.add('menu-arrival');});
 for(const button of modal.querySelectorAll('[data-action^="ev:"]')){const id=button.dataset.action.slice(3);if(features.some(f=>f.id===id))button.dataset.feature=id;}visualizeMenu(modal);decorateMenuScene(modal,topic);
 modal.closest('dialog').scrollTop=0;
}
