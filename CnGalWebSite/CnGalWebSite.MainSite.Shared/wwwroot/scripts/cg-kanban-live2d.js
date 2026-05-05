/**
 * CgKanbanLive2D JS Interop 模块
 * 封装 Live2D 看板娘初始化、模型切换、交互事件与资源释放。
 * Live2D CDN 资源按需懒加载，仅在首次初始化时引入。
 * 所有公开 API 以 ES Module 导出，供 Blazor JSInterop 调用。
 */

const LIVE2D_CORE_JS_URL = 'https://res.cngal.org/live2d/js/live2dcubismcore.min.js';
const LIVE2D_BUNDLE_JS_URL = 'https://res.cngal.org/live2d/js/bundle.js';

/** @type {object|null} */
let _dotNetHelper = null;
/** @type {boolean} */
let _cbReady = false;

// 清理追踪
/** @type {Array<{target: EventTarget, type: string, handler: Function, options: (object|boolean)}>} */
let _cleanupListeners = [];
/** @type {ResizeObserver|null} */
let _resizeObserver = null;
/** @type {Function|null} */
let _resizeDebouncedHandler = null;
/** @type {string[]} */
let _blobUrls = [];

// 拖动/缩放冲突检测状态
let _kanbanMousedown = false;
let _buttongroupMousedown = false;
let _dialogboxMousedown = false;
let _chatcardMousedown = false;
let _kanbanResizing = false;

// 看板娘尺寸常量（最小固定，最大动态计算为屏幕80%）
const KANBAN_MIN_SIZE = 150;

/**
 * 获取当前窗口下的最大尺寸（取宽高中的较小值 × 80%）
 * @returns {number}
 */
function getKanbanMaxSize() {
    return Math.floor(Math.min(window.innerWidth, window.innerHeight) * 0.8);
}

// ---------------------------------------------------------------------------
// CDN 加载工具
//  注：每次调用都移除旧脚本并重新加载，确保 bundle.js 的全局状态（WebGL context 等）
//  被完全重置。这是从旧站点 live2d-core.js 继承的经验——bundle.js 不提供干净的
//  release 方法，只能通过重新执行脚本来重置状态。
// ---------------------------------------------------------------------------

/**
 * 强制加载 JS 文件（总是创建新 script 元素以触发重新执行）
 * @param {string} src
 * @returns {Promise<void>}
 */
function loadScriptFresh(src) {
    var old = document.querySelector('script[src="' + src + '"]');
    if (old) old.remove();

    return new Promise(function (resolve, reject) {
        var script = document.createElement('script');
        script.src = src;
        script.onload = resolve;
        script.onerror = function () { reject(new Error('Failed to load script: ' + src)); };
        document.body.appendChild(script);
    });
}

/**
 * 加载 Live2D CDN 资源（每次调用都强制重新加载以重置全局状态）
 * @returns {Promise<void>}
 */
function ensureLive2dLoaded() {
    return loadScriptFresh(LIVE2D_CORE_JS_URL)
        .then(function () { return loadScriptFresh(LIVE2D_BUNDLE_JS_URL); });
}

// ---------------------------------------------------------------------------
// 内部工具函数
// ---------------------------------------------------------------------------

/**
 * 获取当前时间戳
 * @returns {number}
 */
function getTimeNow() {
    return Date.now();
}

/**
 * 注册监听器并记录用于清理
 * @param {EventTarget} target
 * @param {string} type
 * @param {Function} handler
 * @param {(object|boolean)} [options]
 */
function addTrackedListener(target, type, handler, options) {
    target.addEventListener(type, handler, options);
    _cleanupListeners.push({ target: target, type: type, handler: handler, options: options });
}

/**
 * 垂直翻转图像像素（WebGL readPixels 输出是上下颠倒的）
 * @param {Uint8Array} pixels
 * @param {number} width
 * @param {number} height
 */
