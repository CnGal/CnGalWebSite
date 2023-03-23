/*以下为加载时自动执行的代码*/
Blazor.start({
    reconnectionOptions: {
        maxRetries: 3,
        retryIntervalMilliseconds: 2000
    }
});
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
        if (userAgent.split('Firefox/')[1].split('.')[0] < 72) {
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
    alert("貌似这个网站不支持你的浏览器哦\n你可以选择安装Edge，Chorme，Firefox或其他支持最新Web标准的浏览器，再重新进入\n如果使用的是QQ浏览器或360极速浏览器，请右击网页，选择极速模式，并重新打开标签页\n有任何疑问可以加群 761794704 进行反馈");
}

(function (c, l, a, r, i, t, y) {
    c[a] = c[a] || function () { (c[a].q = c[a].q || []).push(arguments) };
    t = l.createElement(r); t.async = 1; t.src = "https://www.clarity.ms/tag/" + i;
    y = l.getElementsByTagName(r)[0]; y.parentNode.insertBefore(t, y);
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
