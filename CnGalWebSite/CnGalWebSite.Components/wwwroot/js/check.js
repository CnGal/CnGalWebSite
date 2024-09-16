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
        if (userAgent.split('Version/')[1].split(' ')[0] < 17.6) {
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
