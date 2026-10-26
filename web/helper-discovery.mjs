import {campaign} from './generated-content.mjs';
import {RankShuffle} from './rank-shuffle.mjs';
const target={thaw:1,keypass:3,shield:4,prune:9,phase:13,lucky:11};
export const discovered=(p,id)=>id==='shuffle'?p.HighestPlayedLevel>=campaign.shuffleUnlock:id==='tnt'?p.HighestPlayedLevel>=75:id==='joker'?p.HighestPlayedLevel>=10:id==='scout'?p.HighestPlayedLevel>=35:id==='peek'?p.HighestPlayedLevel>=50:target[id]!=null?p.SeenMechanics.includes(target[id]):true;
export const relevant=(hand,id)=>id==='shuffle'?!hand||RankShuffle.available(hand):!hand||(['tnt','joker'].includes(id)?!!hand.Deal.AdventureRules:target[id]==null)||id==='lucky'||hand.Deal.Cards.some((c,i)=>!hand.Removed[i]&&c.Kind===target[id]&&(id!=='thaw'||(hand.Deal.AdventureRules?!hand.s.IceThawed.includes(i):c.Strength>hand.s.Clears)));
