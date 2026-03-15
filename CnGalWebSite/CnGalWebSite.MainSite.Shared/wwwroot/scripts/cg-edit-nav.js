/**
 * 词条编辑页侧边栏导航 JS Interop（JS 隔离模块）
 * - scrollToSection: 平滑滚动到指定 section
 * - initSectionObserver: 使用 IntersectionObserver 追踪当前可见 section 并回调 .NET
 * - disposeSectionObserver: 清理 observer
 */
let _observer = null;
let _dotNetRef = null;

/**
 * 平滑滚动到指定 section
 * @param {string} sectionId - section 的 id（如 "section-main"）
 */
export function scrollToSection(sectionId) {
    const el = document.getElementById(sectionId);
    if (el) {
        const headerHeight = 56;
        const y = el.getBoundingClientRect().top + window.scrollY - headerHeight - 16;
        window.scrollTo({ top: y, behavior: 'smooth' });
    }
}

/**
 * 初始化 IntersectionObserver，监听各 section 进入/离开视口
 * @param {object} dotNetRef - .NET 对象引用，用于回调
 * @param {string[]} sectionIds - 要监听的 section id 列表
 */
export function initSectionObserver(dotNetRef, sectionIds) {
    disposeSectionObserver();
    _dotNetRef = dotNetRef;

    const options = {
        root: null,
        rootMargin: '-10% 0px -60% 0px',
        threshold: 0
    };

    _observer = new IntersectionObserver(function (entries) {
        for (const entry of entries) {
            if (entry.isIntersecting) {
                if (_dotNetRef) {
                    _dotNetRef.invokeMethodAsync('OnSectionVisible', entry.target.id);
                }
            }
        }
    }, options);

    for (const id of sectionIds) {
        const el = document.getElementById(id);
        if (el) {
            _observer.observe(el);
        }
    }
}

/**
 * 清理 observer
 */
export function disposeSectionObserver() {
    if (_observer) {
        _observer.disconnect();
        _observer = null;
    }
    _dotNetRef = null;
}
