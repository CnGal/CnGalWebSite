import { own } from '/_content/CnGalWebSite.MainSite.Shared/scripts/component-lifecycle.js';

export function create(root) {
    const sequence = [0, 1, 2, 4, 7, 6, 5, 3];
    const lifetime = new AbortController();
    let timer, finish, disposed = false;
    function cancel() { clearTimeout(timer); finish?.(false); finish = null; }
    document.addEventListener('cg-navigation-start', cancel, { signal: lifetime.signal });
    return own(root, {
        play(target) {
            cancel();
            if (disposed || !root.isConnected || !sequence.includes(target)) return Promise.resolve(false);
            return new Promise(resolve => {
                finish = resolve;
                let count = 0, index = -1, speed = 200;
                const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)').matches;
                function tick() {
                    if (disposed || !root.isConnected) { cancel(); return; }
                    count++;
                    index = reducedMotion ? sequence.indexOf(target) : (index + 1) % sequence.length;
                    const active = sequence[index];
                    root.querySelectorAll('[data-cg-prize-index]').forEach(element =>
                        element.classList.toggle('active', Number(element.dataset.cgPrizeIndex) === active));
                    if ((count >= 50 || reducedMotion) && active === target) {
                        finish = null; resolve(true); return;
                    }
                    speed = count < 50 / 3 ? Math.max(50, speed - 5)
                        : count < 100 / 3 ? 50 : Math.min(200, speed + 5);
                    timer = setTimeout(tick, speed);
                }
                tick();
            });
        },
        dispose() { if (disposed) return; disposed = true; cancel(); lifetime.abort(); }
    });
}
