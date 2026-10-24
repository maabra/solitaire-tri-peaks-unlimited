// Cookies remember permission only; full saves exceed cookie limits and use localStorage.
export async function requireStorageConsent(){
 if(globalThis.meadowDesktop)return;
 let agreed=false;try{agreed=localStorage.getItem('meadow-storage-consent')==='yes';}catch{}
 if(agreed)return;
 const dialog=document.querySelector('#dialog'),modal=document.querySelector('#modal'),close=dialog.querySelector('.close');
 close.hidden=true;modal.innerHTML=`<section class="consent-panel"><div class="consent-art" aria-hidden="true">✿</div><h2>Your adventure, saved here</h2><p>Allow this game to save cards, coins, collections and progress on this device?</p><p>We use browser storage for your save and a small cookie to remember your choice. Nothing is sent to a server. Clearing browser data removes this save; you can export backups in Settings.</p><button class="story-play" id="allow-storage">Allow local saving &amp; play</button><p id="consent-error" role="status"></p></section>`;
 const prevent=e=>e.preventDefault();dialog.addEventListener('cancel',prevent);dialog.showModal();
 await new Promise(resolve=>modal.querySelector('#allow-storage').addEventListener('click',()=>{try{localStorage.setItem('meadow-storage-consent','yes');document.cookie='meadow_storage=yes; Max-Age=31536000; Path=/; SameSite=Lax'+(location.protocol==='https:'?'; Secure':'');resolve();}catch{modal.querySelector('#consent-error').textContent='Saving is blocked by your browser. Allow storage for this site, then try again.';}}));
 dialog.removeEventListener('cancel',prevent);dialog.close();close.hidden=false;
}