function flipImageVertically(pixels, width, height) {
    var halfHeight = Math.floor(height / 2);
    for (var y = 0; y < halfHeight; y++) {
        for (var x = 0; x < width; x++) {
            for (var c = 0; c < 4; c++) {
                var topIndex = (y * width + x) * 4 + c;
                var bottomIndex = ((height - y - 1) * width + x) * 4 + c;
                var temp = pixels[topIndex];
                pixels[topIndex] = pixels[bottomIndex];
                pixels[bottomIndex] = temp;
            }
        }
    }
}

/**
 * 使用 ResizeObserver 监听 Live2D 画布尺寸变化
 */
function listenLive2dCanvasChange() {
    if (_resizeObserver) {
        _resizeObserver.disconnect();
        _resizeObserver = null;
    }
    var canvas = document.getElementById('live2d');
    if (!canvas) return;
    _resizeObserver = new ResizeObserver(function () {
        resizeLive2d();
    });
    _resizeObserver.observe(canvas);
}

/**
 * 防抖函数
 * @param {Function} fn
 * @param {number} delay
 * @returns {Function}
 */
function debounce(fn, delay) {
    var timer;
    return function () {
        if (timer) {
            clearTimeout(timer);
        }
        timer = setTimeout(function () {
            fn();
        }, delay);
    };
}

// ---------------------------------------------------------------------------
// 全局回调（Live2D 引擎调用）
// ---------------------------------------------------------------------------

/** 初始化完成回调 */
window.Live2dInitCallback = function () {
    console.log('cg-kanban: Live2dInitCallback fired');
    _cbReady = true;
    if (_dotNetHelper) {
        _dotNetHelper.invokeMethodAsync('Live2dInitCallback');
    }
};

/** 摸头事件 */
window.Live2dHitHeadEvent = function () {
    console.log('cg-kanban: Live2dHitHeadEvent fired');
    if (_dotNetHelper) {
        _dotNetHelper.invokeMethodAsync('TriggerCustomEventAsync', '摸头');
    }
};

/** 触碰身体事件 */
window.Live2dHitBodyEvent = function () {
    console.log('cg-kanban: Live2dHitBodyEvent fired');
    if (_dotNetHelper) {
        _dotNetHelper.invokeMethodAsync('TriggerCustomEventAsync', '触碰身体');
    }
};

// ---------------------------------------------------------------------------
// 公开 API（ES Module 导出）
// ---------------------------------------------------------------------------

/**
 * 初始化看板娘 Live2D
 * @param {object} dotNetRef - .NET 互操作引用
 * @param {string} modelDir - 模型目录（逗号分隔）
 * @param {number} modelIndex - 默认模型索引
 * @param {string} resourcesPath - 资源文件路径
 */
export function initKanbanLive2D(dotNetRef, modelDir, modelIndex, resourcesPath) {
    console.log('cg-kanban: initKanbanLive2D called', { modelDir: modelDir, modelIndex: modelIndex, resourcesPath: resourcesPath });
    _dotNetHelper = dotNetRef;
    ensureLive2dLoaded().then(function () {
        console.log('cg-kanban: CDN loaded, ready to init Live2D');
        var modelDirs = modelDir.split(',');
        setLive2dDefine(resourcesPath, '', modelDirs, 'live2d', modelIndex);
        initLive2d();
        initKanbanMoveAction(dotNetRef);
        initKanbanResizeAction(dotNetRef);
        initButtonGroupMoveAction(dotNetRef);
        initDialogBoxMoveAction(dotNetRef);
        initChatCardMoveAction(dotNetRef);
        listenLive2dCanvasChange();

        var getWindowInfo = function () {
            if (_dotNetHelper) {
                _dotNetHelper.invokeMethodAsync('CheckKanbanPositionAsync');
            }
        };
        _resizeDebouncedHandler = debounce(getWindowInfo, 100);
        addTrackedListener(window, 'resize', _resizeDebouncedHandler);

        console.log("初始化Live2D");
    });
}

