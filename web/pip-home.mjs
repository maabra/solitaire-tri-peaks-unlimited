import {pipMarkup} from './pip-view.mjs';
import {escapeHtml} from './ui-markup.mjs';

const thoughts=[
 ['cheer','Solitaire is fun, one card at a time!'],
 ['think','A little luck, a little thinking. That’s solitaire!'],
 ['surprise','Every shuffle brings a little surprise.'],
 ['pointRight','Luck deals the cards. You choose the next move!'],
 ['curious','One higher or one lower… what will you pick?'],
 ['laugh','My favorite kind of adventure? A card adventure!'],
 ['reassure','A tricky hand? Take a breath and look again.'],
 ['star','A lovely streak starts with one little match.'],
 ['wave','Pull up a chair. Let’s play some solitaire!'],
 ['heart','Win or lose, I’m happy to play alongside you.'],
 ['think','Sometimes the next draw changes everything!'],
 ['celebrate','New cards, new chances, new little discoveries!']
];

// Its timer belongs to the home view, so leaving home cannot keep it running.
class PipHome extends HTMLElement{
 connectedCallback(){
  this.index=Math.floor(Math.random()*thoughts.length);
  this.innerHTML='<button class="pip-home-chat" type="button" title="Tap Pip for another thought"><span class="pip-home-bird"></span><span class="pip-home-bubble"><small>Pip says</small><span class="pip-home-thought"></span></span></button>';
  this.querySelector('button').onclick=()=>this.advance();this.paint();
  this.timer=setInterval(()=>{
   if(!document.hidden&&!document.querySelector('dialog[open]')&&!this.matches(':hover,:focus-within'))this.advance();
  },9000);
 }
 disconnectedCallback(){clearInterval(this.timer);}
 advance(){this.index=(this.index+1)%thoughts.length;this.paint();}
 paint(){const [mood,text]=thoughts[this.index];this.querySelector('.pip-home-bird').innerHTML=pipMarkup(mood,100);this.querySelector('.pip-home-thought').innerHTML=escapeHtml(text);}
}
customElements.define('pip-home',PipHome);
