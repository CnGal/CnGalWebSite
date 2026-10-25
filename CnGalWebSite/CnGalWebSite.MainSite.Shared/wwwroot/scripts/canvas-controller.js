import { own } from '/_content/CnGalWebSite.MainSite.Shared/scripts/component-lifecycle.js';

export function createCanvasController(root, canvas, draw) {
    let data, disposed = false;
    const lifetime = new AbortController();
    const redraw = () => { if (!disposed && data && canvas.clientWidth && canvas.clientHeight) draw(canvas, ...data); };
    const resize = new ResizeObserver(redraw);
    resize.observe(canvas.parentElement);
    document.addEventListener('cg-theme-change', redraw, { signal: lifetime.signal });
    return own(root, {
        update(...args) { data = args; redraw(); },
        dispose() { if (disposed) return; disposed = true; resize.disconnect(); lifetime.abort(); data = null; }
    });
}
