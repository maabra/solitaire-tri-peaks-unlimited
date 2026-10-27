const moods=['welcome','wave','laugh','celebrate','think','curious','pointRight','pointLeft','reassure','surprise','sad','sleep','star','cheer','heart','gift'];
// Render Pip's generated expression with the requested size and accessible treatment.
export function pipMarkup(mood='welcome',size=88){const index=Math.max(0,moods.indexOf(mood));return `<span class="pip-sprite" data-pip="${moods[index]}" role="img" aria-label="Pip" style="--pip-size:${size}px;background-position:${index%4*100/3}% ${Math.floor(index/4)*100/3}%"></span>`;}
// Choose an expression that matches the guide's current message or context.
export function guideMood(active,step=0){if(active==='help')return 'curious';if(active?.startsWith('mechanic'))return 'think';if(active?.startsWith('helper'))return 'pointRight';return ({chest:'gift',album:'star',garden:'cheer',wheel:'surprise',welcome:'wave',cards:'pointRight',regatta:'cheer',travel:'curious'})[active]??(step?'pointRight':'reassure');}
export const pipExpressions=Object.freeze(moods);
