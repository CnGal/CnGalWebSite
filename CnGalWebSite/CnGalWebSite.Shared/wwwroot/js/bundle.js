
function slotMachineTrigger(luckyNum) {
    let slot1 = document.querySelector("div.slot1 > ul");
    let slot2 = document.querySelector("div.slot2 > ul");
    let slot3 = document.querySelector("div.slot3 > ul");
    let slot4 = document.querySelector("div.slot4 > ul");
    if (slot1.querySelectorAll('li').length > 1) {
        let lis1 = Array.prototype.slice.call(slot1.querySelectorAll('li')).slice(0, 30);
        let lis2 = Array.prototype.slice.call(slot2.querySelectorAll('li')).slice(0, 30);
        let lis3 = Array.prototype.slice.call(slot3.querySelectorAll('li')).slice(0, 30);
        let lis4 = Array.prototype.slice.call(slot4.querySelectorAll('li')).slice(0, 30);
        for (let li of lis1) {
            slot1.removeChild(li);
        }
        for (let li of lis2) {
            slot2.removeChild(li);
        }
        for (let li of lis3) {
            slot3.removeChild(li);
        }
        for (let li of lis4) {
            slot4.removeChild(li);
        }
        slot1.classList.remove("slotAnimation");
        slot2.classList.remove("slotAnimation");
        slot3.classList.remove("slotAnimation");
        slot4.classList.remove("slotAnimation");
        slot1.style.transform = "translateY(0px)";
        slot2.style.transform = "translateY(0px)";
        slot3.style.transform = "translateY(0px)";
        slot4.style.transform = "translateY(0px)";
        void document.getElementById('slot1').offsetHeight;
        void document.getElementById('slot2').offsetHeight;
        void document.getElementById('slot3').offsetHeight;
        void document.getElementById('slot4').offsetHeight;
    }
    for (let i = 0; i < 29; i++) {
        let n1 = Math.floor(Math.random() * 10);
        let n2 = Math.floor(Math.random() * 10);
        let n3 = Math.floor(Math.random() * 10);
        let n4 = Math.floor(Math.random() * 10);
        let li1 = document.createElement('li');
        let li2 = document.createElement('li');
        let li3 = document.createElement('li');
        let li4 = document.createElement('li');
        li1.textContent = n1;
        li2.textContent = n2;
        li3.textContent = n3;
        li4.textContent = n4;
        slot1.appendChild(li1);
        slot2.appendChild(li2);
        slot3.appendChild(li3);
        slot4.appendChild(li4);
    }
    luckyNum = luckyNum.toString();
    while (luckyNum.length < 4) {
        luckyNum = '0' + luckyNum;
    }
    let li5 = document.createElement('li');
    let li6 = document.createElement('li');
    let li7 = document.createElement('li');
    let li8 = document.createElement('li');
    li5.textContent = luckyNum[0];
    li6.textContent = luckyNum[1];
    li7.textContent = luckyNum[2];
    li8.textContent = luckyNum[3];
    slot1.appendChild(li5);
    slot2.appendChild(li6);
    slot3.appendChild(li7);
    slot4.appendChild(li8);
    slot1.classList.add("slotAnimation");
    slot2.classList.add("slotAnimation");
    slot3.classList.add("slotAnimation");
    slot4.classList.add("slotAnimation");
    let fin = function () {
        slot1.style.transform = "translateY(-4890px)";
        slot2.style.transform = "translateY(-4890px)";
        slot3.style.transform = "translateY(-4890px)";
        slot4.style.transform = "translateY(-4890px)";
    };
    setTimeout(fin, 1000);
};

// Js代码
var onscrolling = false;

var dotNetHelper;

function initInfiniteScroll(helper) {
    dotNetHelper = helper;

    onscrolling = false;
    window.onscroll = async function () {
        if (onscrolling == true || dotNetHelper == null) {
            return;
        }
        onscrolling = true;

        //文档内容实际高度（包括超出视窗的溢出部分）
        var scrollHeight = Math.max(document.documentElement.scrollHeight, document.body.scrollHeight);
        //滚动条滚动距离
        var scrollTop = window.pageYOffset || document.documentElement.scrollTop || document.body.scrollTop;
        //窗口可视范围高度
        var clientHeight = window.innerHeight || Math.min(document.documentElement.clientHeight, document.body.clientHeight);

        if (clientHeight + scrollTop + 400 >= scrollHeight) {
            if (dotNetHelper != null) {
                await dotNetHelper.invokeMethodAsync('LoadMore');
            }
        }

        onscrolling = false;
    }
}
function deinitInfiniteScroll() {
    dotNetHelper = null;
    window.onscroll = null;
}

/**
* 保存div为图片
*
* 该访问为外部方法，如需使用，请解开注解，并且添加js引用
* js引用地址：
* @param {string} id - 要保存为图片的div的id
* @param {string} fileName - 保存的文件名
* @param {boolean} copyToClipboard - 是否复制到剪贴板，true表示复制到剪贴板，false或不传表示下载
*/

function saveDivAsImage(id, fileName, copyToClipboard) {
    var element = document.getElementById(id);

    // 临时修改样式确保元素完全可见
    var originalPosition = element.style.position;
    var originalZIndex = element.style.zIndex;
    var originalTransform = element.style.transform;

    html2canvas(element, {
        scale: 2, // 缩放比例，提高清晰度
        useCORS: true, // 允许加载跨域图片
        allowTaint: true, // 允许图片污染画布
        backgroundColor: null, // 保留背景透明
        logging: false, // 关闭日志记录以提高性能
        imageTimeout: 0, // 不限制图片加载时间
        width: element.offsetWidth, // 明确设置宽度
        height: element.offsetHeight, // 明确设置高度
        x: 0, // 从元素左上角开始捕获
        y: 0, // 从元素左上角开始捕获
        scrollX: 0, // 禁用水平滚动
        scrollY: 0, // 禁用垂直滚动
        windowWidth: document.documentElement.offsetWidth, // 设置窗口宽度
        windowHeight: document.documentElement.offsetHeight // 设置窗口高度
    }).then(canvas => {
        // 使用最高质量导出PNG
        var MIME_TYPE = "image/png";
        var imgURL = canvas.toDataURL(MIME_TYPE, 1.0);

        if (copyToClipboard) {
            // 复制到剪贴板
            // 由于不能直接复制图片到剪贴板，我们需要先创建一个blob
            canvas.toBlob(function(blob) {
                try {
                    // 尝试使用新的Clipboard API (适用于较新的浏览器)
                    const clipboardItem = new ClipboardItem({ 'image/png': blob });
                    navigator.clipboard.write([clipboardItem]).then(() => {
                    }).catch(err => {
                        console.error('剪贴板复制失败:', err);
                        fallbackCopyToClipboard(imgURL, fileName);
                    });
                } catch(e) {
                    // 回退方案
                    fallbackCopyToClipboard(imgURL, fileName);
                }
            }, 'image/png', 1.0);
        } else {
            // 下载图片
            const anchorElement = document.createElement('a');
            anchorElement.href = imgURL;
            anchorElement.download = fileName || '';
            document.body.appendChild(anchorElement);
            anchorElement.click();
            document.body.removeChild(anchorElement);
        }

        // 恢复原始样式
        element.style.position = originalPosition;
        element.style.zIndex = originalZIndex;
        element.style.transform = originalTransform;
    }).catch(error => {
        console.error('截图出错：', error);

        // 发生错误时也要恢复原始样式
        element.style.position = originalPosition;
        element.style.zIndex = originalZIndex;
        element.style.transform = originalTransform;
    });
}

