/**
 * 将 DOM 元素导出为 PNG（下载或复制到剪贴板）。
 *
 * 依赖 html2canvas 库，采用懒加载策略：首次调用时动态注入 CDN 脚本，
 * 避免为单个页面付出全局脚本体积。
 *
 * 导出模式：
 *  - copyToClipboard === true  → 使用 Clipboard API 将 PNG 写入剪贴板；
 *  - 否则                       → 触发浏览器下载。
 */

const HTML2CANVAS_SRC = "https://cdn.jsdelivr.net/npm/html2canvas@1.4.1/dist/html2canvas.min.js";
const MIN_CAPTURE_WIDTH = 1000;
const CAPTURE_WIDTH = 1200;
const MOBILE_CLASS_NAME = "cg-sedai-card--mobile";

let _loadingPromise = null;

function loadHtml2Canvas() {
    if (typeof window.html2canvas === "function") {
        return Promise.resolve(window.html2canvas);
    }
    if (_loadingPromise) {
        return _loadingPromise;
    }
    _loadingPromise = new Promise((resolve, reject) => {
        const script = document.createElement("script");
        script.src = HTML2CANVAS_SRC;
        script.async = true;
        script.crossOrigin = "anonymous";
        script.onload = () => {
            if (typeof window.html2canvas === "function") {
                resolve(window.html2canvas);
            } else {
                reject(new Error("html2canvas loaded but global not found"));
            }
        };
        script.onerror = () => {
            _loadingPromise = null;
            reject(new Error("Failed to load html2canvas from CDN"));
        };
        document.head.appendChild(script);
    });
    return _loadingPromise;
}

async function captureElement(element) {
    const html2canvas = await loadHtml2Canvas();

    element.classList.remove(MOBILE_CLASS_NAME);
    const originalWidth = element.style.width;

    if (element.offsetWidth < MIN_CAPTURE_WIDTH) {
        element.style.width = `${CAPTURE_WIDTH}px`;
    }

    try {
        return await html2canvas(element, {
            scale: 2,
            useCORS: true,
            allowTaint: true,
            backgroundColor: null,
            logging: false,
            imageTimeout: 0,
            width: element.offsetWidth,
            height: element.offsetHeight,
            x: 0,
            y: 0,
            scrollX: 0,
            scrollY: 0,
            windowWidth: document.documentElement.offsetWidth,
            windowHeight: document.documentElement.offsetHeight,
        });
    } finally {
        element.style.width = originalWidth;
        element.classList.add(MOBILE_CLASS_NAME);
    }
}

function downloadDataUrl(dataUrl, fileName) {
    const anchor = document.createElement("a");
    anchor.href = dataUrl;
    anchor.download = fileName || "";
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);
}

function canvasToBlob(canvas) {
    return new Promise((resolve, reject) => {
        canvas.toBlob((blob) => {
            if (blob) {
                resolve(blob);
            } else {
                reject(new Error("Failed to create image blob"));
            }
        }, "image/png", 1.0);
    });
}

/**
 * 将指定元素保存为图片。
 * @param {string} id - 目标元素 id
 * @param {string} fileName - 文件名（下载模式使用）
 * @param {boolean} copyToClipboard - true 为复制到剪贴板，false 为下载
 * @returns {Promise<boolean>} true 表示执行成功
 */
export async function saveDivAsImage(id, fileName, copyToClipboard) {
    const element = document.getElementById(id);
    if (!element) {
        throw new Error(`Element '${id}' not found`);
    }

    const canvas = await captureElement(element);

    if (!copyToClipboard) {
        downloadDataUrl(canvas.toDataURL("image/png", 1.0), fileName);
        return true;
    }

    if (!navigator.clipboard || typeof window.ClipboardItem !== "function") {
        // 浏览器不支持剪贴板图片 API，退化为下载。
        downloadDataUrl(canvas.toDataURL("image/png", 1.0), fileName);
        throw new Error("CLIPBOARD_UNSUPPORTED");
    }

    const blob = await canvasToBlob(canvas);
    await navigator.clipboard.write([new ClipboardItem({ "image/png": blob })]);
    return true;
}
