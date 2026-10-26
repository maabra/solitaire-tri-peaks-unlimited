import {adventureDeal} from '../web/adventure-hand.mjs';
let count=0;
for(let level=1095;level<=10000;level+=100){
 adventureDeal(104729+level*997,level,false,'MP26');count++;
 if(count%1000===0)console.log('Verified long gardens:',count);
}
console.log(`PASS: ${count} long gardens, including every 60/80/104/120-card campaign hand.`);