/**
 * 回退方案：将图片URL复制到剪贴板或下载
 * @param {string} imgURL - 图片的DataURL
 * @param {string} fileName - 文件名
 */
function fallbackCopyToClipboard(imgURL, fileName) {
    // 创建一个临时的图片元素
    const img = document.createElement('img');
    img.src = imgURL;
    img.style.position = 'fixed';
    img.style.left = '-9999px';
    document.body.appendChild(img);

    // 尝试复制图片元素
    try {
        // 创建一个range和selection
        const range = document.createRange();
        range.selectNode(img);
        const selection = window.getSelection();
        selection.removeAllRanges();
        selection.addRange(range);

        // 尝试复制
        const success = document.execCommand('copy');
        if (success) {
            alert('图片已复制到剪贴板');
        } else {
            // 如果复制失败，尝试下载
            const anchorElement = document.createElement('a');
            anchorElement.href = imgURL;
            anchorElement.download = fileName || '';
            document.body.appendChild(anchorElement);
            anchorElement.click();
            document.body.removeChild(anchorElement);
            alert('无法复制到剪贴板，已改为下载图片');
        }
    } catch(e) {
        console.error('回退复制失败:', e);
        // 如果出错，尝试下载
        const anchorElement = document.createElement('a');
        anchorElement.href = imgURL;
        anchorElement.download = fileName || '';
        document.body.appendChild(anchorElement);
        anchorElement.click();
        document.body.removeChild(anchorElement);
        alert('无法复制到剪贴板，已改为下载图片');
    } finally {
        // 清理
        document.body.removeChild(img);
        window.getSelection().removeAllRanges();
    }
}


function getElementScrollLeft(id) {
    return document.getElementById(id).scrollLeft;
}
function getElementScrollWidth(id) {
    return document.getElementById(id).scrollWidth;
}
function getBodyClientWidth() {
    return document.body.clientWidth;
}
function setElementScrollLeft(id,left) {
    document.getElementById(id).scrollLeft = left;
}

//获取元素相对于页面的偏移
function getOffsetSum(ele) {
    var top = 0, left = 0;
    while (ele) {
        top += ele.offsetTop;
        left += ele.offsetLeft;
        ele = ele.offsetParent;
    }
    return {
        top: top,
        left: left
    }
}
function getOffsetRect(ele) {
    var box = ele.getBoundingClientRect();
    var body = document.body,
        docElem = document.documentElement;
    //获取页面的scrollTop,scrollLeft(兼容性写法)
    var scrollTop = window.pageYOffset || docElem.scrollTop || body.scrollTop,
        scrollLeft = window.pageXOffset || docElem.scrollLeft || body.scrollLeft;
    var clientTop = docElem.clientTop || body.clientTop,
        clientLeft = docElem.clientLeft || body.clientLeft;
    var top = box.top + scrollTop - clientTop,
        left = box.left + scrollLeft - clientLeft;
    return {
        //Math.round 兼容火狐浏览器bug
        top: Math.round(top),
        left: Math.round(left)
    }
}
function getOffset(ele) {
    if (ele.getBoundingClientRect) {
        return getOffsetRect(ele);
    } else {
        return getOffsetSum(ele);
    }
}

function getElementLeftOfLayout( objectId) {
    var element = document.getElementById(objectId);
    return getOffset(element).left;
}


/**
* 拖动内容，滚动条滚动，横向
* @param {string} container 需要拖动的面板
*/
function dragMoveX(id) {
    let scrollContainer = document.querySelector(id);
    scrollContainer.onmousedown = e => {
        //鼠标按下那一刻，滚动条的位置
        let mouseDownScrollPosition = {
            scrollLeft: scrollContainer.scrollLeft,
            scrollTop: scrollContainer.scrollTop
        };
        //鼠标按下的位置坐标
        let mouseDownPoint = {
            x: e.clientX,
            y: e.clientY
        };
        scrollContainer.onmousemove = e => {
            //鼠标滑动的实时距离
            let dragMoveDiff = {
                x: mouseDownPoint.x - e.clientX,
                y: mouseDownPoint.y - e.clientY
            };
            scrollContainer.scrollLeft = mouseDownScrollPosition.scrollLeft + dragMoveDiff.x;
            scrollContainer.scrollTop = mouseDownScrollPosition.scrollTop + dragMoveDiff.y;
        };
        document.onmouseup = e => {
            scrollContainer.onmousemove = null;
            document.onmouseup = null;
        };

    };
}

/*设置聚焦*/
function focusOnElement(id) {
    document.getElementById(id).focus();
}

"v0.4.8 Geetest Inc.";

