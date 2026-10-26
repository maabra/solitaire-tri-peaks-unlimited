import {RankShuffle} from './rank-shuffle.mjs';
import {storyLayoutV4,storyNameV4} from './story-layouts-v4.mjs';
import {campaignProgression} from './progression.mjs';
import {storyLayoutV3,storyNameV3} from './story-layouts-v3.mjs';
import {Hand,generate,SeededRandom,K,clone,adjacent} from './core.mjs';
export const specialGoal=level=>level%75===0?{name:'Clear all gold cards',suit:null,gold:true}:level%40===0?{name:'Clear all diamond cards',suit:2}:level%60===0?{name:'Clear all spade cards',suit:0}:null;
export function adventureDeal(seed,level,balloon=false,generation="MP21"){for(let attempt=0;attempt<16;attempt++){try{return buildAdventureDeal((seed+attempt*7919)%2147483647,level,balloon,generation)}catch(error){if(attempt===15)throw error;}}}
function buildAdventureDeal(seed,level,balloon=false,generation="MP21"){
 const modern=generation==='MP26',pacing=['MP24','MP26'].includes(generation)?campaignProgression(level):null;if(modern){pacing.MultiDeck=true;pacing.DrawInterval=level===1?1000:level<5?9:level<100?8:5;}const ruleLevel=pacing?.rulesLevel??level;const d=generate(seed,level,'MP19',modern?storyLayoutV4(level):['MP22','MP24'].includes(generation)?storyLayoutV3(level):null,pacing),rng=new SeededRandom(seed+71);d.Generation=generation;if(modern){d.Formation=storyNameV4(level);const draws=d.Solution.filter(s=>s.Index<0).length;d.Stock=d.Stock.slice(0,Math.max(draws,(level<5?8:20)-pacing.difficulty.ReserveTrim));}if(['MP22','MP24'].includes(generation))d.Formation=storyNameV3(level);d.AdventureSeed=seed;const originalSolution=clone(d.Solution),originalRanks=d.Cards.map(c=>c.Rank);d.AdventureRules=true;d.Objective=specialGoal(level);d.Balloon=balloon;
 d.Cards=d.Cards.map((c,i)=>({...c,Pack:level>=15&&i===d.Solution.filter(x=>x.Index!=null).at(-1)?.Index?1+Math.min(4,Math.floor(level/300)%5):0,Trophy:level>=25&&i%13===7?20+10*(i%4):0,Arrow:balloon&&i%7===3?1:0,Gold:d.Objective?.gold&&i%5===2||false,Motion:ruleLevel>=400&&[K.Up,K.Down].includes(c.Kind)?c.Kind===K.Up?1:-1:0,Kind:ruleLevel>=1100&&i%23===11&&c.Kind===0?15:ruleLevel>=1400&&i%19===9&&c.Kind===0?16:c.Kind}));
 d.GoalCards=d.Cards.map(c=>d.Objective?(d.Objective.gold?c.Gold:c.Suit===d.Objective.suit):true);
 if(d.Objective)d.Cards.forEach((c,i)=>c.Goal=d.GoalCards[i]);
 // Ice now has two matching interactions, independent of unrelated clears.
 for(const c of d.Cards)if(c.Kind===K.Ice)c.Strength=1;
 // Construct and replay a witness under the actual mutation rules. Unrelated
 // chapters and earlier MP16/18/19 deals keep their original generation.
 const h=new AdventureHand(d),path=[],bombs=new Map();
 for(let i=0;i<d.Cards.length;i++)if(d.Cards[i].Kind===K.Bomb){bombs.set(i,199);d.Cards[i].Strength=199;}
 for(const step of originalSolution){if(h.Won)break;if(step.Index<0){if(!h.draw())throw Error('Invalid planned draw.');h.tick(2);path.push({Index:-1});continue;}
  const i=step.Index,c=d.Cards[i],desired=originalRanks[i],motion=c.Motion??0,steps=h.s.MotionSteps[i]??0,source=h.s.FaceSource[i];
  const wrap=n=>(n%13+13)%13+1;
  if(source)d.Cards[source.Index].TornadoFaces[i].Rank=wrap(desired-1-motion*(steps-source.Steps));
  else c.Rank=wrap(desired-1-motion*steps);
  h.s.Faces[i]={Rank:desired,Suit:h.face(i).Suit};
  if(c.Kind===16){if(!h.s.Turns)c.Kind=K.Normal;else c.FogParity=h.s.Turns%2;}
  if(c.Kind===K.Vine&&h.s.Run<Math.max(2,c.Strength))c.Kind=K.Normal;
  if(c.Kind===K.Gate)c.Strength=h.s.Turns%2;
  if(c.Kind===K.ColorSeal)c.Strength=[1,2].includes(h.Waste.Suit)?1:0;
  if(c.Kind===K.Ice&&!h.s.IceThawed.includes(i)){if(!h.play(i))throw Error('Invalid planned ice hit.');path.push({Index:i});}
  if(!h.play(i))throw Error('Invalid planned mutation at '+i+' kind '+c.Kind+' turn '+h.s.Turns+' run '+h.s.Run+' rank '+h.face(i).Rank+' waste '+h.Waste.Rank+' keys '+h.s.Keys+' failed '+h.Failed);path.push({Index:i});
  if(bombs.has(i)&&h.Removed[i])bombs.set(i,h.s.Turns+(pacing?.difficulty.BombSlack??3));
 }
 for(const [i,fuse]of bombs)d.Cards[i].Strength=Math.min(199,fuse);
 d.Solution=path;d.WitnessReady=true;const verified=new AdventureHand(d);for(const step of path){if(step.Index<0){if(!verified.draw())throw Error('Draw witness failed.');verified.tick(2);}else if(!verified.play(step.Index))throw Error('Mutation witness failed at '+step.Index+' turn '+verified.s.Turns+' rank '+verified.face(step.Index).Rank+' waste '+verified.Waste.Rank+' kind '+verified.face(step.Index).Kind+' failed '+verified.Failed);}if(!verified.Won)throw Error('Mutation deal has no verified finish.');
 return d;
}
export class AdventureHand extends Hand {
 constructor(deal,run=null){super(deal,run);this.s.Faces??={};this.s.MotionSteps??={};this.s.FaceSource??={};this.s.IceThawed??=[];this.s.ExposedAt??={};this.s.Earned??={Packs:[],Trophies:0,Arrows:0};this.watchExposure();}
 face(i){const c={...this.Deal.Cards[i],...this.s.Faces[i]};c.TrophyTurns=c.Trophy?Math.max(0,5-(this.s.Turns-(this.s.ExposedAt?.[i]??this.s.Turns))):0;return c;}
 watchExposure(){for(let i=0;i<this.Removed.length;i++)if(this.isExposed(i)&&this.s.ExposedAt[i]==null)this.s.ExposedAt[i]=this.s.Turns;}

