export const STARTING_HEARTS=3,MAX_HEARTS=999;
export class Lives {
 constructor(profile){this.p=profile;}
 get count(){return this.p.Hearts;}
 add(count=1){if(!Number.isSafeInteger(count)||count<0)throw Error('Invalid heart reward.');const before=this.count;this.p.Hearts=Math.min(MAX_HEARTS,before+count);return this.count-before;}
 lose(){if(this.count===0)return false;this.p.Hearts--;return true;}
 get canStart(){return this.count>0;}
}