(function (window) {
    "use strict";
    if (typeof window === 'undefined') {
        throw new Error('Geetest requires browser environment');
    }

var document = window.document;
var Math = window.Math;
var head = document.getElementsByTagName("head")[0];

function _Object(obj) {
    this._obj = obj;
}

_Object.prototype = {
    _each: function (process) {
        var _obj = this._obj;
        for (var k in _obj) {
            if (_obj.hasOwnProperty(k)) {
                process(k, _obj[k]);
            }
        }
        return this;
    }
};

function Config(config) {
    var self = this;
    new _Object(config)._each(function (key, value) {
        self[key] = value;
    });
}

Config.prototype = {
    api_server: 'api.geetest.com',
    protocol: 'http://',
    typePath: '/gettype.php',
    fallback_config: {
        slide: {
            static_servers: ["static.geetest.com", "dn-staticdown.qbox.me"],
            type: 'slide',
            slide: '/static/js/geetest.0.0.0.js'
        },
        fullpage: {
            static_servers: ["static.geetest.com", "dn-staticdown.qbox.me"],
            type: 'fullpage',
            fullpage: '/static/js/fullpage.0.0.0.js'
        }
    },
    _get_fallback_config: function () {
        var self = this;
        if (isString(self.type)) {
            return self.fallback_config[self.type];
        } else if (self.new_captcha) {
            return self.fallback_config.fullpage;
        } else {
            return self.fallback_config.slide;
        }
    },
    _extend: function (obj) {
        var self = this;
        new _Object(obj)._each(function (key, value) {
            self[key] = value;
        })
    }
};
var isNumber = function (value) {
    return (typeof value === 'number');
};
var isString = function (value) {
    return (typeof value === 'string');
};
var isBoolean = function (value) {
    return (typeof value === 'boolean');
};
var isObject = function (value) {
    return (typeof value === 'object' && value !== null);
};
var isFunction = function (value) {
    return (typeof value === 'function');
};
var MOBILE = /Mobi/i.test(navigator.userAgent);
var pt = MOBILE ? 3 : 0;

var callbacks = {};
var status = {};

var nowDate = function () {
    var date = new Date();
    var year = date.getFullYear();
    var month = date.getMonth() + 1;
    var day = date.getDate();
    var hours = date.getHours();
    var minutes = date.getMinutes();
    var seconds = date.getSeconds();

    if (month >= 1 && month <= 9) {
      month = '0' + month;
    }
    if (day >= 0 && day <= 9) {
      day = '0' + day;
    }
    if (hours >= 0 && hours <= 9) {
      hours = '0' + hours;
    }
    if (minutes >= 0 && minutes <= 9) {
      minutes = '0' + minutes;
    }
    if (seconds >= 0 && seconds <= 9) {
      seconds = '0' + seconds;
    }
    var currentdate = year + '-' + month + '-' + day + " " + hours + ":" + minutes + ":" + seconds;
    return currentdate;
}

var random = function () {
    return parseInt(Math.random() * 10000) + (new Date()).valueOf();
};

var loadScript = function (url, cb) {
    var script = document.createElement("script");
    script.charset = "UTF-8";
    script.async = true;

    // 对geetest的静态资源添加 crossOrigin
    if ( /static\.geetest\.com/g.test(url)) {
        script.crossOrigin = "anonymous";
    }

    script.onerror = function () {
        cb(true);
    };
    var loaded = false;
    script.onload = script.onreadystatechange = function () {
        if (!loaded &&
            (!script.readyState ||
            "loaded" === script.readyState ||
            "complete" === script.readyState)) {

            loaded = true;
            setTimeout(function () {
                cb(false);
            }, 0);
        }
    };
    script.src = url;
    head.appendChild(script);
};

var normalizeDomain = function (domain) {
    // special domain: uems.sysu.edu.cn/jwxt/geetest/
    // return domain.replace(/^https?:\/\/|\/.*$/g, ''); uems.sysu.edu.cn
    return domain.replace(/^https?:\/\/|\/$/g, ''); // uems.sysu.edu.cn/jwxt/geetest
};
var normalizePath = function (path) {
    path = path.replace(/\/+/g, '/');
    if (path.indexOf('/') !== 0) {
        path = '/' + path;
    }
    return path;
};
var normalizeQuery = function (query) {
    if (!query) {
        return '';
    }
    var q = '?';
    new _Object(query)._each(function (key, value) {
        if (isString(value) || isNumber(value) || isBoolean(value)) {
            q = q + encodeURIComponent(key) + '=' + encodeURIComponent(value) + '&';
        }
    });
    if (q === '?') {
        q = '';
    }
    return q.replace(/&$/, '');
};
var makeURL = function (protocol, domain, path, query) {
    domain = normalizeDomain(domain);

    var url = normalizePath(path) + normalizeQuery(query);
    if (domain) {
        url = protocol + domain + url;
    }

    return url;
};

var load = function (config, send, protocol, domains, path, query, cb) {
    var tryRequest = function (at) {

        var url = makeURL(protocol, domains[at], path, query);
        loadScript(url, function (err) {
            if (err) {
                if (at >= domains.length - 1) {
                    cb(true);
                    // report gettype error
                    if (send) {
                        config.error_code = 508;
                        var url = protocol + domains[at] + path;
                        reportError(config, url);
                    }
                } else {
                    tryRequest(at + 1);
                }
            } else {
                cb(false);
            }
        });
    };
    tryRequest(0);
};


var jsonp = function (domains, path, config, callback) {
    if (isObject(config.getLib)) {
        config._extend(config.getLib);
        callback(config);
        return;
    }
    if (config.offline) {
        callback(config._get_fallback_config());
        return;
    }

    var cb = "geetest_" + random();
    window[cb] = function (data) {
        if (data.status == 'success') {
            callback(data.data);
        } else if (!data.status) {
            callback(data);
        } else {
            callback(config._get_fallback_config());
        }
        window[cb] = undefined;
        try {
            delete window[cb];
        } catch (e) {
        }
    };
    load(config, true, config.protocol, domains, path, {
        gt: config.gt,
        callback: cb
    }, function (err) {
        if (err) {
            callback(config._get_fallback_config());
        }
    });
};

var reportError = function (config, url) {
    load(config, false, config.protocol, ['monitor.geetest.com'], '/monitor/send', {
        time: nowDate(),
        captcha_id: config.gt,
        challenge: config.challenge,
        pt: pt,
        exception_url: url,
        error_code: config.error_code
    }, function (err) {})
}

var throwError = function (errorType, config) {
    var errors = {
        networkError: '网络错误',
        gtTypeError: 'gt字段不是字符串类型'
    };
    if (typeof config.onError === 'function') {
        config.onError(errors[errorType]);
    } else {
        throw new Error(errors[errorType]);
    }
};

var detect = function () {
    return window.Geetest || document.getElementById("gt_lib");
};

if (detect()) {
    status.slide = "loaded";
}

window.initGeetest = function (userConfig, callback) {

    var config = new Config(userConfig);

    if (userConfig.https) {
        config.protocol = 'https://';
    } else if (!userConfig.protocol) {
        config.protocol = window.location.protocol + '//';
    }

    // for KFC
    if (userConfig.gt === '050cffef4ae57b5d5e529fea9540b0d1' ||
        userConfig.gt === '3bd38408ae4af923ed36e13819b14d42') {
        config.apiserver = 'yumchina.geetest.com/'; // for old js
        config.api_server = 'yumchina.geetest.com';
    }

    if(userConfig.gt){
        window.GeeGT = userConfig.gt
    }

    if(userConfig.challenge){
        window.GeeChallenge = userConfig.challenge
    }

    if (isObject(userConfig.getType)) {
        config._extend(userConfig.getType);
    }
    jsonp([config.api_server || config.apiserver], config.typePath, config, function (newConfig) {
        var type = newConfig.type;
        var init = function () {
            config._extend(newConfig);
            callback(new window.Geetest(config));
        };

        callbacks[type] = callbacks[type] || [];
        var s = status[type] || 'init';
        if (s === 'init') {
            status[type] = 'loading';

            callbacks[type].push(init);

            load(config, true, config.protocol, newConfig.static_servers || newConfig.domains, newConfig[type] || newConfig.path, null, function (err) {
                if (err) {
                    status[type] = 'fail';
                    throwError('networkError', config);
                } else {
                    status[type] = 'loaded';
                    var cbs = callbacks[type];
                    for (var i = 0, len = cbs.length; i < len; i = i + 1) {
                        var cb = cbs[i];
                        if (isFunction(cb)) {
                            cb();
                        }
                    }
                    callbacks[type] = [];
                }
            });
        } else if (s === "loaded") {
            init();
        } else if (s === "fail") {
            throwError('networkError', config);
        } else if (s === "loading") {
            callbacks[type].push(init);
        }
    });

};


})(window);



function openNewPage(url) {
    window.open(url, "_blank");
}
function navigateTo(url) {
    window.open(url, "_self");
}


function getResponseFromRecaptcha(id) {
    var responseToken = grecaptcha.getResponse(id);
    return responseToken;
}

function initRecaptcha() {
    // 得到组件id
    var widgetId = grecaptcha.render('robot', {
        'sitekey': '6LedTqcbAAAAADER0LFm7wmLcdyc7BtTuD8kFa74',
        'theme': 'light',
        'size': 'normal'
    });
    return widgetId;
}

function initDebugTool() {
    // init vConsole
    eruda.init();
    // var vConsole = new VConsole();
    console.log('成功启动调试工具');
}
function initMouseJs() {
    var script2 = document.createElement('script');
    script2.setAttribute('type', 'text/javascript');
    script2.setAttribute('src', 'js/mouse.js');
    document.body.appendChild(script2);
}
function highlightAllCode() {
    hljs.highlightAll();
}

