import {campaign} from './generated-content.mjs';
// Campaign pacing is independent of card rules and older deal generations.
export const FULL_FEATURE_LEVEL=campaign.fullFeatureLevel;
export const mechanicUnlocks=Object.freeze(campaign.mechanicUnlocks);
const anchors=campaign.anchors;
// Interpolate authored pacing anchors into the older rule-level scale. This lets the
// shorter campaign introduce every mechanic while preserving the existing rule machinery.
export function rulesLevel(level){const n=Math.min(FULL_FEATURE_LEVEL,Math.max(1,level));for(let i=1;i<anchors.length;i++){const [b,y]=anchors[i],[a,x]=anchors[i-1];if(n<=b)return Math.floor(x+(y-x)*(n-a)/(b-a));}return 2400;}
export function campaignProgression(level){const n=Math.min(FULL_FEATURE_LEVEL,Math.max(1,level)),tier=Math.floor((Math.max(1,level)-1)/100),eligible=[5,1,3,4,6,8,10,11,9,12,15,16,13,14].filter(k=>mechanicUnlocks[k]<=n),focus=eligible.length?eligible[tier%eligible.length]:0;return {rulesLevel:rulesLevel(n),difficulty:{Badge:'✦ '+(tier+1),Tier:tier,Focus:focus,HazardBudget:Math.min(10,1+Math.floor(n/1000)),IceLead:Math.max(0,3-Math.floor(n/3200)),KeysRequired:Math.min(3,1+Math.floor(n/4000)),BombSlack:Math.max(2,6-Math.floor(n/2500)),ArmorHits:Math.min(4,2+Math.floor(n/5000)),VineRun:Math.min(3,2+Math.floor(n/8000)),ReserveTrim:Math.min(8,Math.floor(n/1250)+(level>=10&&level%10===0?(level%20===0?2:1):0))}};}
