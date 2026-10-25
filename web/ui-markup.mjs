export const escapeHtml=value=>String(value).replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
export const actionButton=(label,action,cls='',extra='')=>`<button data-action="${action}" class="${cls}" ${extra}>${label}</button>`;
