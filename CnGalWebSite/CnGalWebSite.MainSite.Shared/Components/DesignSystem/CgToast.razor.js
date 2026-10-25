import { own } from '/_content/CnGalWebSite.MainSite.Shared/scripts/component-lifecycle.js';

export function create(root) {
    const items = new Map();
    let disposed = false;
    const remove = element => { clearTimeout(items.get(element)); items.delete(element); element.remove(); };
    return own(root, {
        show(title, detail, variant, duration) {
            if (disposed) return;
            variant = ['info', 'success', 'warning', 'error'].includes(variant) ? variant : 'info';
            const element = root.querySelector(`template[data-variant="${variant}"]`).content.firstElementChild.cloneNode(true);
            element.querySelector('.cg-toast__title').textContent = title;
            const details = element.querySelector('.cg-toast__detail');
            details.textContent = detail ?? ''; details.hidden = !detail;
            root.append(element);
            items.set(element, setTimeout(() => {
                element.classList.add('cg-toast--exit');
                Promise.allSettled(element.getAnimations().map(animation => animation.finished)).then(() => remove(element));
            }, duration));
            while (items.size > 5) remove(items.keys().next().value);
        },
        dispose() { if (disposed) return; disposed = true; [...items.keys()].forEach(remove); }
    });
}
