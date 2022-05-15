
function openNewPage(url) {
    window.open(url, "_blank");
    /*var windowReference = window.open();
    $.ajax({
        url: "url", success: function (result) {
            windowReference.location = url;
        }
    });*/
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

function trackEvent(categotry, action, lable, value, nodeid) {
    //_czc.push(['_trackEvent', categotry, action, lable, value, nodeid]);
    umami.trackEvent(lable,categotry  + ' ' + action);
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
                url: 'https://pic.cngal.top/api/1/upload/?format=json&key=00ee89a663169218bd1ddeab71d54ddf',
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
            objRef.invokeMethodAsync('OnSuccessed', result.geetest_challenge, result.geetest_validate,result.geetest_seccode);
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
        return 'true';
    } else {
　　　　return 'false';
　　}
};


