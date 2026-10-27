import {gameIcon} from './game-art.mjs';
import {pipMarkup} from './pip-view.mjs';
import {escapeHtml} from './ui-markup.mjs';
import {resourceBadge} from './resource-view.mjs';

// Share the countdown label with the live spin controls so a consumed daily spin never still says Ready.
export function wheelCooldownText(remaining){return remaining>0?(remaining>=3600000?Math.ceil(remaining/3600000)+'h':Math.ceil(remaining/60000)+'m'):'Ready';}
// Render the wheel and its prize labels from the same prize ordering used by the reward logic.
export function wheelView(session,busy=false,lastPrize='',lastIndex=null){
 const remaining=Math.max(0,session.events.a.WheelAt-session.now());
 const cooldown=wheelCooldownText(remaining);
 return `<section class="wheel-panel"><h2>Lucky meadow wheel</h2><div class="prize-wheel ${busy?'is-spinning':lastIndex!=null?'wheel-won':''}" ${lastIndex!=null?'data-winning-index="'+lastIndex+'"':''}><div class="wheel-rotor" data-angle="${lastIndex==null?0:(360-lastIndex*45)%360}" style="transform:rotate(${lastIndex==null?0:(360-lastIndex*45)%360}deg);--landed-angle:${lastIndex==null?0:(360-lastIndex*45)%360}deg">${[['coins',25],['wild',1],['coins',50],['extra',1],['coins',100],['remove',1],['coins',75],['coins',40]].map(([name,count],i)=>`<span data-segment="${i}" class="${lastIndex===i?'is-winning':''}" style="--segment:${i}"><span class="wheel-label">${gameIcon(name,38)}<small>${count}</small>${i===5?'<em class="wheel-heart">'+gameIcon('heart',16)+'+1</em>':''}</span></span>`).join('')}</div>${Array.from({length:8},(_,i)=>`<i class="wheel-lamp" style="--bulb:${i}" aria-hidden="true"></i>`).join('')}<b aria-hidden="true">${gameIcon('flower',30)}</b><i class="wheel-needle" aria-hidden="true"></i></div><div class="wheel-controls"><div class="wheel-pip">${pipMarkup('surprise',80)}</div><p id="wheel-result" role="status">${busy?'Spinning…':lastPrize?'Last prize: '+escapeHtml(lastPrize):'A little spin, a lovely surprise.'}</p><button class="story-play" data-action="spinWheel" ${busy||!session.wheelSpins?'disabled':''}>${gameIcon('wheel',28)} Spin · ${session.wheelSpins}</button><button class="primary" data-action="timedWheel" ${busy||remaining?'disabled':''}>${gameIcon('clock',25)} Daily spin · ${cooldown}</button><p class="meta">${resourceBadge(5,'trophy','Five campaign wins')} → 1 spin<br>Coins, helpers and a little luck!</p></div></section>`;
}