function trackEvent(event_name, type, objectName, objectId, userId, note) {
    umami.track(event_name, {
        type: type,
        objectName: objectName,
        objectId: objectId,
        userId: userId,
        note: note
    });
}

function initVditorContext(objRef, domRef) {
    domRef.Vditor.vditor.options.upload.success = (editor, res) => {
        objRef.invokeMethodAsync('HandleUploadSuccessAsync', res);
    };
    domRef.Vditor.vditor.options.upload.linkToImgFormat = (res) => {
        objRef.invokeMethodAsync('HandleLinkToImgFormatAsync', res);
    };
}

const sleep = (delay) => new Promise((resolve) => setTimeout(resolve, delay))

function initUploadButton(objRef, up_to_chevereto, up_img_label) {
    jQuery(up_to_chevereto).change(function () {
        for (var i = 0; i < this.files.length; i++) {
            var f = this.files[i];
            var formData = new FormData();
            formData.append('source', f);
            jQuery.ajax({
                async: true,
                crossDomain: true,
                url: 'https://image.cngal.org/api/1/upload/?format=json&key=00ee89a663169218bd1ddeab71d54ddf',
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,
                beforeSend: function (xhr) {
                    jQuery(up_img_label).html('<i class="fa fa-fw fa-spinner fa-spin" aria-hidden="true"></i> 上传中...');
                },
                success: function (res) {
                    objRef.invokeMethodAsync('UpLoaded', res.image.url + '||' + f.size);
                    //   document.getElementsByClassName("tui-editor-contents")[0].innerHTML='<a href="' + res.image.url + '"><img src="' + res.image.url + '" alt="' + res.image.title + '"></img></a>';
                    jQuery(up_img_label).html('<i class="fa fa-fw fa-check" aria-hidden="true"></i> 上传成功,继续上传');
                },
                error: function () {
                    jQuery(up_img_label).html('<i class="fa fa-fw fa-times" aria-hidden="true"></i> 上传失败，重新上传');
                }
            });
        }
    });
}

function initTencentCAPTCHA(objRef) {
    var captcha1 = new TencentCaptcha('2049507573', function (res) {
        //成功回调
        if (res.ret === 0) {
            // 上传票据 可根据errorCode和errorMessage做特殊处理或统计
            objRef.invokeMethodAsync('OnChecked', res.ticket, res.randstr);
        }
        else {
            objRef.invokeMethodAsync('OnCancel');
        }
    });
    captcha1.show(); // 显示验证码
}
function setCustomVar(name, value) {
    _czc.push(['_setCustomVar', name, value, 1]);
}


function addfavorite() {
    var ctrl = (navigator.userAgent.toLowerCase()).indexOf('mac') != -1 ? 'Command/Cmd' : 'CTRL';
    try {
        if (document.all) { //IE类浏览器
            try {
                window.external.toString(); //360浏览器不支持window.external，无法收藏
                window.alert("国内开发的360浏览器等不支持主动加入收藏。\n您可以尝试通过浏览器菜单栏 或快捷键 ctrl+D 试试。");
            }
            catch (e) {
                try {
                    window.external.addFavorite(window.location, document.title);
                }
                catch (e) {
                    window.external.addToFavoritesBar(window.location, document.title);  //IE8
                }
            }
        }
        else if (window.sidebar) { //firfox等浏览器
            window.sidebar.addPanel(document.title, window.location, "");
        }
        else {
            alert('您可以尝试通过快捷键' + ctrl + ' + D 加入到收藏夹~');
        }
    }
    catch (e) {
        window.alert("因为IE浏览器存在bug，添加收藏失败！\n解决办法：在注册表中查找\n HKEY_CLASSES_ROOT\\TypeLib\\{EAB22AC0-30C1-11CF-A7EB-0000C05BAE0B}\\1.1\\0\\win32 \n将 C:\\WINDOWS\\system32\\shdocvw.dll 改为 C:\\WINDOWS\\system32\\ieframe.dll ");
    }
}

function initGeetestCAPTCHA(objRef, gt, challenge, success) {
    initGeetest({
        // 以下配置参数来自服务端 SDK
        gt: gt,
        challenge: challenge,
        offline: !success,
        new_captcha: true,
        width: '100%'
    }, function (captchaObj) {
        // 这里可以调用验证实例 captchaObj 的实例方法
        captchaObj.appendTo('#captcha-box');
        //成功回调
        captchaObj.onSuccess(function () {
            var result = captchaObj.getValidate();
            objRef.invokeMethodAsync('OnSuccessed', result.geetest_challenge, result.geetest_validate, result.geetest_seccode);
        });
    });
}

function goback() {
    window.location.href = "javascript:history.go(-1)";
}


function initGeetestBindCAPTCHA(objRef, gt, challenge, success) {
    initGeetest({
        // 以下配置参数来自服务端 SDK
        gt: gt,
        challenge: challenge,
        offline: !success,
        new_captcha: true,
        product: 'bind'
    }, function (captchaObj) {
        //成功回调
        captchaObj.onReady(function () {
            captchaObj.verify();
        });

        captchaObj.onSuccess(function () {
            var result = captchaObj.getValidate();
            objRef.invokeMethodAsync('OnChecked', result.geetest_challenge, result.geetest_validate, result.geetest_seccode);
        });
        captchaObj.onError(function () {
            objRef.invokeMethodAsync('OnCancel');
        });
        captchaObj.onClose(function () {
            objRef.invokeMethodAsync('OnCancel');
        });
    });
}

function showGeetestBindCAPTCHA(objRef) {
    if (capObj == null) {
        objRef.invokeMethodAsync('OnCancel');
    }
    else {

    }
}
function isMobile() {
    var ua = navigator.userAgent; /* navigator.userAgent 浏览器发送的用户代理标题 */
    if (ua.indexOf('Android') > -1 || ua.indexOf('iPhone') > -1 || ua.indexOf('iPod') > -1 || ua.indexOf('Symbian') > -1) {
        return true;
    } else {
        return false;
    }
};

/*加载友盟js*/
var structuredData = document.createElement('script');
structuredData.type = 'application/ld+json';
var root_s = document.head;
root_s.parentNode.insertBefore(structuredData, root_s);

function setStructuredData(data) {
    structuredData.innerText = data;
}

function playAudio(url) {
    var audio = document.getElementById("bgMusic");
    if (url != null && (audio.currentTime == 0 || audio.currentTime == audio.duration || audio.src != url)) {
        audio.src = url;
        audio.play();
    }
    else {
        audio.pause();
        audio.currentTime = 0;
    }
}

/*下载文件*/
window.downloadFileFromStream = async (fileName, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    if (fileName == null) {
        anchorElement.download = '';
    }
    else {
        anchorElement.download = fileName;
    }
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}

function domToImageUrl(id, name) {
    domtoimage.toBlob(document.getElementById(id))
        .then(function (blob) {
            window.saveAs(blob, name);
        });
}
// 生成图片
function domToImageUrl(id, name) {
    const setup = {
        useCORS: true, // 使用跨域
    };
    html2canvas(document.getElementById(id), {
        useCORS: true,
    }).then((canvas) => {
        const link = canvas.toDataURL("image/jpg");
        exportPicture(link, name);
    });
}

