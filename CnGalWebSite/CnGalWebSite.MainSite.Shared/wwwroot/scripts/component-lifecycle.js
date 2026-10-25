const connected = new Set();
let navigating = false;

export function registerElement(name, load) {
    if (customElements.get(name)) return;
    customElements.define(name, class extends HTMLElement {
        connectedCallback() {
            connected.add(this);
            this.generation = (this.generation ?? 0) + 1;
            this.lifetime = new AbortController();
            this.ensureMounted();
        }
        async ensureMounted() {
            if (this.controller || this.loading || !this.isConnected) return;
            const generation = this.generation;
            const signal = this.lifetime.signal;
            this.loading = true;
            let controller;
            try {
                const module = await load();
                if (signal.aborted || generation !== this.generation || !this.isConnected) return;
                controller = await module.mount(this, { signal });
                if (signal.aborted || generation !== this.generation || !this.isConnected) {
                    controller?.dispose();
                    return;
                }
                this.controller = controller;
                this.removeAttribute('data-cg-module-error');
                controller.update();
                if (navigating) controller.suspend?.();
            } catch (error) {
                controller?.dispose();
                if (!signal.aborted && generation === this.generation) {
                    this.controller = null;
                    this.setAttribute('data-cg-module-error', '');
                    console.error(`[${name}] mount`, error);
                    this.lifetime.abort();
                    this.lifetime = new AbortController();
                }
            } finally {
                if (generation === this.generation) this.loading = false;
            }
        }
        refresh() {
            if (!this.controller) { this.ensureMounted(); return; }
            try { this.controller.update(); }
            catch (error) { console.error(`[${name}] update`, error); }
        }
        disconnectedCallback() {
            connected.delete(this);
            this.generation++;
            this.lifetime?.abort();
            try { this.controller?.dispose(); }
            finally { this.controller = null; this.loading = false; }
        }
    });
}

export function observeContent(root, update, relevant) {
    let queued = false, disposed = false;
    const observer = new MutationObserver(records => {
        if (queued || !records.some(relevant)) return;
        queued = true;
        queueMicrotask(() => { queued = false; if (!disposed && root.isConnected) update(); });
    });
    observer.observe(root, { childList: true, subtree: true, attributes: true,
        attributeFilter: ['src', 'href', 'alt', 'data-index', 'data-cg-carousel-duration'] });
    return () => { disposed = true; observer.disconnect(); };
}

export function install(blazor) {
    if (!customElements.get('cg-owned')) customElements.define('cg-owned', class extends HTMLElement {
        disconnectedCallback() { this.cgDispose?.(); this.cgDispose = null; }
    });
    const refresh = () => connected.forEach(element => element.refresh());
    window.addEventListener('pagehide', () => connected.forEach(element => element.controller?.suspend?.()));
    window.addEventListener('pageshow', event => {
        if (event.persisted) connected.forEach(element => { element.controller?.resume?.(); element.refresh(); });
    });
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', refresh, { once: true });
    else refresh();
    blazor.addEventListener('enhancednavigationstart', () => {
        navigating = true;
        connected.forEach(element => element.controller?.suspend?.());
        document.dispatchEvent(new Event('cg-navigation-start'));
    });
    blazor.addEventListener('enhancedload', refresh);
    blazor.addEventListener('enhancednavigationend', () => {
        navigating = false;
        connected.forEach(element => { element.controller?.resume?.(); element.refresh(); });
    });
    document.addEventListener('click', event => {
        const retry = event.target.closest?.('[data-cg-module-retry]');
        retry?.closest('[data-cg-module-error]')?.refresh();
    });
}

export function own(root, controller) {
    if (!root?.isConnected) { controller.dispose(); return controller; }
    root.cgDispose?.();
    root.cgDispose = () => controller.dispose();
    return controller;
}
