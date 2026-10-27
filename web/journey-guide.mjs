import {features, mechanicLessons, briefGuides} from './generated-content.mjs';
export {mechanicLessons} from './generated-content.mjs';
export {features} from './generated-content.mjs';
import {pipMarkup,guideMood} from './pip-view.mjs';
import {helpers} from './core.mjs';
import {featureTutorials} from './feature-tutorials.mjs';
import {gameIcon} from './game-art.mjs';
// Render the saved discovery badge for a newly available feature.
export function featureBadge(p,id){return featureUnlocked(p,id)&&!p.GuideSeen.includes(id)?'<span class="new-feature-badge" aria-label="New feature — tutorial available">!</span>':'';}
// Update feature badges after progress changes without rebuilding unrelated controls.
export function refreshUnlockBadges(p){for(const tool of document.querySelectorAll('[data-helper-intro]')){if(p.GuideSeen.includes('helper-'+tool.dataset.helperIntro))tool.querySelector('.new-feature-badge')?.remove();}for(const button of document.querySelectorAll('[data-feature]')){button.querySelector('.new-feature-badge')?.remove();const badge=featureBadge(p,button.dataset.feature);if(badge)button.insertAdjacentHTML('beforeend',badge);button.classList.toggle('new-feature',!!badge);}}
// Compare a feature's canonical unlock requirement with campaign progress.
export function featureUnlocked(p,id){const f=features.find(f=>f.id===id);if(!f)return false;if(id==='customize')return p.ChapterGifts100.length>0||(p.Customization?.Unlocked.length??1)>1;if(id==='garden'&&p.Coins<75)return true;return Math.max(p.Unlocked,p.HighestPlayedLevel)>=f.level;}
// Build available destinations and their current reward/readiness indicators.
export function journeyOptions(session){const e=session.events,a=e.a,options=[];
 // Append an authored point or option to the current local collection.
 const add=(id,label,badge,priority)=>{if(featureUnlocked(session.p,id)){const f=features.find(f=>f.id===id);options.push({...f,label,badge,priority});}};
 add('album','Sticker packs',a.Album.Packs.length,a.Album.Packs.length?100:10);
 const ready=e.missions.filter(g=>g.current>=g.target).length;add('missions','Picnic tasks',ready,ready?95:20);
 add('hunt','Souvenir Hunt',a.Hunt.Arrows,a.Hunt.Arrows?90:5);add('bingo','Bingo draws',a.Bingo.Tokens,a.Bingo.Tokens?75:4);
 add('balloon',a.Balloon.Active?'Flight '+a.Balloon.Stage+'/3':'Balloon Trail',a.Balloon.Active?'→':'',a.Balloon.Active?85:40);
 add('regatta','Regatta',a.Regatta.Distance+' mi',25);add('voyage','Monthly voyage',a.Pass.Wins+' wins',15);
 add('trail','Treasure trail',session.side.a.Rolls,session.side.a.Rolls?80:12);add('memory','Matching garden',session.side.a.Seeds,session.side.a.Seeds?78:11);add('customize','Card studio','',12);return options.sort((a,b)=>b.priority-a.priority);
}
// Render the logical destination groups and their unlock states for the travel menu.
export function journeyMarkup(session,limit=5){return journeyOptions(session).slice(0,limit).map(o=>`<button class="journey-link" data-action="${o.action}" title="${o.title}">${gameIcon(o.icon,30)}<span>${o.label}</span>${o.badge!==''?`<b>${o.badge}</b>`:''}</button>`).join('');}
export class BirdGuide {
 // Bind the current session and navigation callbacks while tracking deferred introductions.
 constructor(getSession,run){this.getSession=getSession;this.run=run;this.active=null;this.step=0;this.lessonIndex=0;this.deferred=new Set();this.host=document.createElement('aside');this.host.className='bird-guide';this.host.dataset.guideFrame='pip';this.host.setAttribute('aria-live','polite');this.host.setAttribute('aria-label','Pip’s guide');this.screen='map';}
 // Resolve the current session lazily so importing a save does not leave the guide on the old profile.
 get s(){return this.getSession();}
 // Offer context-appropriate guidance when entering a screen.
 check(screen){if(this.screen!==screen)this.clearMistakes();this.screen=screen;if(this.active==='welcome'&&screen==='play'||this.active==='cards'&&this.s.hand?.Won||this.step===1&&screen!=='gems'&&!document.querySelector('#dialog').open){if(featureTutorials[this.active])this.pause();else this.dismiss();}if(!this.active&&!document.querySelector('#dialog').open){const eligible=features.filter(f=>featureUnlocked(this.s.p,f.id)&&!this.s.p.GuideSeen.includes(f.id)&&!this.deferred.has(f.id)).sort((a,b)=>a.level-b.level);if(screen==='play'&&!this.s.hand?.Won){const kind=this.s.hand?.Deal.Cards.find(c=>c.Kind&&mechanicLessons[c.Kind]&&!this.s.p.GuideSeen.includes('mechanic'+c.Kind))?.Kind;if(kind)this.show('mechanic'+kind);}}this.paint();}
 // Select a guide topic and render its current lesson step.
 show(id){this.active=id;this.step=0;this.lessonIndex=0;this.paint();}
 // Find the next unlocked, unseen feature that Pip can introduce.
 offerUnlock(){const pending=features.filter(f=>f.id!=='welcome'&&featureUnlocked(this.s.p,f.id)&&!this.s.p.GuideSeen.includes(f.id));const f=this.s.lastReward?.chapterGift?pending.find(f=>f.id==='customize'):pending.find(f=>f.level===this.s.p.Unlocked);if(f){this.active=f.id;this.step=0;this.paint();}}
 // Count repeated rejected actions before offering Pip's contextual suggestion.
 mistake(kind,message){
  if(this.mistakeKind!==kind){this.mistakeKind=kind;this.mistakeCount=0;}
  this.mistakeCount=(this.mistakeCount??0)+1;
  if(this.mistakeCount<3||Date.now()-(this.lastMistakeTip??0)<12000||this.active&&this.active!=='mistake')return;
  this.lastMistakeTip=Date.now();this.mistakeCount=0;this.mistakeMessage=message;this.show('mistake');
 }
 // Reset repeated-error tracking after an accepted action.
 clearMistakes(){this.mistakeCount=0;this.mistakeKind=null;if(this.active==='mistake'){this.active=null;this.paint();}}
 // Resolve the active topic's explanation and available guide commands.
 details(){if(this.active==='mistake')return {title:'A little tip from Pip',text:this.mistakeMessage};if(this.active?.startsWith('helper-')){const h=helpers.find(h=>h.Id===this.active.slice(7));if(h)return {title:h.Name,text:h.Description,target:'[data-action="helper:'+h.Id+'"]',icon:h.Id};}if(this.active?.startsWith('mechanic')){const kind=+this.active.slice(8),lesson=mechanicLessons[kind];return lesson?{title:lesson[0],text:lesson[1],target:'.obstacle-'+kind}:null;}if(this.active==='cards')return {id:'cards',title:'One higher, one lower',text:'Pick an uncovered card one rank above or below the waste. Ace and King connect. Stuck? Draw a fresh card below the field.',target:'[data-action="draw"]',action:'guide',icon:'extra'};if(this.active==='help')return {title:'Where shall we wander?',text:'Choose a place below and I’ll show you how it connects to your cards.',icon:'journal'};return features.find(f=>f.id===this.active);}
 // Open the guide-selected destination through the normal navigation callbacks.
 async navigate(id){this.show(id);const f=this.details();if(f?.action&&(['welcome','cards'].includes(id)||featureUnlocked(this.s.p,id)))await this.go();}
 // Perform the currently suggested guided action rather than inventing a separate flow.
 async go(){const f=this.details();if(!f?.action||f.id&&!['welcome','cards'].includes(f.id)&&!featureUnlocked(this.s.p,f.id))return;this.step=1;await this.run(f.action);this.paint();}
 // Defer the active introduction and clear highlights when the player chooses another route.
 pause(){if(this.active)this.deferred.add(this.active);this.active=null;this.step=0;this.paint();}
 // Advance within the active lesson or finish its introduction.
 next(){const lessons=featureTutorials[this.active];if(lessons&&this.lessonIndex<lessons.length-1){this.lessonIndex++;this.paint();}else this.dismiss();}
 // Record an introduction as seen where appropriate, then remove its guide presentation.
 dismiss(){if(this.active&&!['help','mistake'].includes(this.active)&&!this.s.p.GuideSeen.includes(this.active)){this.s.p.GuideSeen.push(this.active);this.s.save();}this.active=null;refreshUnlockBadges(this.s.p);this.paint();}
 // Replace guide highlights and Pip's panel to match the current topic and visible screen.
 paint(){document.querySelector('.guide-hand')?.remove();document.querySelectorAll('.guide-highlight').forEach(e=>e.classList.remove('guide-highlight'));document.body.classList.toggle('coaching',!!this.active);if(this.screen==='home'&&!document.querySelector('#dialog').open){this.host.remove();document.body.classList.remove('coaching');return;}if(!this.active){this.host.remove();return;}const detail=this.details(),f=detail?{...detail}:null;if(!f){this.active=null;this.host.remove();return;}if(this.active==='cards'&&this.screen==='play'){const moves=this.s.hand?.Moves??[];f.target=moves.length?'[data-action="card:'+moves[0]+'"]':'[data-action="draw"]';}const lessons=featureTutorials[this.active],lesson=this.step&&lessons?lessons[this.lessonIndex]:null;if(lesson)f.target=lesson[1];const dialog=document.querySelector('#dialog'),root=dialog.open?(document.querySelector('#modal .dialog-content')??document.querySelector('#modal')):['play','gems'].includes(this.screen)?document.querySelector('#app'):document.body;
  if(dialog.open&&(root.querySelector('.level-preview')||(this.active!=='help'&&!root.querySelector(f.target??'.no-guide-target')))){this.host.remove();document.body.classList.remove('coaching');return;}
  // Navigation needs a scrollable choice list; short lessons keep their compact helper bay.
  this.host.dataset.guidePanel=this.active==='help'?'places':'lesson';
  this.host.classList.toggle('in-panel',dialog.open);this.host.classList.toggle('in-play',!dialog.open&&['play','gems'].includes(this.screen));this.host.innerHTML=`${pipMarkup(guideMood(this.active,this.step),88)}<div class="guide-copy"><small>PIP’S FIELD NOTES</small><strong>${f.title}</strong><p>${f.id&&!['welcome','cards'].includes(f.id)&&!featureUnlocked(this.s.p,f.id)?'Opens at level '+f.level:this.step?(this.active==='welcome'?'Press Play below to start your level.':lesson?lesson[0]:'Use the highlighted control to try it.'):(this.active==='cards'&&this.screen==='play'?(this.s.hand?.Moves.length?'Tap the pointed card: one higher or lower.':'No match? Tap the stock to draw.'):(this.active==='mistake'?f.text:briefGuides[this.active]??f.text.split('. ')[0]+'.'))}</p>${this.active==='help'?`<div class="guide-places"><button data-action="birdTopic:welcome" aria-label="Open current level">${gameIcon('play',30)}<span>Play</span></button><button data-action="birdTopic:cards" aria-label="Open card rules">${gameIcon('extra',30)}<span>Cards</span></button>${features.filter(f=>f.id!=='welcome'&&featureUnlocked(this.s.p,f.id)).map(f=>`<button data-action="birdTopic:${f.id}" title="${f.title}" aria-label="Open ${f.title}">${gameIcon(f.icon,30)}<span>${({garden:'Garden',missions:'Picnic',wheel:'Wheel',album:'Album',tournament:'Trophies',balloon:'Flight',hunt:'Hunt',bingo:'Bingo',regatta:'Regatta',voyage:'Voyage',travel:'Travel',chest:'Chest',postcard:'Postcards',tickets:'Trail',milestones:'Milestones',customize:'Studio',trail:'Treasure',memory:'Pairs'})[f.id]}</span></button>`).join('')}</div>`:''}<div class="guide-actions">${this.active?.startsWith('mechanic')?`<button class="info-button" data-ui-info="${this.active}" aria-label="More about ${f.title}">i</button>`:''}${f.action&&!(this.active==='cards'&&this.screen==='play')&&this.step===0&&(!f.id||['welcome','cards'].includes(f.id)||featureUnlocked(this.s.p,f.id))?'<button data-action="birdGo" class="primary">Show me</button>':''}${lesson?`<span class="tutorial-step">${this.lessonIndex+1} / ${lessons.length}</span><button class="primary" data-action="birdNext">${this.lessonIndex<lessons.length-1?'Next':'Got it'}</button>`:`<button data-action="birdDismiss">${this.step||!f.action?'Got it':'I’ll explore'}</button>`}</div></div>`;
  // The destination picker is its own popup; it must not lengthen or scroll the underlying book page.
  const mount=dialog.open&&this.active==='help'?dialog:root;
  if(mount&&this.host.parentElement!==mount){if(dialog.open&&this.active!=='help'||!dialog.open&&['play','gems'].includes(this.screen))mount.prepend(this.host);else mount.append(this.host);}// Reserve a tutorial bay above helpers; the tableau never changes size.
  document.body.style.removeProperty('--guide-bay-height');
  if(!dialog.open&&this.screen==='play'&&this.active!=='help'){
   const tools=root.querySelector('.tools'),shell=root.querySelector('.board-shell');
   if(tools&&shell){const body=document.body.getBoundingClientRect(),r=tools.getBoundingClientRect(),box=shell.getBoundingClientRect();this.host.style.left=(r.left-body.left)+'px';this.host.style.right='auto';this.host.style.top=(box.top-body.top+10)+'px';this.host.style.width=r.width+'px';document.body.style.setProperty('--guide-bay-height',(this.host.offsetHeight+8)+'px');}
  }else{for(const name of ['left','right','top','width'])this.host.style.removeProperty(name);}
  const target=f.target?(root.querySelector(f.target)??document.querySelector(f.target)):null;if(target&&target.getClientRects().length){target.classList.add('guide-highlight');if(this.active==='cards'&&this.screen==='play'&&!dialog.open){const r=target.getBoundingClientRect(),hand=document.createElement('span');hand.className='guide-hand';hand.setAttribute('aria-hidden','true');hand.textContent=String.fromCodePoint(0x1F447);hand.style.left=(r.x+r.width/2-17)+'px';hand.style.top=(r.y-35)+'px';document.body.append(hand);}if(dialog.open&&root.contains(target))target.scrollIntoView({block:'nearest',behavior:'instant'});}
 }
}
