const target={thaw:1,keypass:3,shield:4,prune:9,phase:13,lucky:11};
export const discovered=(p,id)=>id==='scout'?p.HighestPlayedLevel>=35:id==='peek'?p.HighestPlayedLevel>=50:target[id]!=null?p.SeenMechanics.includes(target[id]):true;
export const relevant=(hand,id)=>!hand||target[id]==null||id==='lucky'||hand.Deal.Cards.some((c,i)=>!hand.Removed[i]&&c.Kind===target[id]&&(id!=='thaw'||c.Strength>hand.s.Clears));
