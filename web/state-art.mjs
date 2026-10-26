import {chestTiers} from './chest-tiers.mjs';
import {atlasIcon} from './atlas-icon.mjs';
export function chestArt(tier=0,opened=false,size=80){const i=Math.max(0,Math.min(5,tier)),cell=i+(opened?6:0);return atlasIcon('chest-states.png',cell,3,4,1214,1295,size,'chest-art chest-painted',`role="img" aria-label="${chestTiers[i].name} chest, ${opened?'opened':'closed'}"`);}
