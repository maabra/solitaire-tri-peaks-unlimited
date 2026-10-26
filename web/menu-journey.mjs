// Remember routes, never rendered HTML or reward state. Back rerenders fresh data.
export class MenuJourney {
 constructor(isRoute){this.isRoute=isRoute;this.routes=[];}
 visit(action,open,scroll=0){if(!this.isRoute(action))return;if(!open)this.routes=[];const last=this.routes.at(-1);if(last?.action===action)return;if(last)last.scroll=scroll;const existing=this.routes.findIndex(route=>route.action===action);if(existing>=0){this.routes.splice(existing+1);return;}this.routes.push({action,scroll:0});if(this.routes.length>20)this.routes.shift();}
 get canBack(){return this.routes.length>1;}
 back(){if(!this.canBack)return null;this.routes.pop();return this.routes.at(-1);}
 clear(){this.routes=[];}
}
