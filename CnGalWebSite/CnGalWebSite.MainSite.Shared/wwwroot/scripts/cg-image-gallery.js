/**
 * CgImageGallery — 渐进增强脚本
 * 无 JS 时：SSR 渲染首张图片 + 完整缩略图条（纯静态）
 * 有 JS 时：缩略图切换 / 胶片条滚动 / 全屏查看 / 键盘导航
 */
(function () {
    'use strict';

    var SELECTOR_GALLERY = '.cg-image-gallery[data-cg-image-gallery]';
    var ATTR_INIT = 'data-cg-image-gallery-init';

    var CLASS_PREVIEW_ACTIVE = 'cg-image-gallery__preview-slide--active';
    var CLASS_THUMB_ACTIVE = 'cg-image-gallery__thumbnail--active';
    var CLASS_FS_OPEN = 'cg-image-gallery__fullscreen--open';
    var CLASS_FS_CLOSING = 'cg-image-gallery__fullscreen--closing';
    var CLASS_FS_THUMB_ACTIVE = 'cg-image-gallery__fullscreen-thumbnail--active';

    function initGallery(el) {
        if (el.hasAttribute(ATTR_INIT)) return;
        el.setAttribute(ATTR_INIT, '');

        /* ── DOM 引用 ── */
        var previewSlides = el.querySelectorAll('.cg-image-gallery__preview-slide');
        var thumbnails = el.querySelectorAll('.cg-image-gallery__thumbnail');
        var previewArea = el.querySelector('.cg-image-gallery__preview');
        var fullscreenBtn = el.querySelector('.cg-image-gallery__preview-fullscreen-btn');

        var fsContainer = el.querySelector('[data-cg-image-gallery-fullscreen]');
        if (!fsContainer) return;

        var fsImage = fsContainer.querySelector('[data-cg-image-gallery-fs-image]');
        var fsCounter = fsContainer.querySelector('[data-cg-image-gallery-counter]');
        var fsCloseBtn = fsContainer.querySelector('.cg-image-gallery__fullscreen-btn-close');
        var fsBackdrop = fsContainer.querySelector('.cg-image-gallery__fullscreen-backdrop');
        var fsPrevNav = fsContainer.querySelector('.cg-image-gallery__fullscreen-nav--prev');
        var fsNextNav = fsContainer.querySelector('.cg-image-gallery__fullscreen-nav--next');
        var fsThumbnails = fsContainer.querySelectorAll('.cg-image-gallery__fullscreen-thumbnail');

        var total = previewSlides.length;
        if (total === 0) return;

        /* ── 缓存所有图片的 URL ── */
        var imageUrls = [];
        for (var i = 0; i < total; i++) {
            var img = previewSlides[i].querySelector('img');
            imageUrls.push(img ? img.getAttribute('src') : '');
        }

        var currentIndex = 0;
        var isFullscreenOpen = false;
        var fsCloseTimer = null;
        var fsOriginalParent = null;

        /* ──────────────────────────────────
           辅助函数
           ────────────────────────────────── */

        function setMainImage(index) {
            if (index < 0 || index >= total) return;
            currentIndex = index;

            // 更新预览幻灯片
            for (var i = 0; i < previewSlides.length; i++) {
                previewSlides[i].classList.toggle(CLASS_PREVIEW_ACTIVE, i === index);
            }

            // 更新缩略图激活状态
            for (var j = 0; j < thumbnails.length; j++) {
                thumbnails[j].classList.toggle(CLASS_THUMB_ACTIVE, j === index);
            }

            // 滚动缩略图到可见区域
            if (thumbnails[index]) {
                thumbnails[index].scrollIntoView({
                    behavior: 'smooth',
                    block: 'nearest',
                    inline: 'center'
                });
            }
        }

        function setFullscreenImage(index) {
            if (index < 0 || index >= total) return;

            // 更新全屏主图
            if (fsImage && imageUrls[index]) {
                fsImage.setAttribute('src', imageUrls[index]);
            }

            // 更新全屏计数器
            if (fsCounter) {
                fsCounter.textContent = (index + 1) + ' / ' + total;
            }

            // 更新全屏缩略图激活状态
            for (var i = 0; i < fsThumbnails.length; i++) {
                fsThumbnails[i].classList.toggle(CLASS_FS_THUMB_ACTIVE, i === index);
            }

            // 滚动全屏缩略图到可见区域
            if (fsThumbnails[index]) {
                fsThumbnails[index].scrollIntoView({
                    behavior: 'smooth',
                    block: 'nearest',
                    inline: 'center'
                });
            }
        }

        function navigateTo(index) {
            setMainImage(index);
            if (isFullscreenOpen) {
                setFullscreenImage(index);
            }
        }

        function navigateDelta(delta) {
            var newIndex = (currentIndex + delta + total) % total;
            navigateTo(newIndex);
        }

        /* ──────────────────────────────────
           全屏管理
           ────────────────────────────────── */

        function openFullscreen() {
            if (isFullscreenOpen) return;

            // Portal: 将全屏元素移到 body 以突破 isolation: isolate 层叠上下文
            if (!fsOriginalParent) {
                fsOriginalParent = fsContainer.parentNode;
            }
            document.body.appendChild(fsContainer);

            // 取消正在进行的关闭动画
            clearTimeout(fsCloseTimer);
            fsContainer.classList.remove(CLASS_FS_CLOSING);

            fsContainer.classList.add(CLASS_FS_OPEN);
            fsContainer.setAttribute('aria-hidden', 'false');
            isFullscreenOpen = true;

            setFullscreenImage(currentIndex);
            document.body.style.overflow = 'hidden';

            // 聚焦到全屏容器以便键盘导航
            fsContainer.setAttribute('tabindex', '-1');
            fsContainer.focus();
        }

        function closeFullscreen() {
            if (!isFullscreenOpen) return;

            isFullscreenOpen = false;
            fsContainer.classList.add(CLASS_FS_CLOSING);
            fsContainer.setAttribute('aria-hidden', 'true');

            clearTimeout(fsCloseTimer);
            fsCloseTimer = setTimeout(function () {
                fsContainer.classList.remove(CLASS_FS_OPEN);
                fsContainer.classList.remove(CLASS_FS_CLOSING);
                fsContainer.removeAttribute('tabindex');

                // Portal: 关闭动画结束后归还全屏元素至原位置
                if (fsOriginalParent) {
                    fsOriginalParent.appendChild(fsContainer);
                }
            }, 200);

            document.body.style.overflow = '';

            // 关闭时将焦点归还到主预览区
            if (previewArea) {
                previewArea.focus();
            }
        }

        /* ──────────────────────────────────
           事件绑定
           ────────────────────────────────── */

        // 大图预览区点击 → 打开全屏
        if (previewArea) {
            previewArea.addEventListener('click', function (e) {
                // 不全屏按钮自身再触发
                if (e.target === fullscreenBtn || fullscreenBtn.contains(e.target)) {
                    return;
                }
                openFullscreen();
            });
        }

        // 全屏按钮点击
        if (fullscreenBtn) {
            fullscreenBtn.addEventListener('click', function (e) {
                e.stopPropagation();
                openFullscreen();
            });
        }

        // 缩略图点击 → 切换当前图片
        thumbnails.forEach(function (thumb) {
            thumb.addEventListener('click', function () {
                var index = parseInt(thumb.getAttribute('data-index'), 10);
                if (!isNaN(index)) {
                    navigateTo(index);
                }
            });
        });

        // 全屏缩略图点击 → 切换当前图片
        fsThumbnails.forEach(function (thumb) {
            thumb.addEventListener('click', function () {
                var index = parseInt(thumb.getAttribute('data-index'), 10);
                if (!isNaN(index)) {
                    navigateTo(index);
                }
            });
        });

        // 全屏关闭按钮
        if (fsCloseBtn) {
            fsCloseBtn.addEventListener('click', closeFullscreen);
        }

        // 全屏背景点击 → 关闭
        if (fsBackdrop) {
            fsBackdrop.addEventListener('click', function (e) {
                // 只在点击背景本身时关闭（而非子元素）
                if (e.target === fsBackdrop) {
                    closeFullscreen();
                }
            });
        }

        // 全屏导航按钮
        if (fsPrevNav) {
            fsPrevNav.addEventListener('click', function () { navigateDelta(-1); });
        }
        if (fsNextNav) {
            fsNextNav.addEventListener('click', function () { navigateDelta(1); });
        }

        // 全屏主图区域点击 → 关闭全屏（导航按钮除外）
        var fsMain = fsContainer.querySelector('.cg-image-gallery__fullscreen-main');
        if (fsMain) {
            fsMain.addEventListener('click', function (e) {
                if (!e.target.closest('.cg-image-gallery__fullscreen-nav')) {
                    closeFullscreen();
                }
            });
        }

        // 键盘导航
        document.addEventListener('keydown', function (e) {
            if (!isFullscreenOpen) return;

            switch (e.key) {
                case 'Escape':
                    e.preventDefault();
                    closeFullscreen();
                    break;
                case 'ArrowLeft':
                    e.preventDefault();
                    navigateDelta(-1);
                    break;
                case 'ArrowRight':
                    e.preventDefault();
                    navigateDelta(1);
                    break;
            }
        });

        /* ──────────────────────────────────
           胶片条滚动按钮
           ────────────────────────────────── */

        function setupFilmstripScroll(container) {
            if (!container) return;

            var track = container.querySelector('[class*="filmstrip-track"]');
            var prevBtn = container.querySelector('[class*="filmstrip-btn--prev"]');
            var nextBtn = container.querySelector('[class*="filmstrip-btn--next"]');

            if (!track) return;

            var scrollAmount = 200;

            function updateBtnState() {
                var canScroll = track.scrollWidth > track.clientWidth + 1;

                if (!canScroll) {
                    if (prevBtn) prevBtn.style.display = 'none';
                    if (nextBtn) nextBtn.style.display = 'none';
                    return;
                }

                if (prevBtn) prevBtn.style.display = '';
                if (nextBtn) nextBtn.style.display = '';

                var atStart = track.scrollLeft <= 1;
                var atEnd = track.scrollLeft + track.clientWidth >= track.scrollWidth - 1;

                if (prevBtn) {
                    prevBtn.disabled = atStart;
                }
                if (nextBtn) {
                    nextBtn.disabled = atEnd;
                }
            }

            if (prevBtn) {
                prevBtn.addEventListener('click', function () {
                    track.scrollBy({ left: -scrollAmount, behavior: 'smooth' });
                });
            }
            if (nextBtn) {
                nextBtn.addEventListener('click', function () {
                    track.scrollBy({ left: scrollAmount, behavior: 'smooth' });
                });
            }

            track.addEventListener('scroll', updateBtnState, { passive: true });

            // 初始状态
            updateBtnState();

            // 触摸/滚轮时也会更新
            if (window.ResizeObserver) {
                new ResizeObserver(function () {
                    updateBtnState();
                }).observe(track);
            }
        }

        // 安装胶片滚动
        setupFilmstripScroll(el.querySelector('.cg-image-gallery__filmstrip'));
        setupFilmstripScroll(fsContainer.querySelector('.cg-image-gallery__fullscreen-filmstrip'));
    }

    /* ──────────────────────────────────
       初始化与动态监测
       ────────────────────────────────── */

    // 初始化页面中已有的画廊
    document.querySelectorAll(SELECTOR_GALLERY).forEach(initGallery);

    // MutationObserver 处理动态添加的画廊（如 Blazor Enhanced Navigation）
    if (window.MutationObserver) {
        new MutationObserver(function (mutations) {
            for (var m = 0; m < mutations.length; m++) {
                var nodes = mutations[m].addedNodes;
                for (var n = 0; n < nodes.length; n++) {
                    var node = nodes[n];
                    if (node.nodeType !== 1) continue;
                    if (node.matches && node.matches(SELECTOR_GALLERY)) {
                        initGallery(node);
                    }
                    if (node.querySelectorAll) {
                        node.querySelectorAll(SELECTOR_GALLERY).forEach(initGallery);
                    }
                }
            }
        }).observe(document.documentElement, { childList: true, subtree: true });
    }
})();
