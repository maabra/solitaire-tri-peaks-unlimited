// Remember routes, never rendered HTML or reward state. Back rerenders fresh data.
export class MenuJourney {
 // Bind route classification and begin with no modal navigation history.
 constructor(isRoute){this.isRoute=isRoute;this.routes=[];}
 // Revisiting an ancestor collapses the intervening history. This avoids A -> B -> A loops
 // while retaining the ancestor's scroll position for the next Back action.
 visit(action,open,scroll=0){if(!this.isRoute(action))return;if(!open)this.routes=[];const last=this.routes.at(-1);if(last?.action===action)return;if(last)last.scroll=scroll;const existing=this.routes.findIndex(route=>route.action===action);if(existing>=0){this.routes.splice(existing+1);return;}this.routes.push({action,scroll:0});if(this.routes.length>20)this.routes.shift();}
 // Offer Back only when a parent destination exists in the route stack.
 get canBack(){return this.routes.length>1;}
 // Remove the current route and return its parent for a fresh render.
 back(){if(!this.canBack)return null;this.routes.pop();return this.routes.at(-1);}
 // Discard modal history after closing the menu.
 clear(){this.routes=[];}
}

// New menu surfaces have their own scroll areas inside the modal.
export function captureMenuScroll(dialog){return {outer:dialog.scrollTop,page:dialog.querySelector('.dialog-content')?.scrollTop??0,shelf:dialog.querySelector('.shop')?.scrollTop??0};}
// Restore the saved scroll areas while accepting older single-number history entries.
export function restoreMenuScroll(dialog,saved){
 if(typeof saved==='number'){dialog.scrollTop=saved;return;}
 if(!saved)return;dialog.scrollTop=saved.outer??0;
 const page=dialog.querySelector('.dialog-content'),shelf=dialog.querySelector('.shop');
 if(page)page.scrollTop=saved.page??0;if(shelf)shelf.scrollTop=saved.shelf??0;
}