 allows(c){if(c.Kind===K.Ice)return true;if(c.Kind===16)return this.s.Turns>0&&this.s.Turns%2===(c.FogParity??1);return super.allows(c);}
 can(i){return i>=0&&i<this.Removed.length&&!this.Won&&!this.Failed&&this.isExposed(i)&&this.allows(this.face(i))&&adjacent(this.Waste.Rank,this.face(i).Rank);}
 advanceMove(played){super.advanceMove(played);for(let i=0;i<this.Removed.length;i++)if(!this.Removed[i]&&this.isExposed(i)&&this.Deal.Cards[i].Motion){const c=this.face(i),step=this.Deal.Cards[i].Motion;this.s.Faces[i]={Rank:(c.Rank-1+step+13)%13+1,Suit:c.Suit};this.s.MotionSteps[i]=(this.s.MotionSteps[i]??0)+1;}this.watchExposure();}
 collect(i){const c=this.Deal.Cards[i];if(c.Pack)this.s.Earned.Packs.push(c.Pack);if(c.Trophy&&this.s.Turns-(this.s.ExposedAt[i]??this.s.Turns)<5)this.s.Earned.Trophies+=c.Trophy;if(c.Arrow)this.s.Earned.Arrows+=c.Arrow;}
 fogVisible(i){const c=this.face(i);return c.Kind!==16||this.s.Turns>0&&this.s.Turns%2===(c.FogParity??1);}
 tornado(source){const rng=new SeededRandom(this.s.Turns*7919+this.Deal.AdventureSeed+43);const map=this.Deal.Cards[source].TornadoFaces??={};for(let i=0;i<this.Removed.length;i++)if(this.isExposed(i)){map[i]??={Rank:rng.next(13)+1,Suit:rng.next(4)};this.s.Faces[i]={...map[i]};this.s.FaceSource[i]={Index:source,Steps:this.s.MotionSteps[i]??0};}const first=this.Deal.Cards.findIndex((c,i)=>this.isExposed(i)&&this.allows(this.face(i)));if(first>=0&&!this.Deal.WitnessReady){map[first].Rank=this.Waste.Rank%13+1;this.s.Faces[first]={...map[first]};}}

