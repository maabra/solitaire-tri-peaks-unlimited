// Escape user-visible text before inserting it into generated HTML.
export const escapeHtml=value=>String(value).replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
// Build a consistently escaped button carrying the action identifier used by event delegation.
export const actionButton=(label,action,cls='',extra='')=>`<button data-action="${action}" class="${cls}" ${extra}>${label}</button>`;
