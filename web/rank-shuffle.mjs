import {SeededRandom,adjacent} from './core.mjs';

// Ranks change; card identities, cover links, keys and earned-prize badges do not.
export class RankShuffle{
 static available(hand){return !!hand?.Deal.AdventureRules&&!hand.Won&&!hand.Failed&&hand.Waste.Rank!==0&&
  hand.Removed.filter(v=>!v).length>1&&hand.Deal.Cards.some((_,i)=>hand.isExposed(i)&&hand.allows(hand.face(i)));}
 static apply(hand,seed){
  if(!this.available(hand)||hand.s.HelperCooldown>0)return false;
  const rng=new SeededRandom(seed),indices=hand.Removed.flatMap((removed,i)=>removed?[]:[i]);
  const ranks=indices.map(i=>hand.face(i).Rank);
  for(let i=ranks.length-1;i>0;i--){const j=rng.next(i+1);[ranks[i],ranks[j]]=[ranks[j],ranks[i]];}
  const faces=Object.fromEntries(indices.map((i,n)=>[i,{Rank:ranks[n],Suit:hand.face(i).Suit}]));
  const candidates=indices.filter(i=>hand.isExposed(i)&&hand.allows({...hand.face(i),...faces[i]}));
  if(!candidates.length)return false;
  if(!candidates.some(i=>adjacent(hand.Waste.Rank,faces[i].Rank)))faces[candidates[rng.next(candidates.length)]].Rank=hand.Waste.Rank%13+1;
  hand.s.Faces={...hand.s.Faces,...faces};hand.s.FaceSource={};hand.history=[];
  hand.s.LastEffect='shuffle';return true;
 }
}