/**
 * 切换模型
 * @param {number} id
 */
export function switchLiv2DModel(id) {
    console.log('cg-kanban: switchLiv2DModel called', id);
    var manger = getLive2dManager();
    manger.changeScene(id);
}

/**
 * 切换表情
 * @param {string} id - 带值则设置表情，不带值则清除所有表情
 */
export function switchLiv2DExpression(id) {
    console.log('cg-kanban: switchLiv2DExpression called', id);
    var manger = getLive2dManager();
    var model = manger.getCurrentModel();
    if (id) {
        model.setExpression(id);
    } else {
        model.cleanExpressions();
    }
}

/**
 * 切换衣服
 * @param {string} id - 带值则设置衣服，不带值则清除衣服
 */
export function switchLiv2DClothes(id) {
    console.log('cg-kanban: switchLiv2DClothes called', id);
    var manger = getLive2dManager();
    var model = manger.getCurrentModel();
    if (id) {
        model.setClothes(id);
    } else {
        model.cleanClothes();
    }
}

/**
 * 切换丝袜
 * @param {string} id - 带值则设置丝袜，不带值则清除丝袜
 */
export function switchLiv2DStockings(id) {
    console.log('cg-kanban: switchLiv2DStockings called', id);
    var manger = getLive2dManager();
    var model = manger.getCurrentModel();
    if (id) {
        model.setStockings(id);
    } else {
        model.cleanStockings();
    }
}

/**
 * 切换鞋子
 * @param {string} id - 带值则设置鞋子，不带值则清除鞋子
 */
export function switchLiv2DShoes(id) {
    console.log('cg-kanban: switchLiv2DShoes called', id);
    var manger = getLive2dManager();
    var model = manger.getCurrentModel();
    if (id) {
        model.setShoes(id);
    } else {
        model.cleanShoes();
    }
}

/**
 * 切换动作
 * @param {string} group - 动作分组
 * @param {number} index - 动作索引（falsy 则随机动作，0 为 falsy）
 */
export function switchLiv2DMotion(group, index) {
    console.log('cg-kanban: switchLiv2DMotion called', { group: group, index: index });
    var manger = getLive2dManager();
    var model = manger.getCurrentModel();
    if (index) {
        model.startMotion(group, index, 3);
    } else {
        model.startRandomMotion(group, 3);
    }
}

/**
 * 生成看板娘图片（WebGL 截图）
 * @param {object} dotNetRef - .NET 互操作引用
 * @param {number} x - 起始 X 坐标
 * @param {number} y - 起始 Y 坐标
 * @param {number} height - 截图高度
 * @param {number} width - 截图宽度
 */
export function startKanbanImageGeneration(dotNetRef, x, y, height, width) {
    console.log('cg-kanban: startKanbanImageGeneration called', { x: x, y: y, height: height, width: width });
    var canvas = document.getElementById('live2d');
    if (!canvas) {
        console.error('cg-kanban: #live2d canvas not found');
        return;
    }
    var gl = canvas.getContext('webgl');
    if (!gl) {
        console.error('cg-kanban: WebGL context not available');
        return;
    }
    var pixels = new Uint8Array(width * height * 4);
    gl.readPixels(x, y, width, height, gl.RGBA, gl.UNSIGNED_BYTE, pixels);
    flipImageVertically(pixels, width, height);

    var offscreenCanvas = document.getElementById('live2d-cache');
    if (!offscreenCanvas) {
        console.error('cg-kanban: #live2d-cache canvas not found');
        return;
    }
    offscreenCanvas.width = width;
    offscreenCanvas.height = height;
    var ctx = offscreenCanvas.getContext('2d');
    var imageData = new ImageData(new Uint8ClampedArray(pixels), width, height);
    ctx.putImageData(imageData, 0, 0);

    offscreenCanvas.toBlob(function (blob) {
        var url = URL.createObjectURL(blob);
        _blobUrls.push(url);
        console.log('cg-kanban: image blob URL created', url);
        dotNetRef.invokeMethodAsync('OnKanbanImageGenerated', url);
    }, 'image/png');
}

