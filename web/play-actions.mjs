import {gameIcon} from './game-art.mjs';
import {actionButton as button} from './ui-markup.mjs';
// Describe hint availability and cooldown using the hand's current timer state.
export function hintLabel(seconds=0){return `${gameIcon('hint',23)}<span>${seconds>0?'Hint in '+Math.ceil(seconds)+'s':'Hint · 3s'}</span>`;}
// Render play commands with their current availability and costs.
export function playActions(hand){
 // Render an icon-and-caption play command using the common action-button helper.
 const control=(icon,label,action,attrs='')=>button(`${gameIcon(icon,23)}<span>${label}</span>`,action,'',attrs);
 return control('undo',hand.FreeUndos?'Undo · '+hand.FreeUndos:'Undo · 15 coins','undo',hand.CanUndo?'':'disabled')+
 button(hintLabel(),'hint','','id="hint-button"')+
 control('restart','Replay','replay')+control('bag','Shop','shop')+
 control('pipGuide','Pip','birdHelp')+control('compass','Activities','events')+
 control('mapRoute','Map','leaveLevel');
}
