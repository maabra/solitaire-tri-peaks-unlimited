const colors=[['#f79baf','#bd365d'],['#a4e4ff','#3374be'],['#fff19b','#db9a2f'],['#bef29a','#4b9c5c'],['#e4bbff','#8f4bae'],['#b2fff2','#299ca8']];
export function gemMarkup(kind){
 const [light,dark]=colors[kind];
 const shapes=['<path d="M32 56C25 48 6 37 6 21C6 6 24 5 32 17C40 5 58 6 58 21C58 37 39 48 32 56Z"/>','<path d="M32 3L60 32L32 61L4 32Z"/>','<path d="M32 2L40 22L61 24L45 38L50 60L32 48L14 60L19 38L3 24L24 22Z"/>','<path d="M12 5H52L60 13V51L52 59H12L4 51V13Z"/>','<circle cx="32" cy="32" r="28"/>','<path d="M32 3L57 17V47L32 61L7 47V17Z"/>'];
 return `<svg class="jewel" viewBox="0 0 64 64" aria-hidden="true" style="--gem-light:${light};--gem-dark:${dark}"><defs><linearGradient id="gem-${kind}" x2=".8" y2="1"><stop stop-color="${light}"/><stop offset="1" stop-color="${dark}"/></linearGradient></defs><g fill="url(#gem-${kind})" stroke="${light}" stroke-width="2">${shapes[kind]}</g><path d="M15 15L31 8L46 17L34 30Z" fill="#ffffff60"/><path d="M34 30L48 17L52 44L34 53Z" fill="#ffffff18"/><path d="M12 45L34 53L34 30Z" fill="#233c5222"/><ellipse cx="22" cy="16" rx="6" ry="3" fill="#ffffffe0" transform="rotate(-26 22 16)"/></svg>`;
}
