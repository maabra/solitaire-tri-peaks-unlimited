// Every generated MP21 deal constructs and replays its actual winning witness.
import {Worker,isMainThread,parentPort,workerData} from 'node:worker_threads';
import {adventureDeal} from '../web/adventure-hand.mjs';
if(!isMainThread){
 const failures=[];for(let level=workerData.first;level<=workerData.last;level++){try{adventureDeal((104729+level*997)%2147483647,level,false,'MP22');}catch(e){failures.push({level,error:e.message});if(failures.length>=20)break;}if((level-workerData.first+1)%10000===0)parentPort.postMessage({progress:10000});}
 parentPort.postMessage({failures,done:true});
}else{
 const total=process.argv.includes('--all')?480000:10000,count=4,size=Math.ceil(total/count);let completed=0;
 const results=await Promise.all(Array.from({length:count},(_,i)=>new Promise((resolve,reject)=>{const w=new Worker(new URL(import.meta.url),{workerData:{first:i*size+1,last:Math.min(total,(i+1)*size)}});w.on('error',reject);w.on('message',m=>{if(m.progress){completed+=m.progress;if(completed%50000===0)console.log('Verified '+completed+' mutation deals.');}if(m.done)resolve(m.failures);});w.on('exit',code=>{if(code)reject(Error('Verifier exited '+code));});})));
 const failures=results.flat();if(failures.length){console.error(JSON.stringify(failures,null,2));process.exitCode=1;}else console.log(`PASS: ${total.toLocaleString()} MP22 campaign deals, actual mutation-aware winning witnesses.`);
}