// 导出图片
function exportPicture(link, name) {
    const file = document.createElement("a");
    file.style.display = "none";
    file.href = link;
    file.download = decodeURI(name);
    document.body.appendChild(file);
    file.click();
    document.body.removeChild(file);
}

/* var dotNetHelper;

async function onWebsiteTemplateInit(helper)
{
    dotNetHelper = helper;
    console.log(`init`);
    var msg = await dotNetHelper.invokeMethodAsync('CheckBooking');
    console.log(msg);
    msg = await dotNetHelper.invokeMethodAsync('GetEntryModel');
    console.log(msg);
}
*/
/*获取系统主题*/
function checkSystemThemeIsDark() {
    const themeMedia = window.matchMedia("(prefers-color-scheme: light)");
    if (themeMedia.matches) {
        return false;
    } else {
        return true;
    }
}
/*获取UA*/
function getUserAgent() {
    return navigator.userAgent;
}


var int = self.setInterval("error_check()", 500);
var counter = 0;
function error_check() {
    // 获取错误弹窗是否显示
    var str = window.getComputedStyle(document.getElementById("blazor-error-ui")).display

    // 没有错误则退出
    if (str == "none") {

        // 计数器
        counter++;

        // 如果正常浏览页面超过30s 清空错误计数
        if (counter > 60) {
            localStorage.setItem("error-counter", 0);
        }
        return;
    }

    // 记录错误数量
    var error = localStorage.getItem("error-counter");
    error++;
    localStorage.setItem("error-counter", error);

    // 大于3次则不刷新页面
    if (error > 3) {
        return;
    }


    // 重新加载页面
    onreload();
}

/*滚动到底部 */
function scrollToBottom(css) {
    var container = document.querySelector(css);
    container.scrollTop = container.scrollHeight;
}

/*以下为加载时自动执行的代码*/
//if (typeof Blazor != "undefined") {
//    Blazor.start({
//        reconnectionOptions: {
//            maxRetries: 3,
//            retryIntervalMilliseconds: 2000
//        }
//    });
//}

function onreload() {
    location.reload();
}
/*检测浏览器版本*/
function browserVersion() {
    var userAgent = navigator.userAgent; //取得浏览器的userAgent字符串
    var isIE = userAgent.indexOf("compatible") > -1 && userAgent.indexOf("MSIE") > -1; //判断是否IE<11浏览器
    var isIE11 = userAgent.indexOf('Trident') > -1 && userAgent.indexOf("rv:11.0") > -1;
    var isEdge = userAgent.indexOf("Edge") > -1 && !isIE; //Edge浏览器
    var isFirefox = userAgent.indexOf("Firefox") > -1; //Firefox浏览器
    var isOpera = userAgent.indexOf("Opera") > -1 || userAgent.indexOf("OPR") > -1; //Opera浏览器
    var isChrome = userAgent.indexOf("Chrome") > -1 && userAgent.indexOf("Safari") > -1 && userAgent.indexOf("Edge") == -1 && userAgent.indexOf("OPR") == -1; //Chrome浏览器
    var isSafari = userAgent.indexOf("Safari") > -1 && userAgent.indexOf("Chrome") == -1 && userAgent.indexOf("Edge") == -1 && userAgent.indexOf("OPR") == -1; //Safari浏览器
    if (isIE) {
        return 1;
    } else if (isIE11) {
        return 1;
    } else if (isEdge) {
        if (userAgent.split('Edge/')[1].split('.')[0] < 91) {
            return 1;
        }
        else {
            return 0;
        }
    } else if (isFirefox) {
        if (userAgent.split('Firefox/')[1].split('.')[0] < 72) {
            return 1;
        }
        else {
            return 0;
        }
    } else if (isOpera) {
        if (userAgent.split('OPR/')[1].split('.')[0] < 70) {
            return 1;
        }
        else {
            return 0;
        }
    } else if (isChrome) {
        if (userAgent.split('Chrome/')[1].split('.')[0] < 91) {
            return 1;
        }
        else {
            return 0;
        }
    } else if (isSafari) {
        if (userAgent.split('Version/')[1].split(' ')[0] < 16.4) {
            return 1;
        }
        else {
            return 0;
        }
    } else {
        return -1;//不是ie浏览器
    }
}

if (browserVersion() == 1) {
    alert("浏览器版本过低，网页可能无法正确展示\n你可以尝试安装 Edge，Chorme，Firefox 的最新版本后重试\n有任何疑问可以加群 761794704 进行反馈");
}


(function (c, l, a, r, i, t, y) {
    if (window.location.href.includes('cngal')) {
        c[a] = c[a] || function () { (c[a].q = c[a].q || []).push(arguments) };
        t = l.createElement(r); t.async = 1; t.src = "https://www.clarity.ms/tag/" + i;
        y = l.getElementsByTagName(r)[0]; y.parentNode.insertBefore(t, y);
    }
})(window, document, "clarity", "script", "buu31hy6os");

//(function ($) {
//    $.extend({
//        loading: function () {
//            var $loader = $("#loading");
//            if ($loader.length > 0) {
//                $loader.addClass("is-done");
//                var handler = window.setTimeout(function () {
//                    window.clearTimeout(handler);
//                    $loader.remove();
//                    $('body').removeClass('overflow-hidden');
//                }, 300);
//            }
//        },
//    });
//})(jQuery);

//var resourcesPath = 'http://localhost:8080/Samples/Resources/';  // 指定资源文件（模型）保存的路径
var backImageName = ''; // 指定背景图片
//var modelDir = '波奇酱2.0,Hiyori,Haru,Mao,Mark,Natori,Rice';  // 指定需要加载的模型
var canvasId = 'live2d';//画布Id

var live2d_dotNetHelper;

// 动态加载脚本
function loadScript(url, callback) {
    let script = document.createElement('script');
    if (script.readyState) { // IE
        script.onreadystatechange = function () {
            if (script.readyState === 'loaded' || script.readyState === 'complete') {
                script.onreadystatechange = null;
                callback();
            }
        }
    } else { // 其他浏览器
        script.onload = function () {
            callback();
        }
    }
    script.src = url;
    document.body.appendChild(script);
}

function initKanbanLive2D(dotNetHelper, modelDir, modelIndex, resourcesPath) {

    // 将注释节点插入到 HTML 文档中的 body 元素中
    var comment = document.createComment("看板娘live2d依赖库");
    document.body.appendChild(comment);
    // 加载js
    loadScript('https://res.cngal.org/live2d/js/live2dcubismcore.min.js', function () {
        loadScript('https://res.cngal.org/live2d/js/bundle.js', function () {
            //loadScript('http://localhost:5000/Samples/TypeScript/Demo/dist/bundle.js', function () {
            // 再初始化l2d
            var modelDirs = modelDir.split(',');
            live2d_dotNetHelper = dotNetHelper;

            setLive2dDefine(resourcesPath, backImageName, modelDirs, canvasId, modelIndex);
            initLive2d();
            initKanbanMoveAction(dotNetHelper);
            initButtonGroupMoveAction(dotNetHelper);
            initDialogBoxMoveAction(dotNetHelper);
            initChatCardMoveAction(dotNetHelper);
            listenLive2dCanvasChange();

            console.log("初始化Live2D")
        })
    })
}