 play(i){if(!this.can(i))return false;const c=this.face(i);if(c.Kind===K.Ice&&!this.s.IceThawed.includes(i)&&this.s.ThawSeconds<=0){this.remember();this.s.IceThawed.push(i);this.advanceMove(null);this.s.LastEffect='thaw';return true;}const original=this.Deal.Cards[i];this.Deal.Cards[i]={...c,Kind:c.Kind===K.Ice?K.Normal:c.Kind};const ok=super.play(i);this.Deal.Cards[i]=original;if(!ok)return false;if(this.Removed[i]){this.collect(i);this.Waste={...c};if(c.Kind===15){this.tornado(i);this.s.LastEffect='tornado';}if(this.Streak>0&&this.Streak%5===0){for(let n=0;n<2&&this.Stock.length<100;n++)this.Stock.push({Rank:(c.Rank+n+3)%13+1,Suit:(c.Suit+n)%4,Kind:0,Strength:0,TemporaryStock:false});this.s.LastEffect='streak';}}return true;}
 use(id,target=null,seed=42){if(id==='shuffle')return RankShuffle.apply(this,seed);if(!['tnt','joker','remove'].includes(id)){const turns=this.s.Turns,before=clone(this.s),history=this.history,ok=super.use(id,target,seed);if(ok&&this.s.Turns===turns)this.advanceMove(null);if(ok&&id==='scout'&&!this.Moves.length){this.s=before;this.history=history;return false;}return ok;}if(this.Won||this.Failed||this.s.HelperCooldown>0)return false;const indices=id==='tnt'?this.Deal.Cards.map((c,i)=>i).filter(i=>this.isExposed(i)):[target];if(!indices.length||indices.some(i=>!Number.isInteger(i)||!this.isExposed(i)))return false;this.history=[];for(const i of indices){this.collect(i);this.Removed[i]=true;this.Score=Math.min(2147483647,this.Score+20);const c=this.face(i);if(c.Kind===K.Key){this.s.Keys++;this.s.HasKey=true;}if(c.Kind===K.Gift)for(let n=0;n<3&&this.Stock.length<100;n++)this.Stock.push({Rank:(c.Rank+n+3)%13+1,Suit:(c.Suit+n)%4,Kind:0,Strength:0,TemporaryStock:false});this.onClear(c);}if(id==='joker')this.Waste=this.face(target);this.s.Clears+=indices.length;this.advanceMove(null);this.s.LastEffect=id;return true;}
 badge(i){const c=this.face(i);let text=c.Kind===K.Ice?'❄'+(this.s.IceThawed.includes(i)||this.s.ThawSeconds>0?'✓':'1'):c.Kind===15?'🌀':c.Kind===16?(this.fogVisible(i)?'\u{1f441}':'\u2601'):c.Motion?(c.Motion>0?'↑':'↓'):super.badge(i);return text;}
}
// Suit and gold objectives end as soon as their marked cards are cleared.
Object.defineProperty(AdventureHand.prototype,'Won',{get(){const g=this.Deal.Objective;if(!g)return this.Removed.every(Boolean);return this.Deal.Cards.every((c,i)=>this.Removed[i]||!(this.Deal.GoalCards?.[i]??(g.gold?c.Gold:c.Suit===g.suit))); }});
