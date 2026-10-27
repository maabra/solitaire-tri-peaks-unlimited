export const STARTING_HEARTS=3,MAX_HEARTS=999;
// This object only bounds the heart balance. Session decides when an attempt loses a
// heart and records that charge, so opening or resuming a menu cannot charge it twice.
export class Lives {
 // Bind heart operations to the profile's persisted balance.
 constructor(profile){this.p=profile;}
 // Read the current saved heart balance.
 get count(){return this.p.Hearts;}
 // Validate and cap a heart reward, returning only the quantity actually added.
 add(count=1){if(!Number.isSafeInteger(count)||count<0)throw Error('Invalid heart reward.');const before=this.count;this.p.Hearts=Math.min(MAX_HEARTS,before+count);return this.count-before;}
 // Deduct one heart if available without allowing a negative balance.
 lose(){if(this.count===0)return false;this.p.Hearts--;return true;}
 // Report whether the balance permits beginning an attempt.
 get canStart(){return this.count>0;}
}