// 初始化完成回调
window.Live2dInitCallback = function () {
    live2d_dotNetHelper.invokeMethodAsync("Live2dInitCallback")
}

function getWindowSize() {
    return {
        Height: window.innerHeight,
        Width: window.innerWidth,
    }
}

// 监听窗口变化
const getWindowInfo = () => {
    const windowInfo = {
        width: window.innerWidth,
        hight: window.innerHeight
    }
    console.log(windowInfo);
    live2d_dotNetHelper.invokeMethodAsync("CheckKanbanPositionAsync")
};

const debounce = (fn, delay) => {
    let timer;
    return function () {
        if (timer) {
            clearTimeout(timer);
        }
        timer = setTimeout(() => {
            fn();
        }, delay);
    }
};
const cancalDebounce = debounce(getWindowInfo, 100);

window.addEventListener('resize', cancalDebounce);



//获取此刻时间
function getTimeNow() {
    var now = new Date();
    return now.getTime();
}

var kanban_mousedown = false;
var buttongroup_mousedown = false;
var dialogbox_mousedown = false;
var chatcard_mousedown = false;

function initKanbanMoveAction(dotNetHelper) {
    const live2dItem = document.getElementById('kanban-live2d');
    let move = false;
    let deltaLeft = 0, deltaTop = 0;
    let x_org, y_org;
    let dx = 0;
    let dy = 0;
    var time;

    // 拖动开始事件，要绑定在被移动元素上
    const mousedown_fun = function (event) {
        /*
        * @des deltaLeft 即移动过程中不变的值
        */
        //获取鼠标按下时的时间
        var touch;
        if (event.touches) {
            touch = event.touches[0];//多个鼠标|手指事件取第一个
        } else {
            touch = event;
        }
        timeStart = getTimeNow();

        kanban_mousedown = true;

        //setInterval会每100毫秒执行一次，也就是每100毫秒获取一次时间
        time = setInterval(function () {
            timeEnd = getTimeNow();

            //如果此时检测到的时间与第一次获取的时间差有1000毫秒
            if (timeEnd - timeStart > 500) {
                //便不再继续重复此函数 （clearInterval取消周期性执行）
                clearInterval(time);
                //检查按钮组是否被按下
                if (buttongroup_mousedown) {
                    return;
                }
                //检查对话框是否被按下
                if (dialogbox_mousedown) {
                    return;
                }
                //执行逻辑
                deltaLeft = touch.clientX - touch.target.offsetLeft;
                deltaTop = touch.clientY - touch.target.offsetTop;
                //console.log("初始", deltaLeft, deltaTop);
                var rect = live2dItem.getBoundingClientRect();
                x_org = rect.x;
                y_org = rect.y;
                move = true;
                document.body.classList.add('user-select-none');

                //拖动开始 设置表情
                switchLiv2DExpression('expression1');

            }
        }, 10);
    }


    live2dItem.addEventListener('mousedown', mousedown_fun)
    live2dItem.addEventListener('touchstart', mousedown_fun, { passive: false })

    // 移动触发事件要放在，区域控制元素上
    const mousemove_fun = function (event) {
        kanban_mousedown = false;
        var touch;
        if (event.touches) {
            touch = event.touches[0];//多个鼠标|手指事件取第一个
        } else {
            touch = event;
        }

        if (move) {

            //阻止页面的滑动默认事件
            event.preventDefault();

            const cx = touch.clientX;
            const cy = touch.clientY;
            /** 相减即可得到相对于父元素移动的位置 */
            dx = cx - deltaLeft
            dy = cy - deltaTop
            var rect = live2dItem.getBoundingClientRect();
            //console.log("坐标", cx, cy)
            //console.log("移动", dx, dy)
            //console.log("位移", rect.x + dx, rect.y + dy);
            live2dItem.setAttribute('style', `left:${x_org + dx}px; top:${y_org + dy}px; width: ${rect.width}px; height: ${rect.height}px;`)
        }
        else {
            clearInterval(time);
        }
    }

    window.addEventListener('mousemove', mousemove_fun)
    window.addEventListener('touchmove', mousemove_fun, { passive: false })

    const mouseup_fun = function (event) {
        if (move) {
            move = false;
            //console.log('mouseup');
            dotNetHelper.invokeMethodAsync('SetKanbanPosition', x_org + dx, y_org + dy);
            document.body.classList.remove('user-select-none');

            //拖动结束 清空表情
            switchLiv2DExpression();
        }
        else {
            clearInterval(time);
        }
    }

    // 拖动结束触发要放在，区域控制元素上
    window.addEventListener('mouseup', mouseup_fun)
    window.addEventListener('touchend', mouseup_fun, { passive: false })
}


function initButtonGroupMoveAction(dotNetHelper) {
    const itemId = 'kanban-button-group';
    const live2dItem = document.getElementById('kanban-live2d');
    const groupItem = document.getElementById(itemId);

    let move = false;
    let deltaLeft = 0, deltaTop = 0;
    let x_org, y_org;
    let dx = 0;
    let dy = 0;
    var time;

    const mousedown_fun = function (event) {
        /*
        * @des deltaLeft 即移动过程中不变的值
        */
        //获取鼠标按下时的时间
        var touch;
        if (event.touches) {
            touch = event.touches[0];//多个鼠标|手指事件取第一个
        } else {
            touch = event;
        }
        //获取鼠标按下时的时间
        timeStart = getTimeNow();

        buttongroup_mousedown = true;

        //setInterval会每100毫秒执行一次，也就是每100毫秒获取一次时间
        time = setInterval(function () {
            timeEnd = getTimeNow();

            //如果此时检测到的时间与第一次获取的时间差有1000毫秒
            if (timeEnd - timeStart > 500) {
                //便不再继续重复此函数 （clearInterval取消周期性执行）
                clearInterval(time);

                //检查对话框是否被按下
                if (dialogbox_mousedown) {
                    return;
                }

                //执行逻辑
                deltaLeft = touch.clientX - touch.target.offsetLeft;
                deltaTop = touch.clientY - touch.target.offsetTop;
                //console.log("初始", deltaLeft, deltaTop);
                var rect_w = live2dItem.getBoundingClientRect();
                var rect_n = groupItem.getBoundingClientRect();
                x_org = rect_w.x;
                y_org = rect_w.y;
                move = true;
                document.body.classList.add('user-select-none');
            }
        }, 10);
    }

    // 拖动开始事件，要绑定在被移动元素上
    groupItem.addEventListener('mousedown', mousedown_fun)
    groupItem.addEventListener('touchstart', mousedown_fun, { passive: false })

    const mousemove_fun = function (event) {
        if (move) {
            //获取鼠标按下时的时间
            var touch;
            if (event.touches) {
                touch = event.touches[0];//多个鼠标|手指事件取第一个
            } else {
                touch = event;
            }
            //阻止页面的滑动默认事件
            event.preventDefault();



            const cx = touch.clientX;
            const cy = touch.clientY;
            /** 相减即可得到相对于父元素移动的位置 */
            dx = cx - x_org;
            dy = cy - y_org;
            //console.log("坐标", cx, cy)
            //console.log("移动", dx, dy)
            var rect = live2dItem.getBoundingClientRect();
            //console.log("位移", rect.x + dx, rect.y + dy);
            //限定范围
            //if (dx < 0) dx = 0
            //if (dy < 0) dy = 0
            //if (dx > rect.width) dx = rect.width
            //if (dy > rect.height) dy = rect.height

            groupItem.setAttribute('style', `left:${dx}px; top:${dy}px;`)
        }
        else {
            clearInterval(time);
        }
    }

    // 移动触发事件要放在，区域控制元素上
    window.addEventListener('mousemove', mousemove_fun)
    window.addEventListener('touchmove', mousemove_fun, { passive: false })

    const mouseup_fun = function () {
        buttongroup_mousedown = false;
        var rect_w = live2dItem.getBoundingClientRect();
        var rect_n = groupItem.getBoundingClientRect();
        if (move) {
            move = false;
            console.log('mouseup');
            dotNetHelper.invokeMethodAsync('SetButtonGroupPosition', dx, rect_w.height - dy - rect_n.height);
            document.body.classList.remove('user-select-none');
        }
        else {
            clearInterval(time);
        }
    }

    // 拖动结束触发要放在，区域控制元素上
    window.addEventListener('mouseup', mouseup_fun)
    window.addEventListener('touchend', mouseup_fun, { passive: false })

}

