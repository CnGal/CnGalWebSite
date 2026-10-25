import { own } from '/_content/CnGalWebSite.MainSite.Shared/scripts/component-lifecycle.js';

export function create(root) {
    const items = new Map();
    let disposed = false;
    let layerOpen = false;
    const closeLayer = () => {
        if (!layerOpen) return;
        layerOpen = false;
        try { root.hidePopover(); } catch { }
    };
    const remove = element => {
        clearTimeout(items.get(element));
        items.delete(element);
        element.remove();
        if (!items.size) closeLayer();
    };
    const promoteLayer = () => {
        if (typeof root.showPopover !== 'function' || !root.isConnected) return;
        try {
            // Re-open the popover so it is the newest entry in the browser top layer.
            closeLayer();
            root.showPopover();
            layerOpen = true;
        } catch (error) {
            console.error('CgToast: unable to open top layer', error);
        }
    };

    const createFallback = variant => {
        const element = document.createElement('div');
        element.className = `cg-toast cg-toast--${variant}`;
        element.setAttribute('role', 'status');
        const content = document.createElement('div');
        content.className = 'cg-toast__content';
        const title = document.createElement('div');
        title.className = 'cg-toast__title';
        const detail = document.createElement('div');
        detail.className = 'cg-toast__detail';
        content.append(title, detail);
        element.append(content);
        return element;
    };

    const createElement = variant => {
        const template = root.querySelector(`[data-toast-template][data-variant="${variant}"]`);
        const prototype = template?.firstElementChild;
        if (!prototype) {
            console.error(`CgToast: missing toast template for variant "${variant}"`);
            return createFallback(variant);
        }
        const element = prototype.cloneNode(true);
        element.removeAttribute('hidden');
        return element;
    };

    return own(root, {
        show(title, detail, variant, duration) {
            if (disposed) return;
            variant = ['info', 'success', 'warning', 'error'].includes(variant) ? variant : 'info';
            const element = createElement(variant);
            element.querySelector('.cg-toast__title').textContent = title;
            const details = element.querySelector('.cg-toast__detail');
            details.textContent = detail ?? ''; details.hidden = !detail;
            promoteLayer();
            root.append(element);
            const timeout = Number.isFinite(duration) && duration >= 0 ? duration : 3000;
            items.set(element, setTimeout(() => {
                element.classList.add('cg-toast--exit');
                Promise.allSettled(element.getAnimations().map(animation => animation.finished)).then(() => remove(element));
            }, timeout));
            while (items.size > 5) remove(items.keys().next().value);
        },
        dispose() { if (disposed) return; disposed = true; [...items.keys()].forEach(remove); closeLayer(); }
    });
}
