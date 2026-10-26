import {collectionIndex,CHAPTER_SET_COUNT} from './chapter-collection.mjs';
import {chapterTitles,tableGrids} from './art-catalog.mjs';
// One original portrait mural per campaign chapter; map and Travel Hub share it.
import {SeededRandom} from './core.mjs';
export function chapterName(chapter){return chapterTitles[collectionIndex(chapter)];}

const images=new Map(),cache=new Map();
function load(src){if(src.startsWith('data:'))return new Promise((resolve,reject)=>{const i=new Image();i.onload=()=>resolve(i);i.onerror=reject;i.src=src;});if(!images.has(src))images.set(src,new Promise((resolve,reject)=>{const i=new Image();i.onload=()=>resolve(i);i.onerror=()=>{images.delete(src);reject(Error('Artwork unavailable'));};i.src=src;}));return images.get(src);}
export async function chapterPicture(chapter){
 if(collectionIndex(chapter)===0)return 'assets/map-meadow-720p.png';
 if(cache.has(chapter))return cache.get(chapter);
 const promise=paint(chapter);cache.set(chapter,promise);if(cache.size>4)cache.delete(cache.keys().next().value);
 try{return await promise;}catch(e){cache.delete(chapter);throw e;}
}
async function paint(chapter){
 const index=collectionIndex(chapter),extra=index>=51,local=extra?index-51:index,asset=Math.floor(local/3),column=local%3,image=await load('assets/'+(extra?'portrait-extra-':'portrait-worlds-')+String(asset).padStart(2,'0')+'.png');
 const canvas=document.createElement('canvas');canvas.width=384;canvas.height=2480;const c=canvas.getContext('2d'),sw=image.width/3,sx=column*sw;
 // A complete portrait mural, no hue filters, mirrored copies or wide-image stretch.
 const sourceWidth=Math.min(sw-2,image.height*canvas.width/canvas.height);c.drawImage(image,sx+(sw-sourceWidth)/2,0,sourceWidth,image.height,0,0,canvas.width,canvas.height);
 return canvas.toDataURL('image/jpeg',.92);
}
export async function chapterThumbnail(chapter){const url=await chapterPicture(chapter),image=await load(url),canvas=document.createElement('canvas');canvas.width=120;canvas.height=260;canvas.getContext('2d').drawImage(image,0,Math.max(0,image.height-image.width*260/120),image.width,Math.min(image.height,image.width*260/120),0,0,120,260);return canvas.toDataURL('image/jpeg',.85);}

export function tableStyle(chapter){const tile=(chapter-1)*5%16;return {image:'assets/card-tables.png',position:`${tile%4*100/3}% ${Math.floor(tile/4)*100/3}%`,hue:(chapter*13%17)-8};}
const tables=new Map();
export async function tableSurface(chapter,width,height){
 const tile=chapter===0?100:collectionIndex(chapter),key=`${tile}:${width}:${height}`;if(tables.has(key))return tables.get(key);
 const promise=(async()=>{const extra=tile>=48&&tile<100,group=extra?Math.floor((tile-48)/16):tile===100?3:Math.floor(tile/16),local=extra?(tile-48)%16:tile===100?0:tile%16,image=await load(extra?'assets/chapter-tables-'+group+'.png':group===3?'assets/card-tables.png':'assets/card-tables-'+group+'.png'),grid=tableGrids[group],cols=extra&&group===3?2:4,col=local%cols,row=Math.floor(local/cols),sx=extra?col*image.width/cols+3:grid.x[0][col]+2,sy=extra?row*image.height/cols+3:grid.y[0][row]+2,sw=extra?image.width/cols-6:grid.x[1][col]-sx-2,sh=extra?image.height/cols-6:grid.y[1][row]-sy-2;
  const c=document.createElement('canvas');c.width=Math.max(1,width);c.height=Math.max(1,height);const ctx=c.getContext('2d'),sourceEdge=Math.min(100,Math.floor(Math.min(sw,sh)*.42)),edge=Math.min(62,width*.12,height*.15),a=[0,sourceEdge,sw-sourceEdge,sw],b=[0,sourceEdge,sh-sourceEdge,sh],x=[0,edge,width-edge,width],y=[0,edge,height-edge,height];
  // Nine-slice the actual irregular atlas cells: no adjacent tiles, no stretched corners.
  for(let r=0;r<3;r++)for(let k=0;k<3;k++)ctx.drawImage(image,sx+a[k],sy+b[r],a[k+1]-a[k],b[r+1]-b[r],x[k],y[r],x[k+1]-x[k],y[r+1]-y[r]);
  return c.toDataURL('image/jpeg',.9);
 })();tables.set(key,promise);if(tables.size>12)tables.delete(tables.keys().next().value);try{return await promise;}catch(e){tables.delete(key);throw e;}
}
export function fitCardTable(root,chapter){const shell=root.querySelector('.board-shell');if(!shell)return;shell.classList.add('painted-table');const w=Math.round(shell.clientWidth),h=Math.round(shell.clientHeight),key=`${chapter}:${w}:${h}`;if(shell.dataset.tableKey===key)return;shell.dataset.tableKey=key;tableSurface(chapter,w,h).then(url=>{if(shell.isConnected&&shell.dataset.tableKey===key){shell.style.backgroundImage=`url('${url}')`;shell.classList.add('painted-table');}}).catch(()=>{});}