function initDialogBoxMoveAction(dotNetHelper) {
    const itemId = 'kanban-dialogbox';
    const live2dItem = document.getElementById('kanban-live2d');
    const groupItem = document.getElementById(itemId);

    let move = false;
    let deltaLeft = 0, deltaTop = 0;
    let x_org, y_org;
    let dx = 0;
    let dy = 0;
    var time;

    const mousedown_fun = function (event) {
        /*
        * @des deltaLeft 即移动过程中不变的值
        */

        var touch;
        if (event.touches) {
            touch = event.touches[0];//多个鼠标|手指事件取第一个
        } else {
            touch = event;
        }
        //获取鼠标按下时的时间
        timeStart = getTimeNow();

        dialogbox_mousedown = true;

        //setInterval会每100毫秒执行一次，也就是每100毫秒获取一次时间
        time = setInterval(function () {
            timeEnd = getTimeNow();

            //如果此时检测到的时间与第一次获取的时间差有1000毫秒
            if (timeEnd - timeStart > 500) {
                //便不再继续重复此函数 （clearInterval取消周期性执行）
                clearInterval(time);
                //执行逻辑
                deltaLeft = touch.clientX - touch.target.offsetLeft;
                deltaTop = touch.clientY - touch.target.offsetTop;
                //console.log("初始", deltaLeft, deltaTop);
                var rect_w = live2dItem.getBoundingClientRect();
                var rect_n = groupItem.getBoundingClientRect();
                x_org = rect_w.x - rect_n.x;
                y_org = rect_w.y - rect_n.y;
                move = true;
                document.body.classList.add('user-select-none');
            }
        }, 10);
    }

    // 拖动开始事件，要绑定在被移动元素上
    groupItem.addEventListener('mousedown', mousedown_fun)
    groupItem.addEventListener('touchstart', mousedown_fun, { passive: false })

    const mousemove_fun = function (event) {
        if (move) {
            var touch;
            if (event.touches) {
                touch = event.touches[0];//多个鼠标|手指事件取第一个
            } else {
                touch = event;
            }
            //阻止页面的滑动默认事件
            event.preventDefault();



            const cx = touch.clientX;
            const cy = touch.clientY;
            /** 相减即可得到相对于父元素移动的位置 */
            dx = cx - deltaLeft - x_org;
            dy = cy - deltaTop - y_org;
            //console.log("坐标", cx, cy)
            //console.log("移动", dx, dy)
            var rect = live2dItem.getBoundingClientRect();
            //console.log("位移", rect.x + dx, rect.y + dy);
            //限定范围
            //if (dx < -rect.width) dx = -rect.width
            //if (dy < -rect.height) dy = -rect.height
            //if (dx > 2*rect.width) dx = 2*rect.width
            //if (dy > 2*rect.height) dy = 2*rect.height

            groupItem.setAttribute('style', `left:${dx}px; top:${dy}px;`)
        }
        else {
            clearInterval(time);
        }
    }

    // 移动触发事件要放在，区域控制元素上
    window.addEventListener('mousemove', mousemove_fun)
    window.addEventListener('touchmove', mousemove_fun, { passive: false })

    const mouseup_fun = function () {
        dialogbox_mousedown = false;

        if (move) {
            move = false;
            console.log('mouseup');
            var rect_w = live2dItem.getBoundingClientRect();
            var rect_n = groupItem.getBoundingClientRect();
            dotNetHelper.invokeMethodAsync('SetDialogBoxPosition', dx, rect_w.height - dy - rect_n.height);
            document.body.classList.remove('user-select-none');
        }
        else {
            clearInterval(time);
        }
    }

    // 拖动结束触发要放在，区域控制元素上
    window.addEventListener('mouseup', mouseup_fun)
    window.addEventListener('touchend', mouseup_fun, { passive: false })

}


function initChatCardMoveAction(dotNetHelper) {
    const itemId = 'kanban-chatcard';
    const live2dItem = document.getElementById('kanban-live2d');
    const groupItem = document.getElementById(itemId);

    let move = false;
    let deltaLeft = 0, deltaTop = 0;
    let x_org, y_org;
    let dx = 0;
    let dy = 0;
    var time;

    const mousedown_fun = function (event) {
        /*
        * @des deltaLeft 即移动过程中不变的值
        */

        var touch;
        if (event.touches) {
            touch = event.touches[0];//多个鼠标|手指事件取第一个
        } else {
            touch = event;
        }
        //获取鼠标按下时的时间
        timeStart = getTimeNow();

        chatcard_mousedown = true;

        //setInterval会每100毫秒执行一次，也就是每100毫秒获取一次时间
        time = setInterval(function () {
            timeEnd = getTimeNow();

            //如果此时检测到的时间与第一次获取的时间差有1000毫秒
            if (timeEnd - timeStart > 500) {
                //便不再继续重复此函数 （clearInterval取消周期性执行）
                clearInterval(time);
                //执行逻辑
                deltaLeft = touch.clientX - touch.target.offsetLeft;
                deltaTop = touch.clientY - touch.target.offsetTop;
                //console.log("初始", deltaLeft, deltaTop);
                var rect_w = live2dItem.getBoundingClientRect();
                var rect_n = groupItem.getBoundingClientRect();
                x_org = rect_w.x - rect_n.x;
                y_org = rect_w.y - rect_n.y;
                move = true;
                document.body.classList.add('user-select-none');
            }
        }, 10);
    }

    // 拖动开始事件，要绑定在被移动元素上
    groupItem.addEventListener('mousedown', mousedown_fun)
    groupItem.addEventListener('touchstart', mousedown_fun, { passive: false })

    const mousemove_fun = function (event) {
        if (move) {
            var touch;
            if (event.touches) {
                touch = event.touches[0];//多个鼠标|手指事件取第一个
            } else {
                touch = event;
            }
            //阻止页面的滑动默认事件
            event.preventDefault();



            const cx = touch.clientX;
            const cy = touch.clientY;
            /** 相减即可得到相对于父元素移动的位置 */
            dx = cx - deltaLeft - x_org;
            dy = cy - deltaTop - y_org;
            //console.log("坐标", cx, cy)
            //console.log("移动", dx, dy)
            var rect = live2dItem.getBoundingClientRect();
            //console.log("位移", rect.x + dx, rect.y + dy);
            //限定范围
            //if (dx < -rect.width) dx = -rect.width
            //if (dy < -rect.height) dy = -rect.height
            //if (dx > 2*rect.width) dx = 2*rect.width
            //if (dy > 2*rect.height) dy = 2*rect.height

            groupItem.setAttribute('style', `left:${dx}px; top:${dy}px;`)
        }
        else {
            clearInterval(time);
        }
    }

    // 移动触发事件要放在，区域控制元素上
    window.addEventListener('mousemove', mousemove_fun)
    window.addEventListener('touchmove', mousemove_fun, { passive: false })

    const mouseup_fun = function () {
        chatcard_mousedown = false;

        if (move) {
            move = false;
            console.log('mouseup');
            var rect_w = live2dItem.getBoundingClientRect();
            var rect_n = groupItem.getBoundingClientRect();
            dotNetHelper.invokeMethodAsync('SetChatCardPosition', dx, rect_w.height - dy - rect_n.height);
            document.body.classList.remove('user-select-none');
        }
        else {
            clearInterval(time);
        }
    }

    // 拖动结束触发要放在，区域控制元素上
    window.addEventListener('mouseup', mouseup_fun)
    window.addEventListener('touchend', mouseup_fun, { passive: false })

}


