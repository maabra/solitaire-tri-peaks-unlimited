// Remember routes, never rendered HTML or reward state. Back rerenders fresh data.
export class MenuJourney {
 constructor(isRoute){this.isRoute=isRoute;this.routes=[];}
 visit(action,open,scroll=0){if(!this.isRoute(action))return;if(!open)this.routes=[];const last=this.routes.at(-1);if(last?.action===action)return;if(last)last.scroll=scroll;const existing=this.routes.findIndex(route=>route.action===action);if(existing>=0){this.routes.splice(existing+1);return;}this.routes.push({action,scroll:0});if(this.routes.length>20)this.routes.shift();}
 get canBack(){return this.routes.length>1;}
 back(){if(!this.canBack)return null;this.routes.pop();return this.routes.at(-1);}
 clear(){this.routes=[];}
}

// New menu surfaces have their own scroll areas inside the modal.
export function captureMenuScroll(dialog){return {outer:dialog.scrollTop,page:dialog.querySelector('.dialog-content')?.scrollTop??0,shelf:dialog.querySelector('.shop')?.scrollTop??0};}
export function restoreMenuScroll(dialog,saved){
 if(typeof saved==='number'){dialog.scrollTop=saved;return;}
 if(!saved)return;dialog.scrollTop=saved.outer??0;
 const page=dialog.querySelector('.dialog-content'),shelf=dialog.querySelector('.shop');
 if(page)page.scrollTop=saved.page??0;if(shelf)shelf.scrollTop=saved.shelf??0;
}
