import {pipMarkup} from './pip-view.mjs';
import {swapHeartProgress} from './resource-view.mjs';
import {actionButton as button} from './ui-markup.mjs';
import {gameIcon} from './game-art.mjs';
export class LivesPanel {
 constructor(getSession,openModal){this.getSession=getSession;this.openModal=openModal;}
 show(){const session=this.getSession(),openModal=this.openModal;openModal(`<section class="lives-panel"><h2>Your hearts</h2><div class="hearts-hero">${pipMarkup('heart',95)}<div class="lives-total">${gameIcon('heart',90)}<b>${session.p.Hearts}</b></div></div><p>Start with 3. Store up to 999.</p><p>Lose one heart when you lose, restart or replace an unfinished hand. Pausing, visiting menus and resuming are free.</p><div class="actions">${button(gameIcon('gems',28)+' Gem Garden','gems','primary')}${button(gameIcon('gift',28)+' Daily gift +1','daily','',session.p.Daily===session.day()?'disabled':'')}${button('Reward activities','events')}</div><p class="meta">${swapHeartProgress(session.p.GemHeartProgress)}<br>Bonus levels and rewards also give hearts.</p>${button('Back to map','backMap')}</section>`);}
}
