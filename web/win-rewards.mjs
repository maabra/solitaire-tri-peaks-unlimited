// Base coins and stars are automatic. Only additional prizes use the reveal tray.
export function extraWinRewards(r={}){return [
 ...(r.hearts?[{icon:'heart',amount:r.hearts,label:'+'+r.hearts+' hearts'}]:[]),
 ...(r.item?[{icon:r.item,amount:1,label:'+1 '+r.item}]:[]),
 ...(r.packs?.length?[{icon:'pack',amount:r.packs.length,label:r.packs.length+' packs'}]:[]),
 ...(r.arrows?[{icon:'arrow',amount:r.arrows,label:r.arrows+' arrows'}]:[]),
 ...(r.trophies?[{icon:'trophy',amount:r.trophies,label:r.trophies+' trophies'}]:[]),
 ...(r.chapterGift?[{icon:'studio',amount:1,label:'New card back and table'}]:[])
];}
