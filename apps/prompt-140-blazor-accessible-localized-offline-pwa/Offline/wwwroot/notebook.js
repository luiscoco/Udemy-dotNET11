export function read() { return localStorage.getItem('course-note') || ''; }
export function save(value) { localStorage.setItem('course-note', value); return true; }
export function language(value) { localStorage.setItem('course-language', value); document.documentElement.lang = value; return true; }