/**
 * 释放 Live2D 资源并清理所有事件监听器
 */
export function releaseLive2D() {
    console.log('cg-kanban: releaseLive2D called');

    // Release Live2D manager
    try {
        var manager = getLive2dManager();
        if (manager) {
            manager.release();
        }
    } catch (e) {
        console.warn('cg-kanban: error releasing Live2D manager', e);
    }

    // Disconnect ResizeObserver
    if (_resizeObserver) {
        _resizeObserver.disconnect();
        _resizeObserver = null;
    }

    // Remove all tracked event listeners
    for (var i = 0; i < _cleanupListeners.length; i++) {
        var item = _cleanupListeners[i];
        item.target.removeEventListener(item.type, item.handler, item.options);
    }
    _cleanupListeners = [];

    // Clean up body class (in case release is called during an active drag)
    document.body.classList.remove('user-select-none');

    // Revoke all blob URLs
    for (var j = 0; j < _blobUrls.length; j++) {
        URL.revokeObjectURL(_blobUrls[j]);
    }
    _blobUrls = [];

    // Reset state
    _dotNetHelper = null;
    _cbReady = false;
    _kanbanMousedown = false;
    _buttongroupMousedown = false;
    _dialogboxMousedown = false;
    _chatcardMousedown = false;
    _kanbanResizing = false;
    _resizeDebouncedHandler = null;
}

/**
 * 初始化看板娘拖拽（优化版 — 含光标反馈、边界钳制、视觉指示）
 * @param {object} dotNetRef - .NET 互操作引用
 */
export function initKanbanMoveAction(dotNetRef) {
    console.log('cg-kanban: initKanbanMoveAction called');
    var live2dItem = document.getElementById('kanban-live2d');
    if (!live2dItem) return;

    var move = false;
    var deltaLeft = 0, deltaTop = 0;
    var x_org, y_org;
    var dx = 0;
    var dy = 0;
    var time;

    var MAX_LEFT, MAX_TOP;
    var MIN_LEFT, MIN_TOP;
    function updateBounds() {
        // 确保至少 50% 看板娘在屏幕内可见
        MIN_LEFT = -(live2dItem.offsetWidth * 0.5);
        MIN_TOP = -(live2dItem.offsetHeight * 0.5);
        MAX_LEFT = window.innerWidth - live2dItem.offsetWidth * 0.5;
        MAX_TOP = window.innerHeight - live2dItem.offsetHeight * 0.5;
    }

    var mousedown_fun = function (event) {
        var touch;
        if (event.touches) {
            touch = event.touches[0];
        } else {
            touch = event;
        }
        var timeStart = getTimeNow();
        _kanbanMousedown = true;

        // 按住 300ms 后添加视觉反馈（即将进入拖拽状态）
        var dragReady = false;
        time = setInterval(function () {
            var timeEnd = getTimeNow();
            if (timeEnd - timeStart > 300 && !dragReady) {
                dragReady = true;
                live2dItem.classList.add('drag-ready');
            }
            if (timeEnd - timeStart > 500) {
                clearInterval(time);
                if (_buttongroupMousedown || _dialogboxMousedown || _kanbanResizing) {
                    live2dItem.classList.remove('drag-ready');
                    return;
                }
                deltaLeft = touch.clientX - touch.target.offsetLeft;
                deltaTop = touch.clientY - touch.target.offsetTop;
                var rect = live2dItem.getBoundingClientRect();
                x_org = rect.x;
                y_org = rect.y;
                move = true;
                live2dItem.classList.remove('drag-ready');
                live2dItem.classList.add('dragging');
                document.body.classList.add('user-select-none');
                switchLiv2DExpression('expression1');
                updateBounds();
            }
        }, 10);
    };

    addTrackedListener(live2dItem, 'mousedown', mousedown_fun);
    addTrackedListener(live2dItem, 'touchstart', mousedown_fun, { passive: false });

    var mousemove_fun = function (event) {
        _kanbanMousedown = false;
        var touch;
        if (event.touches) {
            touch = event.touches[0];
        } else {
            touch = event;
        }

        if (move) {
            event.preventDefault();
            var cx = touch.clientX;
            var cy = touch.clientY;
            dx = cx - deltaLeft;
            dy = cy - deltaTop;

            // 实时边界钳制：至少 50% 在屏幕内可见
            var newLeft = Math.max(MIN_LEFT, Math.min(x_org + dx, MAX_LEFT));
            var newTop = Math.max(MIN_TOP, Math.min(y_org + dy, MAX_TOP));

            live2dItem.setAttribute('style', 'left:' + newLeft + 'px; top:' + newTop + 'px; width: ' + live2dItem.offsetWidth + 'px; height: ' + live2dItem.offsetHeight + 'px;');
        } else {
            clearInterval(time);
            live2dItem.classList.remove('drag-ready');
        }
    };

    addTrackedListener(window, 'mousemove', mousemove_fun);
    addTrackedListener(window, 'touchmove', mousemove_fun, { passive: false });

    var mouseup_fun = function () {
        live2dItem.classList.remove('dragging');
        if (move) {
            move = false;
            var finalLeft = Math.max(MIN_LEFT, Math.min(x_org + dx, MAX_LEFT));
            var finalTop = Math.max(MIN_TOP, Math.min(y_org + dy, MAX_TOP));
            dotNetRef.invokeMethodAsync('SetKanbanPosition', Math.round(finalLeft), Math.round(finalTop));
            document.body.classList.remove('user-select-none');
            switchLiv2DExpression();
        } else {
            clearInterval(time);
            live2dItem.classList.remove('drag-ready');
        }
    };

    addTrackedListener(window, 'mouseup', mouseup_fun);
    addTrackedListener(window, 'touchend', mouseup_fun, { passive: false });
}

