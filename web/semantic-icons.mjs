import {paintedIcon,paintedIcons} from './painted-icons.mjs';
export const semanticIconNames=Object.freeze(Object.keys(paintedIcons));
// Map a semantic UI symbol to its generated-art counterpart.
export function semanticIcon(name,size=32){return paintedIcon(name==='unknown'?'ruleBook':name,size);}
