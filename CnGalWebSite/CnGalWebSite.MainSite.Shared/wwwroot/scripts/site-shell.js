const cookie = name => document.cookie.split(';').map(item => item.trim()).find(item => item.startsWith(`${name}=`))?.slice(name.length + 1);

function setMeta(attribute, name, content) {
    if (typeof content !== 'string' || content.length === 0) return;
    let element = document.head.querySelector(`meta[${attribute}="${name}"]`);
    if (!element) {
        element = document.createElement('meta');
        element.setAttribute(attribute, name);
        document.head.append(element);
    }
    if (element.getAttribute('content') !== content) element.setAttribute('content', content);
}

export function updatePageMeta(title, description, image, structuredData) {
    if (typeof title === 'string' && title.length > 0) document.title = title;
    setMeta('name', 'description', description);
    setMeta('itemprop', 'name', title);
    setMeta('itemprop', 'image', image);

    let script = document.head.querySelector('script[data-cg-page-meta-structured-data]');
    if (typeof structuredData === 'string' && structuredData.length > 0) {
        if (!script) {
            script = document.createElement('script');
            script.type = 'application/ld+json';
            script.dataset.cgPageMetaStructuredData = 'true';
            document.head.append(script);
        }
        if (script.textContent !== structuredData) script.textContent = structuredData;
    } else {
        script?.remove();
    }
}
const setCookie = (name, value) => { document.cookie = `${name}=${value}; path=/; max-age=31536000; samesite=lax${location.protocol === 'https:' ? '; secure' : ''}`; };
const systemTheme = matchMedia('(prefers-color-scheme: dark)');

function applyTheme() {
    const theme = cookie('cg-theme');
    document.documentElement.dataset.theme = ['dark', 'light'].includes(theme) ? theme : systemTheme.matches ? 'dark' : 'light';
    document.dispatchEvent(new Event('cg-theme-change'));
}

export function getMiniMode() { return cookie('cg-mini-mode') === 'true'; }
export function setMiniMode(value) {
    if (value) setCookie('cg-mini-mode', 'true');
    else document.cookie = 'cg-mini-mode=; path=/; max-age=0; samesite=lax';
}
export function scrollToTop() { window.scrollTo({ top: 0, behavior: 'smooth' }); }
export function scrollToBottom(element) { element.scrollTop = element.scrollHeight; }
export function kanbanAlignment(width, height) {
    const rect = document.querySelector('.floating-action-toolbar')?.getBoundingClientRect();
    return rect ? { left: Math.round(Math.max(0, Math.min(rect.right - width - 60, innerWidth - width))),
        top: Math.round(Math.max(0, Math.min(rect.bottom - height, innerHeight - height))) } : null;
}
export async function downloadFileFromStream(name, stream) {
    const url = URL.createObjectURL(new Blob([await stream.arrayBuffer()]));
    try {
        const anchor = document.createElement('a');
        anchor.href = url; anchor.download = name ?? ''; anchor.click();
    } finally { URL.revokeObjectURL(url); }
}

function installStatusTopLayer(blazor) {
    const ids = ['blazor-error-ui', 'components-reconnect-modal'];
    const sync = () => {
        for (const id of ids) {
            const element = document.getElementById(id);
            if (!element || typeof element.showPopover !== 'function') continue;
            if (!element.hasAttribute('popover')) element.setAttribute('popover', 'manual');
            const visible = getComputedStyle(element).display !== 'none';
            let open = false;
            try { open = element.matches(':popover-open'); } catch { }
            if (visible === open) continue;
            try {
                if (visible) element.showPopover();
                else element.hidePopover();
            } catch (error) {
                console.warn(`[status-ui] ${id} top layer`, error);
            }
        }
    };
    const observer = new MutationObserver(sync);
    observer.observe(document.body, { subtree: true, attributes: true, attributeFilter: ['class', 'style'] });
    blazor.addEventListener('enhancedload', sync);
    sync();
}

export function install(blazor) {
    let drawerOpener;
    function drawer(open, trigger) {
        const nav = document.getElementById('cg-drawer');
        nav?.classList.toggle('cg-drawer--open', open);
        document.getElementById('cg-drawer-backdrop')?.classList.toggle('cg-drawer-backdrop--visible', open);
        if (open) { drawerOpener = trigger; nav?.querySelector('button, a')?.focus(); }
        else if (drawerOpener?.isConnected) { drawerOpener.focus({ preventScroll: true }); drawerOpener = null; }
    }
    document.addEventListener('click', event => {
        const target = event.target.closest?.('[data-cg-action]');
        if (!target) return;
        switch (target.dataset.cgAction) {
            case 'drawer-open': drawer(true, target); break;
            case 'drawer-close': drawer(false); break;
            case 'theme-toggle': setCookie('cg-theme', document.documentElement.dataset.theme === 'dark' ? 'light' : 'dark'); applyTheme(); break;
            case 'scroll-top': scrollToTop(); break;
            case 'reload': location.reload(); break;
            case 'audio-select': {
                const playlist = target.closest('.cg-audio-playlist');
                const player = playlist?.querySelector('audio');
                if (!player) break;
                if (player.src === new URL(target.dataset.url, document.baseURI).href) {
                    if (player.paused) player.play().catch(error => console.warn('[audio] play', error)); else player.pause();
                    break;
                }
                player.src = target.dataset.url;
                player.play().catch(error => console.warn('[audio] play', error));
                playlist.querySelector('.cg-audio-title-text').textContent = target.dataset.name;
                const image = playlist.querySelector('.cg-audio-thumbnail');
                image.src = target.dataset.thumbnail; image.style.display = target.dataset.thumbnail ? 'block' : 'none';
                playlist.querySelectorAll('[data-cg-action="audio-select"]').forEach(item => item.classList.toggle('active', item === target));
                break;
            }
        }
    });
    document.addEventListener('keydown', event => { if (event.key === 'Escape') drawer(false); });
    document.addEventListener('keydown', event => {
        if (['Enter', ' '].includes(event.key) && event.target.matches('[data-cg-action="audio-select"]')) {
            event.preventDefault(); event.target.click();
        }
    });
    document.addEventListener('error', event => {
        const target = event.target;
        if (target.matches?.('[data-cg-image-fallback]')) { target.hidden = true; target.parentElement.classList.add('commodity-item-card__image-wrapper--broken'); }
    }, true);
    installStatusTopLayer(blazor);
    systemTheme.addEventListener('change', applyTheme);
    document.addEventListener('cg-navigation-start', () => drawer(false));
    blazor.addEventListener('enhancedload', applyTheme);
    applyTheme();
    const clarity = document.createElement('script');
    window.clarity ??= function () { (window.clarity.q ??= []).push(arguments); };
    clarity.async = true; clarity.src = 'https://www.clarity.ms/tag/buu31hy6os'; document.head.append(clarity);
    const umami = document.createElement('script');
    umami.async = true; umami.src = 'https://umami.cngal.top/script.js';
    Object.assign(umami.dataset, { websiteId: 'c190d5e2-f859-430c-8463-be388f6875b0', tag: 'mainsite', performance: 'true' });
    document.head.append(umami);
}

export function getViewportWidth() { return document.documentElement.clientWidth || innerWidth; }
export function getWindowSize() { return { height: innerHeight, width: getViewportWidth() }; }
