/**
 * CgMarkdownEditor JS Interop 模块
 * 封装 Vditor 初始化、值同步、图片上传桥接与销毁。
 */

let _vditor = null;
let _dotNetRef = null;
let _suppressChangeEvent = false;
let _ready = false;
let _pendingValue = undefined;

/**
 * 初始化 Vditor 编辑器
 * @param {object} dotNetRef - .NET 对象引用
 * @param {HTMLElement} containerElement - 容器 DOM 元素
 * @param {string} initialValue - 初始 Markdown 内容
 * @param {string} placeholder - 占位文本
 */
export function initEditor(dotNetRef, containerElement, initialValue, placeholder) {
    disposeEditor();
    _dotNetRef = dotNetRef;
    _ready = false;
    _pendingValue = undefined;

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
                if (!files || files.length === 0 || !_dotNetRef) {
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
        _vditor.destroy();
        _vditor = null;
    }
    _dotNetRef = null;
    _ready = false;
    _pendingValue = undefined;
}

/**
 * 处理图片上传
 * 将文件转为 base64 后桥接到 .NET 上传
 */
function handleImageUpload(file) {
    if (!_dotNetRef || !_vditor) {
        return;
    }

    var reader = new FileReader();
    reader.onload = function () {
        var base64Full = reader.result;
        var commaIndex = base64Full.indexOf(',');
        var base64Data = commaIndex >= 0 ? base64Full.substring(commaIndex + 1) : base64Full;

        _dotNetRef.invokeMethodAsync('OnImageUploadFromJs', base64Data, file.name)
            .then(function (url) {
                if (url && _vditor) {
                    // 在当前光标位置插入图片 markdown
                    _vditor.insertValue('\n![' + file.name + '](' + url + ')\n');
                }
            })
            .catch(function (err) {
                if (_vditor) {
                    _vditor.tip(err.message || '上传异常', 3000);
                }
            });
    };
    reader.onerror = function () {
        if (_vditor) {
            _vditor.tip('文件读取失败', 3000);
        }
    };
    reader.readAsDataURL(file);
}
