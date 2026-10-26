import {shapes} from './generated-content.mjs';
// Original vector symbols: each identifier has one meaning throughout the UI.
export const semanticIconNames=Object.freeze(Object.keys(shapes));
export function semanticIcon(name,size=32){if(!shapes[name])return null;return `<span class="game-icon semantic-icon" data-icon="${name}" aria-hidden="true" style="--icon-size:${size}px"><svg viewBox="0 0 64 64" fill="none" stroke="#77516c" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">${shapes[name]}</svg></span>`;}