//监听画布大小改变事件
function listenLive2dCanvasChange() {
    var _w = -1;
    var _h = -1;
    var interval = setInterval(function () {
        const live2dItem = document.getElementById(canvasId);
        if (live2dItem) {
            var rect = live2dItem.getBoundingClientRect();
            if (_w > 0 && _h > 0) {
                if (_w != rect.width || _h != rect.height) {
                    resizeLive2d();
                }
            }
            else {
                _w = rect.width;
                _h = rect.height;
            }
        }
        else {
            clearInterval(interval);
        }

    }, 500);
}

//切换模型
function switchLiv2DModel(id) {
    var manger = getLive2dManager();
    manger.changeScene(id);
}

//切换表情
function switchLiv2DExpression(id) {
    var manger = getLive2dManager();
    var model = manger.getCurrentModel();
    if (id) {
        model.setExpression(id);
    }
    else {
        model.cleanExpressions();
    }
}

//切换衣服
function switchLiv2DClothes(id) {
    var manger = getLive2dManager();
    var model = manger.getCurrentModel();
    if (id) {
        model.setClothes(id);
    }
    else {
        model.cleanClothes();
    }
}

//切换丝袜
function switchLiv2DStockings(id) {
    var manger = getLive2dManager();
    var model = manger.getCurrentModel();
    if (id) {
        model.setStockings(id);
    }
    else {
        model.cleanStockings();
    }
}

//切换鞋子
function switchLiv2DShoes(id) {
    var manger = getLive2dManager();
    var model = manger.getCurrentModel();
    if (id) {
        model.setShoes(id);
    }
    else {
        model.cleanShoes();
    }
}

//切换动作
function switchLiv2DMotion(group, id) {
    var manger = getLive2dManager();
    var model = manger.getCurrentModel();
    if (id) {
        model.startMotion(group, id, 3);
    }
    else {
        model.startRandomMotion(group, 3);
    }
}

/*鼠标悬停事件*/
var l2d_dotNetHelper_event
function initMouseOverEvent(dotNetHelper, data) {
    l2d_dotNetHelper_event = dotNetHelper;
    console.log('初始化鼠标悬停事件', data)
    let lastHoverElement;
    window.addEventListener("mouseover", event => {
        for (let { selector, id } of data) {
            if (!event.target.closest(selector)) continue;
            if (lastHoverElement === selector) return;
            lastHoverElement = selector;
            console.log('触发鼠标悬停', selector)
            l2d_dotNetHelper_event.invokeMethodAsync('TriggerMouseOverEventAsync', id);
            return;
        }
    });
}

/*回到顶部*/
function scrollToTop() {
    window.scrollTo({
        top: 0,
        behavior: "smooth"
    })
}


/*获取当前看板娘图片*/
//function startKanbanImageGeneration(dotNetHelper) {
//    document.getElementById('live2d').toBlob((res) => {
//        let blobUrl = URL.createObjectURL(res);
//        console.log(blobUrl)
//        dotNetHelper.invokeMethodAsync('OnKanbanImageGenerated', blobUrl);
//    });
//}
function flipImageVertically(pixels, width, height) {
    const halfHeight = height / 2 | 0; // 向下取整
    for (let y = 0; y < halfHeight; ++y) {
        for (let x = 0; x < width; ++x) {
            for (let c = 0; c < 4; ++c) {
                const topIndex = (y * width + x) * 4 + c;
                const bottomIndex = ((height - y - 1) * width + x) * 4 + c;
                const temp = pixels[topIndex];
                pixels[topIndex] = pixels[bottomIndex];
                pixels[bottomIndex] = temp;
            }
        }
    }
}

/*获取当前看板娘图片*/
function startKanbanImageGeneration(dotNetHelper, x, y, width, height) {
    // 获取 WebGL 上下文
    const canvas = document.getElementById('live2d');
    const gl = canvas.getContext('webgl');

    // 设置读取的矩形范围
    //const x = 0; // 起始 x 坐标
    //const y = 0; // 起始 y 坐标
    //const width = 300; // 矩形的宽度
    //const height = 500; // 矩形的高度

    // 创建一个数组来存储像素数据
    const pixels = new Uint8Array(width * height * 4); // 4 表示 RGBA 四个通道

    // 读取像素数据
    gl.readPixels(x, y, width, height, gl.RGBA, gl.UNSIGNED_BYTE, pixels);

    // 翻转像素数据
    flipImageVertically(pixels, width, height);

    // 创建一个新的 Canvas 用于绘制图像数据
    const offscreenCanvas = document.getElementById('live2d-cache');
    offscreenCanvas.width = width;
    offscreenCanvas.height = height;
    const ctx = offscreenCanvas.getContext('2d');


    // 创建 ImageData 对象
    const imageData = new ImageData(new Uint8ClampedArray(pixels), width, height);

    // 将 ImageData 绘制到 Canvas 上
    ctx.putImageData(imageData, 0, 0);
    //ctx.rotate(Math.PI);

    // 将 Canvas 内容转换为 Blob
    offscreenCanvas.toBlob((blob) => {
        // 生成 Blob URL
        const url = URL.createObjectURL(blob);
        console.log(url); // 输出 Blob URL
        dotNetHelper.invokeMethodAsync('OnKanbanImageGenerated', url);
    }, 'image/png');
}


/*摸头事件*/
window.Live2dHitHeadEvent = function () {
    l2d_dotNetHelper_event.invokeMethodAsync('TriggerCustomEventAsync', '摸头');
}
/*触碰身体事件*/
window.Live2dHitBodyEvent = function () {
    l2d_dotNetHelper_event.invokeMethodAsync('TriggerCustomEventAsync', '触碰身体');
}
