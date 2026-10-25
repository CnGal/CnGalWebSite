import { observeContent } from '/_content/CnGalWebSite.MainSite.Shared/scripts/component-lifecycle.js';
import { createDialog } from '/_content/CnGalWebSite.MainSite.Shared/scripts/dialog.js';

export function mount(root, { signal }) {
    let selected = 0;
    let signature = '';
    let dialogElement;
    let dialog;
    const owned = selector => [...root.querySelectorAll(selector)].filter(el => el.closest('cg-image-gallery') === root);
    const slides = () => owned('.cg-image-gallery__preview-slide');
    const updateFilmstripState = () => {
        for (const track of owned('.cg-image-gallery__filmstrip-track, .cg-image-gallery__fullscreen-filmstrip-track')) {
            const strip = track.parentElement;
            if (!strip) continue;
            const scrollable = track.scrollWidth > track.clientWidth + 1;
            strip.classList.toggle('cg-image-gallery__filmstrip--scrollable', scrollable);
        }
    };
    const resizeObserver = new ResizeObserver(updateFilmstripState);
    resizeObserver.observe(root);
    function render() {
        const items = slides();
        if (!items.length) return;
        selected = Math.min(Math.max(selected, 0), items.length - 1);
        items.forEach((item, i) => item.classList.toggle('cg-image-gallery__preview-slide--active', i === selected));
        for (const [selector, active] of [
            ['.cg-image-gallery__thumbnail', 'cg-image-gallery__thumbnail--active'],
            ['.cg-image-gallery__fullscreen-thumbnail', 'cg-image-gallery__fullscreen-thumbnail--active']]) {
            owned(selector).forEach((item, i) => {
                item.classList.toggle(active, i === selected);
                item.setAttribute('aria-current', i === selected ? 'true' : 'false');
            });
        }
        const image = items[selected].querySelector('img');
        const fullImage = root.querySelector('[data-cg-image-gallery-fs-image]');
        if (fullImage && image) { fullImage.src = image.src; fullImage.alt = image.alt; }
        const counter = root.querySelector('[data-cg-image-gallery-counter]');
        const count = `${selected + 1} / ${items.length}`;
        if (counter && counter.textContent !== count) counter.textContent = count;
        requestAnimationFrame(updateFilmstripState);
    }
    function update() {
        const element = root.querySelector('dialog');
        if (element !== dialogElement) {
            dialog?.dispose(); dialogElement = element;
            dialog = element ? createDialog(element, { signal }) : null;
        }
        const data = JSON.stringify(slides().map(el => [el.getAttribute('href'), el.querySelector('img')?.getAttribute('alt')]));
        if (data !== signature) { dialog?.close(); signature = data; }
        render();
    }
    function choose(index) {
        const count = slides().length;
        if (!count) return;
        selected = (index + count) % count;
        render();
        const track = root.querySelector(dialog?.isOpen
            ? '.cg-image-gallery__fullscreen-filmstrip-track' : '.cg-image-gallery__filmstrip-track');
        const thumb = track?.children[selected];
        if (thumb) track.scrollTo({ left: thumb.offsetLeft - track.offsetLeft - track.clientWidth / 2 + thumb.clientWidth / 2, behavior: 'smooth' });
    }
    root.addEventListener('click', event => {
        if (event.target.closest('cg-image-gallery') !== root) return;
        const target = event.target.closest('a, button, .cg-image-gallery__fullscreen-backdrop');
        if (!target || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey || event.button !== 0) return;
        if (target.matches('.cg-image-gallery__fullscreen-btn-close, .cg-image-gallery__fullscreen-backdrop')) { dialog?.close(); return; }
        if (target.matches('.cg-image-gallery__fullscreen-nav--prev')) { choose(selected - 1); return; }
        if (target.matches('.cg-image-gallery__fullscreen-nav--next')) { choose(selected + 1); return; }
        if (target.matches('[data-index]')) {
            update();
            if (!dialog) return;
            event.preventDefault(); choose(Number(target.dataset.index));
            if (target.matches('.cg-image-gallery__preview-slide')) dialog.open(target);
            return;
        }
        if (target.matches('.cg-image-gallery__preview-fullscreen-btn')) {
            update();
            dialog?.open(target);
            requestAnimationFrame(updateFilmstripState);
            return;
        }
        if (target.className?.includes('filmstrip-btn')) {
            const track = target.parentElement.querySelector('[class$="filmstrip-track"]');
            track?.scrollBy({ left: track.clientWidth * (target.className.includes('--prev') ? -0.8 : 0.8), behavior: 'smooth' });
        }
    }, { signal });
    root.addEventListener('keydown', event => {
        if (event.target.closest('cg-image-gallery') !== root || !['ArrowLeft', 'ArrowRight'].includes(event.key)) return;
        event.preventDefault(); choose(selected + (event.key === 'ArrowLeft' ? -1 : 1));
    }, { signal });
    const stopObserving = observeContent(root, update, record => !record.target.closest?.('dialog')
        && (record.type === 'childList' || record.type === 'attributes'));
    signal.addEventListener('abort', () => {
        stopObserving();
        resizeObserver.disconnect();
    }, { once: true });
    return {
        update,
        suspend: () => dialog?.close(),
        dispose() {
            stopObserving();
            resizeObserver.disconnect();
            dialog?.dispose();
        }
    };
}
