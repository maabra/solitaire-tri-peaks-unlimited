import {environments} from './chapter-scene.mjs';
import {chapterName} from './chapter-worlds.mjs';
import {modeFor} from './mode-scenes.mjs';
import {gameIcon} from './game-art.mjs';

export function surfaceChoices(session){return `<div class="surface-choices" role="group" aria-label="Play surface">${[['scene','travel','Scenery'],['table','studio','Card table']].map(([id,icon,label])=>`<button data-action="surface:${id}" aria-pressed="${session.p.Customization.Surface===id}">${gameIcon(icon,26)} ${label}</button>`).join('')}</div>`;}

export function scenicSurface(session){return session.p.Customization.Surface==='scene';}
export function applyPlaySurface(session){
 const scenic=scenicSurface(session);document.body.classList.toggle('scenic-play',scenic);
 if(!scenic||modeFor(session))return;
 const chapter=Math.ceil(session.level/100),name=chapterName(chapter).toLowerCase();
 const themes=[[/snow|frost|winter|ice|aurora/,1],[/autumn|amber|harvest/,2],[/moon|night|star/,3],
  [/lavender|violet/,4],[/tropic|coral|island|coast|pearl|lagoon/,5],[/desert|dune|oasis/,6],
  [/cherry|blossom|sakura/,7],[/forest|wood|mushroom/,8],[/festival|carnival/,9]];
 const tile=themes.find(([pattern])=>pattern.test(name))?.[1]??(chapter-1)%environments.length;
 document.querySelector('.landscape').style.backgroundImage=`linear-gradient(#102e3214,#102e3245),url('assets/${environments[tile]}.png')`;
}
