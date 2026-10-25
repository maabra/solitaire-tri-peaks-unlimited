import {gameIcon} from './game-art.mjs';
import {extraWinRewards} from './win-rewards.mjs';
import {escapeHtml} from './ui-markup.mjs';

// All values are already saved by Session. These chips only explain the win.
export function winSummary(hand,reward={}){
 const extras=extraWinRewards(reward);
 return `<div class="result-stats"><div class="result-score"><small>${gameIcon('trophy',24)} Score</small><strong>${hand.Score.toLocaleString()}</strong></div><div class="result-total"><small>${gameIcon('coins',26)} Reward</small><strong>+<b data-result-coins>${reward.coins??0}</b></strong></div></div>${extras.length?`<div class="result-prize-chips" aria-label="Extra prizes earned">${extras.map(prize=>`<span title="${escapeHtml(prize.label)}">${gameIcon(prize.icon,29)}<b>${escapeHtml(String(prize.amount??1))}</b></span>`).join('')}</div>`:''}`;
}
