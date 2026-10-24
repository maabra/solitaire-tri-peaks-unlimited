import {chapterTitles,tableGrids} from './art-catalog.mjs';
// 4,800 deterministic named compositions; map and Travel Hub use the same image.
import {SeededRandom} from './core.mjs';
const adjectives=['Willow','Moonlit','Amber','Snowdrop','Lavender','Coral','Golden','Cherry','Emerald','Festival','Whispering','Starlit','Misty','Sunflower','Crystal','Wisteria','Bamboo','Sapphire','Copper','Rosewood','Silver','Ivory','Jade','Crimson','Violet','Peach','Honey','Cedar','Juniper','Maple','Clover','Poppy','Daisy','Bluebell','Orchid','Lotus','Magnolia','Peony','Marigold','Iris','Fern','Moss','Birch','Oak','Pine','Palm','Coconut','Cinnamon','Ginger','Vanilla','Velvet','Silken','Pearl','Opal','Ruby','Topaz','Turquoise','Aquamarine','Amethyst','Diamond','Dawn','Dusk','Twilight','Midnight','Rainbow','Sunny','Cloudy','Breezy','Frosted','Dewy','Glowing','Dreaming','Singing','Dancing','Wandering','Hidden','Quiet','Secret','Happy','Lucky'];
const places=['Village','Harbor','Gardens','Orchard','Valley','Islands','Oasis','Grove','Forest','Fairground','Meadow','Lake','Cove','Canyon','Castle','Cavern','Canals','Highlands','Falls','Bay','Terrace','Hollow','Landing','Haven','Springs','Cliffs','Crossing','Glade','Retreat','Hills','Fields','Marsh','Shore','Reef','Lagoon','Delta','Ridge','Summit','Pass','Trail','Court','Plaza','Bridge','Quarter','Sanctuary','Observatory','Lighthouse','Farmstead','Windmills','Vineyard','Woodland','Creek','Riverbend','Glen','Park','Basin','Peninsula','Plateau','Beach','Market'];
export function chapterName(chapter){return chapterTitles[(chapter-1)%51]+(chapter>51?' · Journey '+(1+Math.floor((chapter-1)/51)):'');}

const images=new Map(),cache=new Map();
function load(src){if(src.startsWith('data:'))return new Promise((resolve,reject)=>{const i=new Image();i.onload=()=>resolve(i);i.onerror=reject;i.src=src;});if(!images.has(src))images.set(src,new Promise((resolve,reject)=>{const i=new Image();i.onload=()=>resolve(i);i.onerror=()=>{images.delete(src);reject(Error('Artwork unavailable'));};i.src=src;}));return images.get(src);}
export async function chapterPicture(chapter){
 if(cache.has(chapter))return cache.get(chapter);
 const promise=paint(chapter);cache.set(chapter,promise);if(cache.size>4)cache.delete(cache.keys().next().value);
 try{return await promise;}catch(e){cache.delete(chapter);throw e;}
}
async function paint(chapter){
 const index=(chapter-1)%51,asset=Math.floor(index/3),column=index%3,image=await load('assets/portrait-worlds-'+String(asset).padStart(2,'0')+'.png');
 const canvas=document.createElement('canvas');canvas.width=384;canvas.height=2480;const c=canvas.getContext('2d'),sw=image.width/3,sx=column*sw;
 // A complete portrait mural, no hue filters, mirrored copies or wide-image stretch.
 const sourceWidth=Math.min(sw-2,image.height*canvas.width/canvas.height);c.drawImage(image,sx+(sw-sourceWidth)/2,0,sourceWidth,image.height,0,0,canvas.width,canvas.height);
 return canvas.toDataURL('image/jpeg',.92);
}
export async function chapterThumbnail(chapter){const url=await chapterPicture(chapter),image=await load(url),canvas=document.createElement('canvas');canvas.width=120;canvas.height=260;canvas.getContext('2d').drawImage(image,0,Math.max(0,image.height-image.width*260/120),image.width,Math.min(image.height,image.width*260/120),0,0,120,260);return canvas.toDataURL('image/jpeg',.85);}

export function tableStyle(chapter){const tile=(chapter-1)*5%16;return {image:'assets/card-tables.png',position:`${tile%4*100/3}% ${Math.floor(tile/4)*100/3}%`,hue:(chapter*13%17)-8};}
const tables=new Map();
export async function tableSurface(chapter,width,height){
 const tile=((chapter-1)%64+64)%64,key=`${tile}:${width}:${height}`;if(tables.has(key))return tables.get(key);
 const promise=(async()=>{const group=Math.floor(tile/16),local=tile%16,image=await load(group===3?'assets/card-tables.png':'assets/card-tables-'+group+'.png'),grid=tableGrids[group],col=local%4,row=Math.floor(local/4),sx=grid.x[0][col]+2,sy=grid.y[0][row]+2,sw=grid.x[1][col]-sx-2,sh=grid.y[1][row]-sy-2;
  const c=document.createElement('canvas');c.width=Math.max(1,width);c.height=Math.max(1,height);const ctx=c.getContext('2d'),sourceEdge=Math.min(100,Math.floor(Math.min(sw,sh)*.42)),edge=Math.min(62,width*.12,height*.15),a=[0,sourceEdge,sw-sourceEdge,sw],b=[0,sourceEdge,sh-sourceEdge,sh],x=[0,edge,width-edge,width],y=[0,edge,height-edge,height];
  // Nine-slice the actual irregular atlas cells: no adjacent tiles, no stretched corners.
  for(let r=0;r<3;r++)for(let k=0;k<3;k++)ctx.drawImage(image,sx+a[k],sy+b[r],a[k+1]-a[k],b[r+1]-b[r],x[k],y[r],x[k+1]-x[k],y[r+1]-y[r]);
  return c.toDataURL('image/jpeg',.9);
 })();tables.set(key,promise);if(tables.size>12)tables.delete(tables.keys().next().value);try{return await promise;}catch(e){tables.delete(key);throw e;}
}
export function fitCardTable(root,chapter){const shell=root.querySelector('.board-shell');if(!shell)return;const w=Math.round(shell.clientWidth),h=Math.round(shell.clientHeight),key=`${chapter}:${w}:${h}`;if(shell.dataset.tableKey===key)return;shell.dataset.tableKey=key;tableSurface(chapter,w,h).then(url=>{if(shell.isConnected&&shell.dataset.tableKey===key){shell.style.backgroundImage=`url('${url}')`;shell.classList.add('painted-table');}}).catch(()=>{});}
