// One hierarchy shared by the directory, sibling navigation and map dock.
export const activityGroups=Object.freeze([
 ['Play & explore','compass',['missions','balloon','hunt','bingo','tickets','trail','memory']],
 ['Your collections','album',['album','customize','postcard']],
 ['Races & rewards','trophy',['regatta','tournament','voyage','chest','milestones']],
 ['Travel','travel',['travel']]
]);
export function activityGroup(view){
 if(['pack','tokenShop','albums'].includes(view))view='album';
 return activityGroups.find(([, ,ids])=>ids.includes(view));
}
