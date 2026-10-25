import { observeContent } from '/_content/CnGalWebSite.MainSite.Shared/scripts/component-lifecycle.js';

export function mount(root, { signal }) {
    let index = 0, timer, signature = '', suspended = false;
    const motion = matchMedia('(prefers-reduced-motion: reduce)');
    const slides = () => [...root.querySelectorAll('.cg-carousel__slide')].filter(el => el.closest('cg-carousel') === root);
    const stop = () => { clearTimeout(timer); timer = null; };
    const show = next => {
        const items = slides();
        index = items.length ? (next + items.length) % items.length : 0;
        items.forEach((item, i) => {
            item.classList.toggle('cg-carousel__slide--active', i === index);
            item.inert = i !== index;
        });
    };
    function schedule() {
        stop();
        if (signal.aborted || suspended || document.hidden || motion.matches || root.matches(':hover')
            || root.contains(document.activeElement) || slides().length < 2) return;
        timer = setTimeout(() => { show(index + 1); schedule(); }, Math.max(1, Number(root.dataset.cgCarouselDuration) || 5) * 1000);
    }
    function update() {
        const data = JSON.stringify(slides().map(el => [el.querySelector('img')?.getAttribute('src'), el.querySelector('a')?.getAttribute('href')]));
        if (data !== signature) { signature = data; index = 0; }
        show(index); schedule();
    }
    root.addEventListener('click', event => {
        if (event.target.closest('cg-carousel') !== root) return;
        const button = event.target.closest('.cg-carousel__btn');
        if (button) { show(index + (button.matches('.cg-carousel__btn--prev') ? -1 : 1)); schedule(); }
    }, { signal });
    root.addEventListener('mouseenter', stop, { signal });
    root.addEventListener('mouseleave', schedule, { signal });
    root.addEventListener('focusin', stop, { signal });
    root.addEventListener('focusout', () => queueMicrotask(schedule), { signal });
    document.addEventListener('visibilitychange', schedule, { signal });
    motion.addEventListener('change', schedule, { signal });
    const stopObserving = observeContent(root, update, () => true);
    signal.addEventListener('abort', () => { stop(); stopObserving(); }, { once: true });
    return { update, suspend() { suspended = true; stop(); }, resume() { suspended = false; schedule(); }, dispose() { stop(); stopObserving(); } };
}
