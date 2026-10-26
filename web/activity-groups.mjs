import {activityGroups} from './generated-content.mjs';
export {activityGroups} from './generated-content.mjs';
// One hierarchy shared by the directory, sibling navigation and map dock.
export function activityGroup(view){
 if(['pack','tokenShop','albums'].includes(view))view='album';
 return activityGroups.find(([, ,ids])=>ids.includes(view));
}