/**
 * 初始化看板娘拖拽缩放（自由宽高比）
 * @param {object} dotNetRef - .NET 互操作引用
 */
export function initKanbanResizeAction(dotNetRef) {
    console.log('cg-kanban: initKanbanResizeAction called');
    var live2dItem = document.getElementById('kanban-live2d');
    var handle = live2dItem && live2dItem.querySelector('.kanban-resize-handle');
    if (!live2dItem || !handle) return;

    var resizing = false;
    var startX, startY, startWidth, startHeight;

    var mousedown_fun = function (event) {
        if (event.target !== handle && !handle.contains(event.target)) return;
        event.preventDefault();
        event.stopPropagation();
        resizing = true;
        _kanbanResizing = true;
        var touch = event.touches ? event.touches[0] : event;
        startX = touch.clientX;
        startY = touch.clientY;
        var rect = live2dItem.getBoundingClientRect();
        startWidth = rect.width;
        startHeight = rect.height;
        live2dItem.classList.add('resizing');
        document.body.classList.add('user-select-none');
    };

    addTrackedListener(handle, 'mousedown', mousedown_fun);
    addTrackedListener(handle, 'touchstart', mousedown_fun, { passive: false });

    function clampAspectRatio(w, h) {
        var ratio = w / h;
        if (ratio > 2.5) { return [h * 2.5, h]; }
        if (ratio < 0.4) { return [w, w / 0.4]; }
        return [w, h];
    }

    function applyNewSize(w, h) {
        var clamped = clampAspectRatio(w, h);
        var maxSize = getKanbanMaxSize();
        var newW = Math.round(Math.max(KANBAN_MIN_SIZE, Math.min(maxSize, clamped[0])));
        var newH = Math.round(Math.max(KANBAN_MIN_SIZE, Math.min(maxSize, clamped[1])));
        live2dItem.style.width = newW + 'px';
        live2dItem.style.height = newH + 'px';
        var canvas = document.getElementById('live2d');
        if (canvas) {
            canvas.width = newW;
            canvas.height = newH;
        }
        return [newW, newH];
    }

    var mousemove_fun = function (event) {
        if (!resizing) return;
        event.preventDefault();
        var touch = event.touches ? event.touches[0] : event;
        var dx = touch.clientX - startX;
        var dy = touch.clientY - startY;
        applyNewSize(startWidth + dx, startHeight + dy);
    };

    addTrackedListener(window, 'mousemove', mousemove_fun);
    addTrackedListener(window, 'touchmove', mousemove_fun, { passive: false });

    var mouseup_fun = function () {
        if (!resizing) return;
        resizing = false;
        _kanbanResizing = false;
        live2dItem.classList.remove('resizing');
        document.body.classList.remove('user-select-none');

        var rect = live2dItem.getBoundingClientRect();
        var canvas = document.getElementById('live2d');
        if (canvas) {
            canvas.width = Math.round(rect.width);
            canvas.height = Math.round(rect.height);
        }
        dotNetRef.invokeMethodAsync('SetKanbanSize', Math.round(rect.width), Math.round(rect.height));
    };

    addTrackedListener(window, 'mouseup', mouseup_fun);
    addTrackedListener(window, 'touchend', mouseup_fun, { passive: false });
}

