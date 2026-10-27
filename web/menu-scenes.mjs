// Presentation only: menu scenes never own progress, rewards or navigation.
const scenes={shop:'spring-village',settings:'moonlit-garden',journal:'autumn-orchard',guide:'enchanted-forest',hub:'spring-village',overview:'spring-village',album:'cherry-garden',customize:'lavender-valley',postcard:'tropical-cove',travel:'tropical-cove',missions:'autumn-orchard',regatta:'tropical-cove',voyage:'tropical-cove',tournament:'festival-garden',chest:'desert-oasis',milestones:'festival-garden',hunt:'enchanted-forest',bingo:'lavender-valley',balloon:'spring-village',tickets:'autumn-orchard',trail:'treasure-trail-scene',memory:'matching-garden-scene'};
// Choose the illustrated menu surface after the content's semantic role is known.
export function decorateMenuScene(root,topic){
 root.dataset.menuScene=topic;
 const place=topic==='shop'?'shop':['album','albums','pack','tokenShop','customize','postcard'].includes(topic)?'collections':['settings','journal','guide','default'].includes(topic)?'study':'adventures';
 const dialog=root.closest('dialog');
 if(dialog.dataset.frame==='menu'){dialog.dataset.place=place;dialog.style.setProperty('--place-art',`url("assets/menu-place-${place}.png")`);}else{delete dialog.dataset.place;dialog.style.removeProperty('--place-art');}
 root.style.setProperty('--menu-scene',`url("assets/${scenes[topic]??'spring-village'}.png")`);
 if(['trail','memory'].includes(topic)){
  const picture=document.createElement('img');picture.className='activity-scene';picture.alt='';
  picture.src=`assets/${topic==='trail'?'treasure-trail':'matching-garden'}-scene.png`;
  root.querySelector('.panel-heading')?.after(picture);
 }
 const shop=root.querySelector('.shop');
 if(shop&&!root.querySelector('.shop-screen'))for(const article of shop.children){
  const description=article.querySelector('p'),title=article.querySelector('h3');
  if(description&&title){
   const details=document.createElement('details');details.className='helper-description';
   const summary=document.createElement('summary');summary.textContent='How to use';
   details.append(summary,description);article.insertBefore(details,article.querySelector('[data-action^="buy:"]'));
  }
 }
}
