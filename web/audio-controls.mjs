import {gameIcon} from './game-art.mjs';
// Render separate music and effects controls using the current saved volume values.
export function volumeControlsMarkup(profile){
 return `<div class="audio-sliders">${[['EffectsVolume','Effects volume',.65,'sound'],['MusicVolume','Music volume',.35,'music']].map(([key,label,fallback,icon])=>{
 const value=Math.round((profile[key]??fallback)*100);
 return `<label class="volume-control" for="volume-${key}"><span class="volume-label">${gameIcon(icon,28)}${label}</span><output for="volume-${key}">${value}%</output><input id="volume-${key}" aria-label="${label}" aria-valuetext="${value} percent" type="range" min="0" max="100" step="1" value="${value}" style="--volume:${value}%" data-audio-volume="${key}"></label>`;
 }).join('')}</div>`;
}
// Normalize a slider change into the bounded volume value stored in the profile.
export function updateVolumeControl(input){
 const percent=Math.max(0,Math.min(100,Number(input.value)||0));
 input.style.setProperty('--volume',percent+'%');input.setAttribute('aria-valuetext',percent+' percent');
 const output=input.closest('.volume-control')?.querySelector('output');if(output)output.textContent=percent+'%';
 return percent/100;
}
