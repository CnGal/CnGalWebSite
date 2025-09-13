window.TicketPage = {
    // 获取图片宽高比（宽/高）
    getImageAspect: (url) => new Promise((resolve) => {
        const img = new Image();
        img.onload = () => {
            if (img.naturalWidth && img.naturalHeight) {
                resolve(img.naturalWidth / img.naturalHeight);
            } else {
                resolve(0);
            }
        };
        img.onerror = () => resolve(0);
        img.src = url;
    }),

    // 将宽高比写入 artboard 的行内样式变量 --bg-ar
    setArtboardAspect: (el, aspect) => {
        if (!el) return;
        el.style.setProperty('--bg-ar', aspect);
        // 同时触发一次 reflow（通常不必，但可确保立即生效）
        void el.offsetWidth;
    }
};
