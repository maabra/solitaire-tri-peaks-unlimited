import {gameIcon} from './game-art.mjs';
// Render a clearly bounded progress indicator from the supplied current and target values.
export function progressView(value,target,label,icon='targetGoal'){
 const max=Math.max(1,Number(target)||1),current=Math.min(max,Math.max(0,Number(value)||0));
 return `<div class="goal-progress" data-progress="${label}" aria-label="${label}: ${current} of ${max}">${gameIcon(icon,25)}<div><span>${label}</span><strong>${current.toLocaleString()} / ${max.toLocaleString()}</strong><progress value="${current}" max="${max}" aria-label="${label}"></progress></div></div>`;
}
