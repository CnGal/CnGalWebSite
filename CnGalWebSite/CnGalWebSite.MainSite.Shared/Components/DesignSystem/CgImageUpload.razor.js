import { own } from '/_content/CnGalWebSite.MainSite.Shared/scripts/component-lifecycle.js';

export function create(root, initialRect, ratio, callback) {
    const lifetime = new AbortController();
    let rect = { ...initialRect }, drag, initialized = false;
    const clamp = (value, min, max) => Math.min(max, Math.max(min, value));
    const notify = () => callback.invokeMethodAsync('OnCropChanged', rect.x, rect.y, rect.width, rect.height)
        .catch(error => console.error('[CgImageUpload] crop result', error));
    function initialize() {
        if (initialized || lifetime.signal.aborted || !root.isConnected) return;
        const bounds = root.getBoundingClientRect();
        if (bounds.width <= 0 || bounds.height <= 0) return;
        if (ratio) {
            const stageRatio = bounds.width / bounds.height;
            const size = Math.min(initialRect.width, initialRect.height);
            const width = stageRatio > ratio ? size * ratio / stageRatio : size;
            const height = stageRatio > ratio ? size : size * stageRatio / ratio;
            rect = { x: (1 - width) / 2, y: (1 - height) / 2, width, height };
        }
        initialized = true;
        render();
        notify();
    }
    function render() {
        const overlay = root.querySelector('.cg-image-upload__crop-overlay');
        Object.assign(overlay.style, { left: `${rect.x * 100}%`, top: `${rect.y * 100}%`, width: `${rect.width * 100}%`, height: `${rect.height * 100}%` });
        const shades = root.querySelectorAll('.cg-image-upload__shade');
        const areas = [
            [0, 0, 1, rect.y], [0, rect.y, rect.x, rect.height],
            [rect.x + rect.width, rect.y, 1 - rect.x - rect.width, rect.height],
            [0, rect.y + rect.height, 1, 1 - rect.y - rect.height]
        ];
        shades.forEach((shade, i) => {
            const [x, y, width, height] = areas[i];
            Object.assign(shade.style, { left: `${x * 100}%`, top: `${y * 100}%`, width: `${width * 100}%`, height: `${height * 100}%` });
        });
    }
    root.addEventListener('pointerdown', event => {
        initialize();
        if (!initialized || lifetime.signal.aborted) return;
        if (!event.target.closest('.cg-image-upload__crop-overlay') || event.button !== 0) return;
        event.preventDefault(); root.setPointerCapture(event.pointerId);
        drag = { x: event.clientX, y: event.clientY, rect: { ...rect }, resize: !!event.target.closest('button') };
    }, { signal: lifetime.signal });
    root.addEventListener('pointermove', event => {
        if (!drag) return;
        const bounds = root.getBoundingClientRect();
        if (bounds.width <= 0 || bounds.height <= 0) return;
        const dx = (event.clientX - drag.x) / bounds.width, dy = (event.clientY - drag.y) / bounds.height;
        const start = drag.rect;
        if (!drag.resize) rect = { ...start, x: clamp(start.x + dx, 0, 1 - start.width), y: clamp(start.y + dy, 0, 1 - start.height) };
        else if (!ratio) rect = { ...start, width: clamp(start.width + dx, .08, 1 - start.x), height: clamp(start.height + dy, .08, 1 - start.y) };
        else {
            const stageRatio = bounds.width / bounds.height;
            const max = Math.min(1 - start.x, (1 - start.y) * ratio / stageRatio);
            const min = Math.min(max, Math.max(.08, .08 * ratio / stageRatio));
            const width = clamp(start.width + (Math.abs(dx) >= Math.abs(dy) ? dx : dy * ratio / stageRatio), min, max);
            rect = { ...start, width, height: width * stageRatio / ratio };
        }
        render();
    }, { signal: lifetime.signal });
    const commit = () => {
        if (!drag) return;
        drag = null;
        notify();
    };
    root.addEventListener('pointerup', commit, { signal: lifetime.signal });
    root.addEventListener('lostpointercapture', commit, { signal: lifetime.signal });
    root.addEventListener('pointercancel', commit, { signal: lifetime.signal });
    // The preview can load before its dialog opens; wait for a measurable stage.
    const resize = new ResizeObserver(initialize);
    resize.observe(root);
    initialize();
    return own(root, {
        getRect() {
            initialize();
            if (!initialized || lifetime.signal.aborted) throw new Error('裁剪器尚未准备好，请稍后重试或直接上传');
            return { ...rect };
        },
        dispose() { drag = null; resize.disconnect(); lifetime.abort(); }
    });
}
