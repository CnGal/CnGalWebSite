/**
 * CgCarousel — 渐进增强脚本
 * 无 JS 时：SSR 渲染第一张幻灯片（纯静态）
 * 有 JS 时：自动轮播 + 手动切换 + 恢复自动播放
 */
(function () {
    'use strict';

    var SELECTOR_SLIDE = '.cg-carousel__slide';
    var SELECTOR_BTN_PREV = '.cg-carousel__btn--prev';
    var SELECTOR_BTN_NEXT = '.cg-carousel__btn--next';
    var CLASS_ACTIVE = 'cg-carousel__slide--active';
    var ATTR_INIT = 'data-cg-carousel-init';
    var ATTR_DURATION = 'data-cg-carousel-duration';

    function initCarousel(el) {
        if (el.hasAttribute(ATTR_INIT)) return;
        el.setAttribute(ATTR_INIT, '');

        var slides = el.querySelectorAll(SELECTOR_SLIDE);
        var prevBtn = el.querySelector(SELECTOR_BTN_PREV);
        var nextBtn = el.querySelector(SELECTOR_BTN_NEXT);

        if (slides.length <= 1) return;

        var duration = (parseInt(el.getAttribute(ATTR_DURATION)) || 5) * 1000;
        var currentIndex = -1;
        var timer = null;

        // 找到当前 active 的幻灯片索引
        for (var i = 0; i < slides.length; i++) {
            if (slides[i].classList.contains(CLASS_ACTIVE)) {
                currentIndex = i;
                break;
            }
        }
        if (currentIndex < 0) currentIndex = 0;

        function showSlide(index) {
            for (var i = 0; i < slides.length; i++) {
                slides[i].classList.toggle(CLASS_ACTIVE, i === index);
            }
            currentIndex = index;
        }

        function next() {
            showSlide((currentIndex + 1) % slides.length);
            resetTimer();
        }

        function prev() {
            showSlide((currentIndex - 1 + slides.length) % slides.length);
            resetTimer();
        }

        function resetTimer() {
            clearTimeout(timer);
            timer = setTimeout(next, duration);
        }

        if (prevBtn) prevBtn.addEventListener('click', prev);
        if (nextBtn) nextBtn.addEventListener('click', next);

        // 鼠标悬停时暂停自动播放
        el.addEventListener('mouseenter', function () {
            clearTimeout(timer);
        });
        el.addEventListener('mouseleave', function () {
            resetTimer();
        });

        resetTimer();
    }

    // 初始化页面中已有轮播
    document.querySelectorAll('.cg-carousel').forEach(initCarousel);

    // 监听动态添加的轮播（增强导航等场景）
    if (window.MutationObserver) {
        new MutationObserver(function (mutations) {
            for (var m = 0; m < mutations.length; m++) {
                var nodes = mutations[m].addedNodes;
                for (var n = 0; n < nodes.length; n++) {
                    var node = nodes[n];
                    if (node.nodeType !== 1) continue;
                    if (node.classList && node.classList.contains('cg-carousel')) {
                        initCarousel(node);
                    }
                    if (node.querySelectorAll) {
                        var nested = node.querySelectorAll('.cg-carousel');
                        for (var k = 0; k < nested.length; k++) {
                            initCarousel(nested[k]);
                        }
                    }
                }
            }
        }).observe(document.documentElement, { childList: true, subtree: true });
    }
})();
