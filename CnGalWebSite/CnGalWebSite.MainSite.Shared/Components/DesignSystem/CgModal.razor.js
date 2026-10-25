import { createDialog } from '/_content/CnGalWebSite.MainSite.Shared/scripts/dialog.js';
import { own } from '/_content/CnGalWebSite.MainSite.Shared/scripts/component-lifecycle.js';

export function create(root, callback) {
    let requesting = false, animation, version = 0, disposed = false;
    const lifetime = new AbortController();
    const element = root.querySelector('dialog');
    const dialog = createDialog(element, { signal: lifetime.signal, requestClose: async () => {
        if (requesting || disposed) return;
        requesting = true;
        try { await callback.invokeMethodAsync('RequestClose'); }
        catch (error) { console.error('[CgModal] close request', error); }
        finally { requesting = false; }
    } });
    return own(root, {
        async setOpen(open) {
            const current = ++version;
            animation?.cancel();
            if (disposed) return;
            if (open) { dialog.open(); return; }
            if (!dialog.isOpen) return;
            animation = element.animate([{ opacity: 1, transform: 'scale(1)' }, { opacity: 0, transform: 'scale(.95)' }],
                { duration: matchMedia('(prefers-reduced-motion: reduce)').matches ? 0 : 180 });
            try { await animation.finished; } catch { return; }
            if (current === version && !disposed) dialog.close();
        },
        dispose() { if (disposed) return; disposed = true; version++; animation?.cancel(); lifetime.abort(); dialog.dispose(); }
    });
}