/**
 * 初始化按钮组拖拽
 * @param {object} dotNetRef - .NET 互操作引用
 */
export function initButtonGroupMoveAction(dotNetRef) {
    console.log('cg-kanban: initButtonGroupMoveAction called');
    var live2dItem = document.getElementById('kanban-live2d');
    var groupItem = document.getElementById('kanban-button-group');
    if (!live2dItem || !groupItem) return;

    var move = false;
    var deltaLeft = 0, deltaTop = 0;
    var x_org, y_org;
    var dx = 0;
    var dy = 0;
    var time;

    var mousedown_fun = function (event) {
        var touch;
        if (event.touches) {
            touch = event.touches[0];
        } else {
            touch = event;
        }
        var timeStart = getTimeNow();
        _buttongroupMousedown = true;

        time = setInterval(function () {
            var timeEnd = getTimeNow();
            if (timeEnd - timeStart > 500) {
                clearInterval(time);
                if (_dialogboxMousedown) {
                    return;
                }
                deltaLeft = touch.clientX - touch.target.offsetLeft;
                deltaTop = touch.clientY - touch.target.offsetTop;
                var rect_w = live2dItem.getBoundingClientRect();
                var rect_n = groupItem.getBoundingClientRect();
                x_org = rect_w.x;
                y_org = rect_w.y;
                move = true;
                document.body.classList.add('user-select-none');
            }
        }, 10);
    };

    addTrackedListener(groupItem, 'mousedown', mousedown_fun);
    addTrackedListener(groupItem, 'touchstart', mousedown_fun, { passive: false });

    var mousemove_fun = function (event) {
        if (move) {
            var touch;
            if (event.touches) {
                touch = event.touches[0];
            } else {
                touch = event;
            }
            event.preventDefault();
            var cx = touch.clientX;
            var cy = touch.clientY;
            dx = cx - x_org;
            dy = cy - y_org;
            groupItem.setAttribute('style', 'left:' + dx + 'px; top:' + dy + 'px;');
        } else {
            clearInterval(time);
        }
    };

    addTrackedListener(window, 'mousemove', mousemove_fun);
    addTrackedListener(window, 'touchmove', mousemove_fun, { passive: false });

    var mouseup_fun = function () {
        _buttongroupMousedown = false;
        var rect_w = live2dItem.getBoundingClientRect();
        var rect_n = groupItem.getBoundingClientRect();
        if (move) {
            move = false;
            dotNetRef.invokeMethodAsync('SetButtonGroupPosition', dx, rect_w.height - dy - rect_n.height);
            document.body.classList.remove('user-select-none');
        } else {
            clearInterval(time);
        }
    };

    addTrackedListener(window, 'mouseup', mouseup_fun);
    addTrackedListener(window, 'touchend', mouseup_fun, { passive: false });
}

