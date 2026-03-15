/**
 * CgMarkdownEditor JS Interop 模块
 * 封装 Vditor 初始化、值同步、图片直传与销毁。
 * Vditor CDN 资源按需懒加载，仅在首次使用编辑器时引入。
 * 图片上传直接通过 fetch 发送到图床 API，绕过 SignalR 通道。
 */

const VDITOR_CSS_URL = 'https://cdn.masastack.com/npm/vditor/3.8.12/dist/index.css';
const VDITOR_JS_URL = 'https://cdn.masastack.com/npm/vditor/3.8.12/dist/index.min.js';

let _vditor = null;
let _dotNetRef = null;
let _suppressChangeEvent = false;
let _ready = false;
let _pendingValue = undefined;
let _vditorLoadPromise = null;
let _imageApiBaseUrl = '';

/**
 * 动态加载 CSS 文件（幂等）
 * @param {string} href
 * @returns {Promise<void>}
 */
function loadCss(href) {
    if (document.querySelector('link[href="' + href + '"]')) {
        return Promise.resolve();
    }
    return new Promise(function (resolve, reject) {
        var link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = href;
        link.onload = resolve;
        link.onerror = function () { reject(new Error('Failed to load CSS: ' + href)); };
        document.head.appendChild(link);
    });
}

/**
 * 动态加载 JS 文件（幂等）
 * @param {string} src
 * @returns {Promise<void>}
 */
function loadScript(src) {
    if (document.querySelector('script[src="' + src + '"]')) {
        return Promise.resolve();
    }
    return new Promise(function (resolve, reject) {
        var script = document.createElement('script');
        script.src = src;
        script.onload = resolve;
        script.onerror = function () { reject(new Error('Failed to load script: ' + src)); };
        document.body.appendChild(script);
    });
}

/**
 * 确保 Vditor CDN 资源已加载（幂等，多次调用只触发一次加载）
 * @returns {Promise<void>}
 */
function ensureVditorLoaded() {
    if (window.Vditor) {
        return Promise.resolve();
    }
    if (!_vditorLoadPromise) {
        _vditorLoadPromise = loadCss(VDITOR_CSS_URL)
            .then(function () { return loadScript(VDITOR_JS_URL); })
            .then(function () {
                if (!window.Vditor) {
                    throw new Error('Vditor failed to initialize after script load');
                }
            })
            .catch(function (err) {
                _vditorLoadPromise = null; // 失败后允许重试
                throw err;
            });
    }
    return _vditorLoadPromise;
}

/**
 * 初始化 Vditor 编辑器
 * @param {object} dotNetRef - .NET 对象引用
 * @param {HTMLElement} containerElement - 容器 DOM 元素
 * @param {string} initialValue - 初始 Markdown 内容
 * @param {string} placeholder - 占位文本
 * @param {string} imageApiBaseUrl - 图床 API 基地址（如 https://api.cngal.top/）
 */
export function initEditor(dotNetRef, containerElement, initialValue, placeholder, imageApiBaseUrl) {
    disposeEditor();
    _dotNetRef = dotNetRef;
    _ready = false;
    _pendingValue = undefined;
    _imageApiBaseUrl = (imageApiBaseUrl || '').replace(/\/+$/, '');

    return ensureVditorLoaded().then(function () {
        _vditor = new Vditor(containerElement, {
            value: initialValue || '',
            placeholder: placeholder || '',
            height: 800,
            mode: 'sv',
            icon: 'material',
            lang: 'zh_CN',
            toolbarConfig: {
                pin: true
            },
            toolbar: [
                'headings', 'bold', 'italic', 'strike', '|',
                'quote', 'list', 'ordered-list', 'check', '|',
                'link', 'upload', 'table', 'line', '|',
                'code', 'inline-code', '|',
                'undo', 'redo', '|',
                'edit-mode', 'outline', 'fullscreen', '|',
                'help'
            ],
            cache: {
                enable: false
            },
            preview: {
                hljs: {
                    lineNumber: true,
                    style: 'github'
                },
                math: {
                    engine: 'KaTeX'
                }
            },
            upload: {
                accept: 'image/*',
                multiple: false,
                max: 10 * 1024 * 1024,
                handler: function (files) {
                    if (!files || files.length === 0) {
                        return null;
                    }
                    handleImageUpload(files[0]);
                    return null;
                }
            },
            input: function (value) {
                if (_suppressChangeEvent || !_dotNetRef) {
                    return;
                }
                _dotNetRef.invokeMethodAsync('OnEditorValueChanged', value);
            },
            after: function () {
                _ready = true;
                // 应用在初始化完成前排队的 setValue 调用
                if (_pendingValue !== undefined && _vditor) {
                    _suppressChangeEvent = true;
                    _vditor.setValue(_pendingValue);
                    _suppressChangeEvent = false;
                    _pendingValue = undefined;
                }
            }
        });
    });
}

/**
 * 获取编辑器当前值
 * @returns {string}
 */
export function getValue() {
    if (!_vditor || !_ready) {
        return '';
    }
    return _vditor.getValue();
}

/**
 * 从外部设置编辑器内容（参数变更时调用）
 * @param {string} value
 */
export function setValue(value) {
    if (!_vditor) {
        return;
    }
    // 编辑器尚未就绪，排队等待 after 回调
    if (!_ready) {
        _pendingValue = value || '';
        return;
    }
    var current = _vditor.getValue();
    if (current === (value || '')) {
        return;
    }
    _suppressChangeEvent = true;
    _vditor.setValue(value || '');
    _suppressChangeEvent = false;
}

/**
 * 销毁编辑器实例
 */
export function disposeEditor() {
    if (_vditor) {
        try {
            _vditor.destroy();
        } catch (e) {
            // DOM element may already be removed during circuit teardown
        }
        _vditor = null;
    }
    _dotNetRef = null;
    _ready = false;
    _pendingValue = undefined;
}

/**
 * 处理图片上传
 * 通过 fetch 直接将文件上传到图床 API，绕过 SignalR 通道
 */
function handleImageUpload(file) {
    if (!_vditor || !_imageApiBaseUrl) {
        if (_vditor) {
            _vditor.tip('图片上传未配置', 3000);
        }
        return;
    }

    var formData = new FormData();
    formData.append('files', file, file.name);

    var uploadUrl = _imageApiBaseUrl + '/api/files/Upload?x=0&y=0&type=0';

    fetch(uploadUrl, {
        method: 'POST',
        body: formData
    })
    .then(function (response) {
        if (!response.ok) {
            throw new Error('上传失败（HTTP ' + response.status + '）');
        }
        return response.json();
    })
    .then(function (results) {
        if (!Array.isArray(results) || results.length === 0) {
            throw new Error('上传失败，服务未返回结果');
        }
        var first = results[0];
        if (!first.uploaded || !first.url) {
            throw new Error(first.error || '上传失败');
        }
        if (_vditor) {
            _vditor.insertValue('\n![' + file.name + '](' + first.url + ')\n');
        }
    })
    .catch(function (err) {
        if (_vditor) {
            _vditor.tip(err.message || '上传异常', 3000);
        }
    });
}
