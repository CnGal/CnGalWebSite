import { loadAsset } from '/_content/CnGalWebSite.MainSite.Shared/scripts/third-party-assets.js';
import { own } from '/_content/CnGalWebSite.MainSite.Shared/scripts/component-lifecycle.js';
/**
 * CgMarkdownEditor JS Interop 模块
 * 封装 Vditor 初始化、值同步、图片直传与销毁。
 * Vditor CDN 资源按需懒加载，仅在首次使用编辑器时引入。
 * 图片上传直接通过 fetch 发送到图床 API，绕过 SignalR 通道。
 */

const VDITOR_CSS_URL = 'https://cdn.masastack.com/npm/vditor/3.8.12/dist/index.css';
const VDITOR_JS_URL = 'https://cdn.masastack.com/npm/vditor/3.8.12/dist/index.min.js';

let _vditorLoadPromise = null;
function ensureVditorLoaded() {
    if (!_vditorLoadPromise) _vditorLoadPromise = (window.Vditor ? Promise.resolve() : loadAsset(VDITOR_JS_URL))
        .then(() => { if (!window.Vditor) throw new Error('Vditor failed to initialize after script load'); })
        .catch(error => { _vditorLoadPromise = null; throw error; });
    return Promise.all([loadAsset(VDITOR_CSS_URL, 'style'), _vditorLoadPromise]);
}

export async function create(dotNetRef, containerElement, initialValue, placeholder, imageApiBaseUrl, disabled, initialRevision = 0) {
const root = containerElement.closest('cg-owned');
if (!root?.isConnected) throw new Error('Editor disconnected before initialization');
const lifetime = new AbortController();
let changeTimer, pendingChange;
let _disabled = disabled;
let revision = initialRevision;
let _vditor = null;
let _dotNetRef = null;
let _suppressChangeEvent = false;
let _ready = false;
let _pendingValue = undefined;
let _imageApiBaseUrl = '';
let _editorContainer = null;
let _isComposing = false;
let _suppressedFinalValue = undefined;

function notifyEditorValueChanged(value) {
    if (_suppressChangeEvent || !_dotNetRef) {
        return;
    }

    clearTimeout(changeTimer);
    changeTimer = setTimeout(() => {
        pendingChange = _dotNetRef?.invokeMethodAsync('OnEditorValueChanged', value, revision)
            .catch(error => console.error('[CgMarkdownEditor] change', error));
    }, 250);
}

function onCompositionStart() {
    _isComposing = true;
    _suppressedFinalValue = undefined;
}

function onCompositionEnd() {
    _isComposing = false;
    if (!_vditor || !_ready) {
        return;
    }

    const value = _vditor.getValue();
    _suppressedFinalValue = value;
    notifyEditorValueChanged(value);
}

function attachCompositionListeners(containerElement) {
    _editorContainer = containerElement;
    _editorContainer.addEventListener('compositionstart', onCompositionStart);
    _editorContainer.addEventListener('compositionend', onCompositionEnd);
}

function detachCompositionListeners() {
    if (_editorContainer) {
        _editorContainer.removeEventListener('compositionstart', onCompositionStart);
        _editorContainer.removeEventListener('compositionend', onCompositionEnd);
        _editorContainer = null;
    }

    _isComposing = false;
    _suppressedFinalValue = undefined;
}

/**
 * 动态加载 CSS 文件（幂等）
 * @param {string} href
 * @returns {Promise<void>}
 */
/**
 * 初始化 Vditor 编辑器
 * @param {object} dotNetRef - .NET 对象引用
 * @param {HTMLElement} containerElement - 容器 DOM 元素
 * @param {string} initialValue - 初始 Markdown 内容
 * @param {string} placeholder - 占位文本
 * @param {string} imageApiBaseUrl - 图床 API 基地址（如 https://api.cngal.top/）
 */
function initEditor(dotNetRef, containerElement, initialValue, placeholder, imageApiBaseUrl) {
    _dotNetRef = dotNetRef;
    _ready = false;
    _pendingValue = undefined;
    _imageApiBaseUrl = (imageApiBaseUrl || '').replace(/\/+$/, '');
    attachCompositionListeners(containerElement);

    return ensureVditorLoaded().then(function () {
        if (lifetime.signal.aborted || !containerElement.isConnected) throw new Error("Editor disconnected during initialization");
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
                if (_isComposing) {
                    return;
                }

                if (_suppressedFinalValue !== undefined) {
                    const isDuplicateFinalInput = _suppressedFinalValue === value;
                    _suppressedFinalValue = undefined;
                    if (isDuplicateFinalInput) {
                        return;
                    }
                }

                notifyEditorValueChanged(value);
            },
            after: function () {
                if (lifetime.signal.aborted) return;
                _ready = true;
                if (_vditor) _disabled ? _vditor.disabled() : _vditor.enable();
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
function getValue() {
    if (!_vditor || !_ready) {
        return _pendingValue ?? initialValue ?? '';
    }
    return _vditor.getValue();
}

/**
 * 从外部设置编辑器内容（参数变更时调用）
 * @param {string} value
 */
function setValue(value, nextRevision) {
    clearTimeout(changeTimer);
    revision = nextRevision ?? revision + 1;
    if (!_vditor) { _pendingValue = value ?? ''; return; }
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
function disposeEditor() {
    if (lifetime.signal.aborted) return;
    lifetime.abort(); clearTimeout(changeTimer);
    root.cgFlush = null;
    detachCompositionListeners();
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
        signal: lifetime.signal,
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

const controller = own(root, {
    getValue, setValue, dispose: disposeEditor,
    setDisabled(value) { _disabled = value; if (_ready && _vditor) value ? _vditor.disabled() : _vditor.enable(); },
    async flush() {
        clearTimeout(changeTimer);
        await pendingChange;
        const value = getValue();
        if (_dotNetRef) await _dotNetRef.invokeMethodAsync('OnEditorValueChanged', value, revision);
        return value;
    }
});
root.cgFlush = () => controller.flush();
try { await initEditor(dotNetRef, containerElement, initialValue, placeholder, imageApiBaseUrl); controller.setDisabled(disabled); return controller; }
catch (error) { controller.dispose(); throw error; }
}

export async function flushEditors(root) {
    for (const host of root.querySelectorAll('cg-owned')) await host.cgFlush?.();
}