/**
 * 初始化对话框拖拽（事件委托模式，绑定在 #kanban-live2d 上，动态检测 #kanban-dialogbox）
 * @param {object} dotNetRef - .NET 互操作引用
 */
export function initDialogBoxMoveAction(dotNetRef) {
    console.log('cg-kanban: initDialogBoxMoveAction called');
    var live2dItem = document.getElementById('kanban-live2d');
    if (!live2dItem) return;

    var move = false;
    var deltaLeft = 0, deltaTop = 0;
    var x_org, y_org;
    var dx = 0;
    var dy = 0;
    var time;

    var mousedown_fun = function (event) {
        // 动态查找对话框元素（可能因 Blazor 重渲染而变化）
        var dialogItem = document.getElementById('kanban-dialogbox');
        // 排除关闭按钮上的点击
        if (!dialogItem || !dialogItem.contains(event.target) || event.target.closest('.kanban-dialogbox__close')) return;

        var touch;
        if (event.touches) {
            touch = event.touches[0];
        } else {
            touch = event;
        }
        var timeStart = getTimeNow();
        _dialogboxMousedown = true;

        time = setInterval(function () {
            var timeEnd = getTimeNow();
            if (timeEnd - timeStart > 500) {
                clearInterval(time);
                deltaLeft = touch.clientX - touch.target.offsetLeft;
                deltaTop = touch.clientY - touch.target.offsetTop;
                var currentDialog = document.getElementById('kanban-dialogbox');
                if (!currentDialog) return;
                var rect_w = live2dItem.getBoundingClientRect();
                var rect_n = currentDialog.getBoundingClientRect();
                x_org = rect_w.x - rect_n.x;
                y_org = rect_w.y - rect_n.y;
                move = true;
                currentDialog.classList.add('kanban-dialogbox--dragging');
                document.body.classList.add('user-select-none');
            }
        }, 10);
    };

    addTrackedListener(live2dItem, 'mousedown', mousedown_fun);
    addTrackedListener(live2dItem, 'touchstart', mousedown_fun, { passive: false });

    var mousemove_fun = function (event) {
        if (move) {
            var touch;
            if (event.touches) {
                touch = event.touches[0];
            } else {
                touch = event;
            }
            event.preventDefault();
            var cx = touch.clientX;
            var cy = touch.clientY;
            dx = cx - deltaLeft - x_org;
            dy = cy - deltaTop - y_org;
            var currentDialog = document.getElementById('kanban-dialogbox');
            if (currentDialog) {
                currentDialog.setAttribute('style', 'left:' + dx + 'px; top:' + dy + 'px;');
            }
        } else {
            clearInterval(time);
        }
    };

    addTrackedListener(window, 'mousemove', mousemove_fun);
    addTrackedListener(window, 'touchmove', mousemove_fun, { passive: false });

    var mouseup_fun = function () {
        _dialogboxMousedown = false;
        if (move) {
            move = false;
            var currentDialog = document.getElementById('kanban-dialogbox');
            if (currentDialog) {
                currentDialog.classList.remove('kanban-dialogbox--dragging');
                var rect_w = live2dItem.getBoundingClientRect();
                var rect_n = currentDialog.getBoundingClientRect();
                dotNetRef.invokeMethodAsync('SetDialogBoxPosition', dx, rect_w.height - dy - rect_n.height);
            }
            document.body.classList.remove('user-select-none');
        } else {
            clearInterval(time);
        }
    };

    addTrackedListener(window, 'mouseup', mouseup_fun);
    addTrackedListener(window, 'touchend', mouseup_fun, { passive: false });
}

/**
 * 初始化聊天卡片拖拽
 * @param {object} dotNetRef - .NET 互操作引用
 */
