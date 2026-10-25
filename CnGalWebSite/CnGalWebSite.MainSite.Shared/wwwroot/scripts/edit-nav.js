export function mount(root, { signal }) {
    const scope = root.closest('article, [data-cg-edit-root]') ?? root.parentElement;
    let links = [], sections = [];
    function select(id) {
        links.forEach(link => {
            const active = link.hash === `#${id}`;
            link.classList.toggle('cg-edit-nav-active', active);
            const base = [...link.classList].find(name => name.endsWith('-nav-item'));
            if (base) link.classList.toggle(`${base}--active`, active);
            link.setAttribute('aria-current', active ? 'location' : 'false');
        });
    }
    const observer = new IntersectionObserver(entries => {
        for (const entry of entries) if (entry.isIntersecting) select(entry.target.id);
    }, { rootMargin: '-10% 0px -60% 0px', threshold: 0 });
    function update() {
        links = [...root.querySelectorAll('a[href^="#"]')];
        sections = links.map(link => scope.querySelector(`[id="${CSS.escape(link.hash.slice(1))}"]`)).filter(Boolean);
        observer.disconnect();
        sections.forEach(section => observer.observe(section));
    }
    root.addEventListener('click', event => {
        const link = event.target.closest('a[href^="#"]');
        const section = sections.find(item => `#${item.id}` === link?.hash);
        if (!section) return;
        event.preventDefault();
        window.scrollTo({ top: section.getBoundingClientRect().top + scrollY - 72, behavior: 'smooth' });
    }, { signal });
    signal.addEventListener('abort', () => observer.disconnect(), { once: true });
    return { update, dispose() { observer.disconnect(); } };
}
