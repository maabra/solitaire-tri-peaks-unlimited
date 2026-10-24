// 4,800 deterministic named compositions; map and Travel Hub use the same image.
import {SeededRandom} from './core.mjs';
const adjectives=['Willow','Moonlit','Amber','Snowdrop','Lavender','Coral','Golden','Cherry','Emerald','Festival','Whispering','Starlit','Misty','Sunflower','Crystal','Wisteria','Bamboo','Sapphire','Copper','Rosewood','Silver','Ivory','Jade','Crimson','Violet','Peach','Honey','Cedar','Juniper','Maple','Clover','Poppy','Daisy','Bluebell','Orchid','Lotus','Magnolia','Peony','Marigold','Iris','Fern','Moss','Birch','Oak','Pine','Palm','Coconut','Cinnamon','Ginger','Vanilla','Velvet','Silken','Pearl','Opal','Ruby','Topaz','Turquoise','Aquamarine','Amethyst','Diamond','Dawn','Dusk','Twilight','Midnight','Rainbow','Sunny','Cloudy','Breezy','Frosted','Dewy','Glowing','Dreaming','Singing','Dancing','Wandering','Hidden','Quiet','Secret','Happy','Lucky'];
const places=['Village','Harbor','Gardens','Orchard','Valley','Islands','Oasis','Grove','Forest','Fairground','Meadow','Lake','Cove','Canyon','Castle','Cavern','Canals','Highlands','Falls','Bay','Terrace','Hollow','Landing','Haven','Springs','Cliffs','Crossing','Glade','Retreat','Hills','Fields','Marsh','Shore','Reef','Lagoon','Delta','Ridge','Summit','Pass','Trail','Court','Plaza','Bridge','Quarter','Sanctuary','Observatory','Lighthouse','Farmstead','Windmills','Vineyard','Woodland','Creek','Riverbend','Glen','Park','Basin','Peninsula','Plateau','Beach','Market'];
export function chapterName(chapter){const n=chapter-1;return adjectives[n%80]+' '+places[Math.floor(n/80)%60];}
const images=new Map(),cache=new Map();
function load(src){if(!images.has(src))images.set(src,new Promise((resolve,reject)=>{const i=new Image();i.onload=()=>resolve(i);i.onerror=()=>{images.delete(src);reject(Error('Artwork unavailable'));};i.src=src;}));return images.get(src);}
export async function chapterPicture(chapter){
 if(cache.has(chapter))return cache.get(chapter);
 const promise=paint(chapter);cache.set(chapter,promise);if(cache.size>24)cache.delete(cache.keys().next().value);
 try{return await promise;}catch(e){cache.delete(chapter);throw e;}
}
async function paint(chapter){
 const paintings=['lighthouse-harbor','crystal-cavern','sky-islands','pumpkin-village','spring-village','snow-village','autumn-orchard','moonlit-garden','lavender-valley','tropical-cove','desert-oasis','cherry-garden','enchanted-forest','festival-garden'];const atlas=await load('assets/'+paintings[(chapter-1)*5%14]+'.png'),rng=new SeededRandom(chapter*7919+731),col=0,row=0,tw=atlas.width,th=atlas.height;
 const canvas=document.createElement('canvas');canvas.width=1280;canvas.height=720;const c=canvas.getContext('2d');
 const crop=.83+rng.next(160)/1000,sw=tw*crop,sh=th*crop,sx=col*tw+rng.next(1000)/1000*(tw-sw),sy=row*th+rng.next(1000)/1000*(th-sh);
 c.save();if(rng.next(2)){c.translate(1280,0);c.scale(-1,1);}c.filter=`hue-rotate(${rng.next(17)-8}deg) saturate(${.94+rng.next(18)/100})`;c.drawImage(atlas,sx,sy,sw,sh,0,0,1280,720);c.restore();c.filter='none';
 const light=c.createRadialGradient(rng.next(1100),rng.next(200),10,700,300,1000);light.addColorStop(0,['#ffedaa35','#ecbaff25','#bceeff20'][chapter%3]);light.addColorStop(1,'#173b3220');c.fillStyle=light;c.fillRect(0,0,1280,720);
 // Each destination has its own foreground branches, flowers and firefly placement.
 for(let i=0;i<24;i++){const right=i%2,x=right?1190+rng.next(100):rng.next(90),y=420+rng.next(300),size=6+rng.next(17);c.save();c.translate(x,y);c.rotate(rng.next(628)/100);c.fillStyle=['#f8d6e3bb','#fff1b7cc','#c0e68dbb','#cea9e8bb'][rng.next(4)];for(let p=0;p<5;p++){c.rotate(Math.PI*2/5);c.beginPath();c.ellipse(size*.7,0,size*.8,size*.4,0,0,Math.PI*2);c.fill();}c.fillStyle='#f6c15e';c.beginPath();c.arc(0,0,size*.25,0,Math.PI*2);c.fill();c.restore();}
 for(let i=0;i<35;i++){const x=rng.next(1280),y=rng.next(720);c.fillStyle=`rgba(255,244,192,${.15+rng.next(35)/100})`;c.beginPath();c.arc(x,y,1+rng.next(3),0,Math.PI*2);c.fill();}
 return canvas.toDataURL('image/jpeg',.88);
}
export function tableStyle(chapter){const tile=(chapter-1)*5%16;return {image:'assets/card-tables.png',position:`${tile%4*100/3}% ${Math.floor(tile/4)*100/3}%`,hue:(chapter*13%17)-8};}
