const locks = new Set();
let overflow;

function lockScroll() {
    const token = {};
    if (!locks.size) {
        overflow = document.documentElement.style.overflow;
        document.documentElement.style.overflow = 'hidden';
    }
    locks.add(token);
    return () => {
        if (!locks.delete(token) || locks.size) return;
        document.documentElement.style.overflow = overflow;
    };
}

export function createDialog(dialog, { signal, requestClose } = {}) {
    const lifetime = new AbortController();
    let release;
    let opener;
    let disposed = false;
    const reset = (restoreFocus = true) => {
        release?.(); release = null;
        const target = opener; opener = null;
        if (restoreFocus && target?.isConnected) target.focus({ preventScroll: true });
    };
    const close = (restoreFocus = true) => {
        if (dialog.open) dialog.close();
        reset(restoreFocus);
    };
    dialog.addEventListener('close', () => { if (!dialog.open) reset(); }, { signal: lifetime.signal });
    dialog.addEventListener('cancel', event => {
        event.preventDefault();
        if (requestClose) requestClose(); else close();
    }, { signal: lifetime.signal });
    dialog.addEventListener('click', event => {
        if (event.target !== dialog) return;
        const rect = dialog.getBoundingClientRect();
        if (event.clientX >= rect.left && event.clientX <= rect.right
            && event.clientY >= rect.top && event.clientY <= rect.bottom) return;
        if (requestClose) requestClose(); else close();
    }, { signal: lifetime.signal });
    const dispose = () => {
        if (disposed) return;
        disposed = true;
        lifetime.abort(); close(false);
        signal?.removeEventListener('abort', dispose);
    };
    signal?.addEventListener('abort', dispose, { once: true });
    return {
        open(trigger = document.activeElement) {
            if (disposed || dialog.open || !dialog.isConnected) return;
            opener = trigger;
            try { dialog.showModal(); release = lockScroll(); }
            catch (error) { reset(false); throw error; }
        },
        close, dispose,
        get isOpen() { return dialog.open; }
    };
}
