// Only resource downloads are shared. Component state belongs to each controller.
const downloads = new Map();
let sequence = 0;

export function loadAsset(url, kind = 'script') {
    const existing = downloads.get(url);
    if (existing?.element.isConnected) return existing.promise;
    const element = document.createElement(kind === 'style' ? 'link' : 'script');
    if (kind === 'style') { element.rel = 'stylesheet'; element.href = url; }
    else { element.src = url; element.async = true; }
    const promise = new Promise((resolve, reject) => {
        element.onload = () => resolve();
        element.onerror = () => { downloads.delete(url); element.remove(); reject(new Error(`Asset load failed: ${url}`)); };
    });
    downloads.set(url, { element, promise });
    // Enhanced navigation must not remove the library's stylesheet while an editor is mounted.
    element.setAttribute('data-permanent', '');
    element.id = `cg-library-${kind}-${++sequence}`;
    document.head.append(element);
    return promise;
}
