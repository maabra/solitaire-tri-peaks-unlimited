// Original procedural score and effects. No recordings, downloads or external services.
// Browsers require a user gesture before audio can start. Keep unlocking, volume and playback
// inside one owner so rerendering a menu does not create competing music players.
export class MeadowAudio {
 // Bind saved settings and initialize one shared audio context/voice owner.
 constructor(getSettings){this.settings=getSettings;this.context=null;this.scene='map';this.voices=new Set();this.step=0;this.next=0;this.timer=null;this.unlocked=false;this.hidden=()=>{if(document.hidden)this.pause();else if(this.unlocked)this.resume();};document.addEventListener('visibilitychange',this.hidden);}
 // Record the user gesture and resume audio under the browser autoplay policy.
 async unlock(){this.unlocked=true;await this.resume();}
 // Create or resume the audio context only when the current settings permit playback.
 async resume(){const p=this.settings();if(document.hidden||!p.Sound&&!p.Music)return;try{if(!this.context){const Context=globalThis.AudioContext??globalThis.webkitAudioContext;if(!Context)return;this.context=new Context();this.effects=this.context.createGain();this.music=this.context.createGain();const master=this.context.createDynamicsCompressor();master.threshold.value=-12;master.ratio.value=4;this.effects.connect(master);this.music.connect(master);master.connect(this.context.destination);}await this.context.resume();this.configure();if(!this.timer){this.next=this.context.currentTime+.05;this.timer=setInterval(()=>this.schedule(),100);}}catch{/* Audio unavailable: gameplay continues silently. */}}
 // Apply saved volume and enabled settings to the active audio paths.
 configure(){if(!this.context)return;const p=this.settings(),t=this.context.currentTime;this.effects.gain.setTargetAtTime(p.Sound?(p.EffectsVolume??.65):0,t,.03);this.music.gain.setTargetAtTime(p.Music?(p.MusicVolume??.35):0,t,.08);}
 // Switch the music scene without creating an independent player for each screen.
 setScene(scene){this.scene=scene;}
 // Schedule a synthesized tone with the requested frequency and envelope.
 tone(frequency,time,length,volume=.1,type='sine',bus=this.effects){if(!this.context||!bus)return;const o=this.context.createOscillator(),g=this.context.createGain();o.type=type;o.frequency.value=frequency;g.gain.setValueAtTime(0,time);g.gain.linearRampToValueAtTime(volume,time+.012);g.gain.exponentialRampToValueAtTime(.0001,time+length);o.connect(g);g.connect(bus);this.voices.add(o);o.onended=()=>{this.voices.delete(o);o.disconnect();g.disconnect();};o.start(time);o.stop(time+length+.02);}
 // Generate a short shaped noise sound for effects that need a non-tonal attack.
 noise(time,length=.09){const ctx=this.context,n=Math.ceil(ctx.sampleRate*length),buffer=ctx.createBuffer(1,n,ctx.sampleRate),data=buffer.getChannelData(0);let seed=7123;for(let i=0;i<n;i++){seed=(seed*16807)%2147483647;data[i]=(seed/2147483647*2-1)*Math.min(1,i/(ctx.sampleRate*.006))*(1-i/n)**2;}const src=ctx.createBufferSource(),filter=ctx.createBiquadFilter(),gain=ctx.createGain();src.buffer=buffer;filter.type='lowpass';filter.frequency.value=1100;filter.Q.value=.5;gain.gain.value=.018;src.connect(filter);filter.connect(gain);gain.connect(this.effects);this.voices.add(src);src.onended=()=>{this.voices.delete(src);src.disconnect();filter.disconnect();gain.disconnect();};src.start(time);src.stop(time+length);}
 // Choose and schedule the requested sound effect using the current effects volume.
 play(kind='card'){if(!this.context||this.context.state!=='running'||!this.settings().Sound||document.hidden)return;const t=this.context.currentTime;
  const notes={click:[740],card:[660,880],draw:[392,523],coin:[1047,1319,1568],win:[523,659,784,1047],gem:[784,988,1175],helper:[440,659,880],error:[196,185],reveal:[523,784,1047],wheel:[880,988,1175]}[kind]??[660];
  if(kind==='card'||kind==='draw')this.noise(t,.045);
  notes.forEach((f,i)=>this.tone(f,t+i*.065,kind==='win'?.65:kind==='card'||kind==='draw'?.15:.2,kind==='click'?.035:kind==='card'||kind==='draw'?.045:.065,'sine'));
 }
 // Schedule the next portion of scene music against the audio clock.
 schedule(until=null){if(!this.context||document.hidden)return;const ctx=this.context;if(!this.settings().Music){this.next=ctx.currentTime+.1;return;}if(this.next<ctx.currentTime)this.next=ctx.currentTime+.05;const beat=this.scene==='gems'?.32:.42;
  // 32-bar A/B composition: C, Am, F, G with a contrasting Dm/Em bridge.
  const chords=[[48,52,55],[45,48,52],[41,45,48],[43,47,50],[48,52,55],[45,48,52],[50,53,57],[43,47,50]];
  const melody=[0,2,4,2,1,2,5,4,2,1,0,2,4,5,4,2,0,1,3,2,4,2,1,0,2,4,6,5,4,2,1,2],scale=[60,62,64,65,67,69,71,72];
  while(this.next<(until??ctx.currentTime+.25)){const bar=Math.floor(this.step/8)%32,pos=this.step%8,chord=chords[Math.floor(bar/4)],note=scale[melody[(bar*2+Math.floor(pos/2))%melody.length]]+(this.scene==='play'?0:12),/* Convert the musical pitch into the oscillator frequency used by the scheduled notes. */
 hz=n=>440*2**((n-69)/12);
   if(pos%2===0)this.tone(hz(note),this.next,beat*2.5,.045,'sine',this.music);
   this.tone(hz(chord[pos%3]+12),this.next,beat*1.8,.021,'triangle',this.music);
   if(pos===0)this.tone(hz(chord[0]),this.next,beat*7,.045,'sine',this.music);
   this.step=(this.step+1)%256;this.next+=beat;
  }
 }
 // Suspend scheduled scene playback while retaining the shared audio owner.
 pause(){clearInterval(this.timer);this.timer=null;for(const voice of this.voices)try{voice.stop();}catch{}this.voices.clear();this.context?.suspend().catch(()=>{});}
 // Release audio scheduling and voices when the owner is no longer needed.
 dispose(){this.pause();document.removeEventListener('visibilitychange',this.hidden);this.context?.close().catch(()=>{});}
}
