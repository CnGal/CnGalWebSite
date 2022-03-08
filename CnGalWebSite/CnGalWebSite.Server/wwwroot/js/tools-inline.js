
function openNewPage(url) {
    window.open(url, "_blank");
    /*var windowReference = window.open();
    $.ajax({
        url: "url", success: function (result) {
            windowReference.location = url;
        }
    });*/
}
function setInnerHTML(id, html) {
    var box = document.getElementById(id)
    if (box == null) {
        return;
    }
    if (box.innerHTML.length == 0) {
        box.innerHTML = html;
    }
}
function cleanInnerHTML(id) {
    var box = document.getElementById(id)
    if (box == null) {
        return;
    }
    if (box.innerHTML.length >0) {
        box.innerHTML = "";
    }

}
function deleteDiv(classname) {
    var boxs = document.getElementsByClassName(classname);
    if (boxs == null) {
        return;
    }
    for (var i = 0; i < boxs.length; i++) {
        boxs[i].remove();
    }
}
function copyUrl() {
    var clipBoardContent = this.location.href;
    window.clipboardData.setData("Text", clipBoardContent);
    alert("复制成功!");
}
function tool_scroll(t, i) {

    if (i === "dispose") {

        window.onscroll = null;
        return
    }
    var r = function () {
        var t2 = document.body.scrollHeight;
        var t1 = document.documentElement.scrollTop || document.body.scrollTop;
        var r = t2 - t1;
        t.invokeMethodAsync(i, r)
    };
    r();
    window.onscroll = function () { r() };
}
function getScrollBottom() {
    this.docScrollTop = document.documentElement && document.documentElement.scrollTop
    // console.log(this.docScrollTop, '高度')    
    this.docClientHeight = document.body.clientHeight && document.documentElement.clientHeight
    // console.log(this.docClientHeight, '页面高度')
    this.docScrollHeight = document.body.scrollHeight
    // console.log(this.docScrollHeight, '文档实际高度')
    const aaa = this.docScrollHeight - this.docScrollTop - this.docClientHeight

    return aaa;
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

var _czc = _czc || [];
_czc.push(["_setAccount", "1280206141"]);

function trackEvent(categotry, action, lable, value, nodeid) {
    _czc.push(['_trackEvent', categotry, action, lable, value, nodeid]);
}
var editor;
function initEditorMd(markdownstring) {
    $(function () {
        editor = editormd("editor", {
            htmlDecode: true,
            width: "100%",
            height: "100%",
            autoFocus: false,
           // tocm: true, // Using [TOCM]
           // tex: true, // 开启科学公式TeX语言支持，默认关闭
           // flowChart: true, // 开启流程图支持，默认关闭
            placeholder: "这里是Markdown编辑器，可以点击右侧问号了解语法详情",
            markdown: markdownstring,     // dynamic set Markdown text
            path: "_content/CnGalWebSite.Shared/editor.md/lib/"  // Autoload modules mode, codemirror, marked... dependents libs path
        });
    });
}

function getEditorMdContext() {
    return editor.getMarkdown();
}
function addEditorMdContext(markdownstring) {
    editor.appendMarkdown(markdownstring);
}

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


/*以下为加载时自动执行的代码*/

/*显示重连提示*/
window.setInterval(showalert, 500);
var count = 0;
function showalert() {

    if ($("#components-reconnect-modal").css("display") != 'none') {
        count++;
        if (count > 10) {
            $("#components-reconnect-modal-infor").addClass("d-block");
        }
    }
    else {
        $("#components-reconnect-modal-infor").removeClass("d-block");
        count = 0;
    }
    if (document.querySelector('.components-reconnect-failed')
        || document.querySelector('.components-reconnect-rejected')) {
        location.reload();
    }
}

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
        if (userAgent.split('Edge/')[1].split('.')[0] < 16) {
            return 1;
        }
        else {
            return 0;
        }
    } else if (isFirefox) {
        if (userAgent.split('Firefox/')[1].split('.')[0] < 53) {
            return 1;
        }
        else {
            return 0;
        }
    } else if (isOpera) {
        if (userAgent.split('OPR/')[1].split('.')[0] < 44) {
            return 1;
        }
        else {
            return 0;
        }
    } else if (isChrome) {
        if (userAgent.split('Chrome/')[1].split('.')[0] < 56) {
            return 1;
        }
        else {
            return 0;
        }
    } else if (isSafari) {
        if (userAgent.split('Safari/')[1].split('.')[0] < 11) {
            return 1;
        }
        else {
            return 0;
        }
    } else {
        return -1;//不是ie浏览器
    }
}

browserVersion();

if (browserVersion() == 1) {
    window.location.href = "/_content/CnGalWebSite.Shared/pages/NotSupported.html";
}

/*分享链接*/
function shareLink(icon, link, title, desc) {
    var nativeShare = new NativeShare();

    // 设置分享文案
    nativeShare.setShareData({
        icon: icon,
        link: link,
        title: title,
        desc: desc
    })

    // 唤起浏览器原生分享组件(如果在微信中不会唤起，此时call方法只会设置文案。类似setShareData)
    try {
        nativeShare.call()
        // 如果是分享到微信则需要 nativeShare.call('wechatFriend')
        // 类似的命令下面有介绍
    } catch (err) {
        // 如果不支持，你可以在这里做降级处理
    }
}

/*加载友盟js*/
var cnzz_s_tag = document.createElement('script');
cnzz_s_tag.type = 'text/javascript';
cnzz_s_tag.async = true;
cnzz_s_tag.charset = "utf - 8";
cnzz_s_tag.src = "https://w.cnzz.com/c.php?id=1280206141&async=1";
var root_s = document.getElementsByTagName('script')[0];
root_s.parentNode.insertBefore(cnzz_s_tag, root_s);

