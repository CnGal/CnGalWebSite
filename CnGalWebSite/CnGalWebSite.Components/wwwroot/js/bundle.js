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
        if (userAgent.split('Safari/')[1].split('.')[0] < 15) {
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