export function initChatCardMoveAction(dotNetRef) {
    console.log('cg-kanban: initChatCardMoveAction called');
    var live2dItem = document.getElementById('kanban-live2d');
    var groupItem = document.getElementById('kanban-chatcard');
    if (!live2dItem || !groupItem) return;

    var move = false;
    var deltaLeft = 0, deltaTop = 0;
    var x_org, y_org;
    var dx = 0;
    var dy = 0;
    var time;

    var mousedown_fun = function (event) {
        var touch;
        if (event.touches) {
            touch = event.touches[0];
        } else {
            touch = event;
        }
        var timeStart = getTimeNow();
        _chatcardMousedown = true;

        time = setInterval(function () {
            var timeEnd = getTimeNow();
            if (timeEnd - timeStart > 500) {
                clearInterval(time);
                deltaLeft = touch.clientX - touch.target.offsetLeft;
                deltaTop = touch.clientY - touch.target.offsetTop;
                var rect_w = live2dItem.getBoundingClientRect();
                var rect_n = groupItem.getBoundingClientRect();
                x_org = rect_w.x - rect_n.x;
                y_org = rect_w.y - rect_n.y;
                move = true;
                document.body.classList.add('user-select-none');
            }
        }, 10);
    };

    addTrackedListener(groupItem, 'mousedown', mousedown_fun);
    addTrackedListener(groupItem, 'touchstart', mousedown_fun, { passive: false });

    var mousemove_fun = function (event) {
        if (move) {
            var touch;
            if (event.touches) {
                touch = event.touches[0];
            } else {
                touch = event;
            }
            event.preventDefault();
            var cx = touch.clientX;
            var cy = touch.clientY;
            dx = cx - deltaLeft - x_org;
            dy = cy - deltaTop - y_org;
            groupItem.setAttribute('style', 'left:' + dx + 'px; top:' + dy + 'px;');
        } else {
            clearInterval(time);
        }
    };

    addTrackedListener(window, 'mousemove', mousemove_fun);
    addTrackedListener(window, 'touchmove', mousemove_fun, { passive: false });

    var mouseup_fun = function () {
        _chatcardMousedown = false;
        if (move) {
            move = false;
            var rect_w = live2dItem.getBoundingClientRect();
            var rect_n = groupItem.getBoundingClientRect();
            dotNetRef.invokeMethodAsync('SetChatCardPosition', dx, rect_w.height - dy - rect_n.height);
            document.body.classList.remove('user-select-none');
        } else {
            clearInterval(time);
        }
    };

    addTrackedListener(window, 'mouseup', mouseup_fun);
    addTrackedListener(window, 'touchend', mouseup_fun, { passive: false });
}

/**
 * 初始化鼠标悬停事件
 * @param {object} dotNetRef - .NET 互操作引用
 * @param {Array<{selector: string, id: (string|number)}>} data - 悬停区域配置
 */
export function initMouseOverEvent(dotNetRef, data) {
    console.log('cg-kanban: initMouseOverEvent called', data);
    var lastHoverElement;

    var mouseoverHandler = function (event) {
        for (var i = 0; i < data.length; i++) {
            var item = data[i];
            if (!event.target.closest(item.selector)) continue;
            if (lastHoverElement === item.selector) return;
            lastHoverElement = item.selector;
            console.log('cg-kanban: mouseover triggered', item.selector);
            dotNetRef.invokeMethodAsync('TriggerMouseOverEventAsync', item.id);
            return;
        }
    };

    addTrackedListener(window, 'mouseover', mouseoverHandler);
}

/**
 * 回到顶部
 */
export function scrollToTop() {
    console.log('cg-kanban: scrollToTop called');
    window.scrollTo({
        top: 0,
        behavior: 'smooth'
    });
}

/**
 * 获取窗口尺寸
 * @returns {{Height: number, Width: number}}
 */
export function getWindowSize() {
    console.log('cg-kanban: getWindowSize called');
    return {
        Height: window.innerHeight,
        Width: window.innerWidth
    };
}
