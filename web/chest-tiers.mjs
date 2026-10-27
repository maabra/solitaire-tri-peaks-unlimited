export const chestTiers=Object.freeze([
 {name:'Bronze',stars:10,color:'#c58c54',edge:'#875939'},
 {name:'Silver',stars:60,color:'#c9dbe7',edge:'#748aab'},
 {name:'Gold',stars:120,color:'#ffd270',edge:'#b87835'},
 {name:'Platinum',stars:200,color:'#dcf2ee',edge:'#7aa9b0'},
 {name:'Diamond',stars:250,color:'#9be6ff',edge:'#6887c1'},
 {name:'Obsidian',stars:300,color:'#735288',edge:'#3e284c'}
]);
// Count the chest thresholds reached by the chapter's earned stars.
export const chestTierFor=stars=>chestTiers.filter(t=>stars>=t.stars).length;
// Translate the reached chest tier into its claimable reward, or return no reward below Bronze.
export function chestReward(stars){const tier=chestTierFor(stars);return tier?{hearts:1,coins:50+tier*50,pack:Math.min(5,tier),item:tier>=4?'tnt':'extra'}:null;}
