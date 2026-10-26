import {gameIcon} from './game-art.mjs';
export function goalProgress(hand){
 const goals=hand.Deal.GoalCards??hand.Removed.map(()=>true);
 const total=goals.filter(Boolean).length,cleared=goals.filter((goal,i)=>goal&&hand.Removed[i]).length;
 return {cleared,total,icon:hand.Deal.Objective?.gold?'coins':hand.Deal.Objective?.suit===2?'ruby':hand.Deal.Objective?.suit===0?'seal':'cardProgress'};
}
export function handProgress(hand){const p=goalProgress(hand);return `${gameIcon(p.icon,22)} <span>Cleared</span> <b>${p.cleared} / ${p.total}</b>`;}
export function handStatus(hand){
 if(hand.Won)return `${gameIcon('check',22)} All goals complete!`;
 if(hand.Failed)return `${gameIcon('bomb',22)} Try Undo or Replay`;
 if(!hand.Moves.length)return hand.Stock.length?`${gameIcon('extra',22)} No match? Draw from the pile below.`:`${gameIcon('bag',22)} No cards left? Try a helper, or Replay.`;
 return hand.Waste.Rank===0?`${gameIcon('wild',22)} Any rank on a Wild!`:'';
}
