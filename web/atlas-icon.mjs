// Clip each atlas cell in its own square coordinate system. CSS sizing can no
// longer expose an adjacent cell when an icon is displayed in a narrow slot.
let serial=0;
export function atlasIcon(file,index,columns,rows,width,height,size,classes='',attributes=''){
 const w=width/columns,h=height/rows,x=index%columns*w,y=Math.floor(index/columns)*h;
 const clip=`atlas-cell-${++serial}`;
 return `<svg class="game-icon atlas-icon ${classes}" viewBox="${x-2} ${y-2} ${w+4} ${h+4}" preserveAspectRatio="xMidYMid meet" style="--icon-size:${size}px;--chest-size:${size}px" ${attributes||'aria-hidden="true"'}><defs><clipPath id="${clip}"><rect x="${x+.75}" y="${y+.75}" width="${w-1.5}" height="${h-1.5}"/></clipPath></defs><image href="assets/${file}" width="${width}" height="${height}" clip-path="url(#${clip})"/></svg>`;
}
