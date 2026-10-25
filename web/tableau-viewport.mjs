// Fit the complete deal, never the remaining cards: clearing a card cannot move
// its neighbors. One uniform scale preserves the engine's overlap geometry.
export function fitTableau(slots,width,viewportHeight,compact=false){
 const bounds=slots.map(s=>{
  const angle=s.Angle*Math.PI/180,c=Math.abs(Math.cos(angle)),n=Math.abs(Math.sin(angle));
  return {left:s.DrawX-(c*76+n*106)/2,right:s.DrawX+(c*76+n*106)/2,
   top:s.DrawY-(c*106+n*76)/2,bottom:s.DrawY+(c*106+n*76)/2};
 });
 const left=Math.min(...bounds.map(b=>b.left)),right=Math.max(...bounds.map(b=>b.right));
 const top=Math.min(...bounds.map(b=>b.top)),bottom=Math.max(...bounds.map(b=>b.bottom));
 const target=Math.max(compact?190:230,Math.min(compact?300:350,viewportHeight*(compact?.34:.43)));
 const scale=Math.min((width-18)/(right-left),target/(bottom-top+18),(compact?66:112)/76);
 const height=(bottom-top)*scale+18;
 return {width,height,cw:76*scale,ch:106*scale,
  positions:slots.map(s=>({left:width/2+(s.DrawX-(left+right)/2)*scale-38*scale,
   top:9+(s.DrawY-top)*scale-53*scale}))};
}
