import {gameIcon} from './game-art.mjs';
import {pipMarkup} from './pip-view.mjs';
import {escapeHtml} from './ui-markup.mjs';
import {resourceBadge} from './resource-view.mjs';

export function wheelView(session,busy=false,lastPrize=''){
 const remaining=Math.max(0,session.events.a.WheelAt-session.now());
 const cooldown=remaining?(remaining>=3600000?Math.ceil(remaining/3600000)+'h':Math.ceil(remaining/60000)+'m'):'Ready';
 return `<section class="wheel-panel"><h2>Lucky meadow wheel</h2><div class="prize-wheel">${[['coins',25],['wild',1],['coins',50],['extra',1],['coins',100],['remove',1],['coins',75],['coins',40]].map(([name,count],i)=>`<span style="--segment:${i}">${gameIcon(name,38)}<small>${count}</small>${i===5?'<em class="wheel-heart">'+gameIcon('heart',20)+'+1</em>':''}</span>`).join('')}${Array.from({length:8},(_,i)=>`<i class="wheel-lamp" style="--bulb:${i}" aria-hidden="true"></i>`).join('')}<b aria-hidden="true">${gameIcon('flower',30)}</b><i class="wheel-needle" aria-hidden="true"></i></div><div class="wheel-controls"><div class="wheel-pip">${pipMarkup('surprise',80)}</div><p id="wheel-result" role="status">${busy?'Spinning…':lastPrize?'Last prize: '+escapeHtml(lastPrize):'A little spin, a lovely surprise.'}</p><button class="story-play" data-action="spinWheel" ${busy||!session.wheelSpins?'disabled':''}>${gameIcon('wheel',28)} Spin · ${session.wheelSpins}</button><button class="primary" data-action="timedWheel" ${busy||remaining?'disabled':''}>${gameIcon('clock',25)} Daily spin · ${cooldown}</button><p class="meta">${resourceBadge(5,'trophy','Five campaign wins')} → 1 spin<br>Coins, helpers and a little luck!</p></div></section>`;
}
