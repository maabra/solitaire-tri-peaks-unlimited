// Presentation only: menu scenes never own progress, rewards or navigation.
const scenes={shop:'spring-village',settings:'moonlit-garden',journal:'autumn-orchard',guide:'enchanted-forest',hub:'spring-village',overview:'spring-village',album:'cherry-garden',customize:'lavender-valley',postcard:'tropical-cove',travel:'tropical-cove',missions:'autumn-orchard',regatta:'tropical-cove',voyage:'tropical-cove',tournament:'festival-garden',chest:'desert-oasis',milestones:'festival-garden',hunt:'enchanted-forest',bingo:'lavender-valley',balloon:'spring-village',tickets:'autumn-orchard',trail:'enchanted-forest',memory:'cherry-garden'};
export function decorateMenuScene(root,topic){
 root.dataset.menuScene=topic;
 root.style.setProperty('--menu-scene',`url("assets/${scenes[topic]??'spring-village'}.png")`);
 const shop=root.querySelector('.shop');
 if(shop)for(const article of shop.children){
  const description=article.querySelector('p'),title=article.querySelector('h3');
  if(description&&title){
   const details=document.createElement('details');details.className='helper-description';
   const summary=document.createElement('summary');summary.textContent='How to use';
   details.append(summary,description);article.insertBefore(details,article.querySelector('[data-action^="buy:"]'));
  }
 }
}
