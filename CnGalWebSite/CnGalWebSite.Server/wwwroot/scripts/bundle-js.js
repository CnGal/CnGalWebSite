function InitMouse() {
    const canvas = document.createElement('canvas');
    canvas.id = 'mouse-canvas';
    document.body.appendChild(canvas);
    const ctx = canvas.getContext('2d');
    const colours = ['#F73859', '#14FFEC', '#00E0FF', '#FF99FE', '#FAF15D'];

    let balls = [];
    let pressed = false;
    let longPressed = false;
    let longPress;
    let multiplier = 0;
    let width, height;
    let origin;
    let normal;

    // Make the canvas high res
    function updateSize() {
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
        canvas.style.width = window.innerWidth + 'px';
        canvas.style.height = window.innerHeight + 'px';
        ctx.scale(1, 1);

        width = canvas.width = window.innerWidth;
        height = canvas.height = window.innerHeight;
        origin = {
            x: width,
            y: height
        };
        normal = {
            x: width,
            y: height
        };
    }

    updateSize();
    window.addEventListener('resize', updateSize, false);

    class Ball {
        constructor(x = origin.x, y = origin.y) {
            this.x = x;
            this.y = y;
            this.angle = Math.PI * 2 * Math.random();

            if (longPressed == true) {
                this.multiplier = randBetween(14 + multiplier, 15 + multiplier);
            } else {
                this.multiplier = randBetween(6, 12);
            }

            this.vx = (this.multiplier + Math.random() * 0.5) * Math.cos(this.angle);
            this.vy = (this.multiplier + Math.random() * 0.5) * Math.sin(this.angle);
            this.r = randBetween(5, 10) + 3 * Math.random();
            this.color = colours[Math.floor(Math.random() * colours.length)];
            this.direction = randBetween(-1, 1);
        }

        update() {
            this.x += this.vx - normal.x;
            this.y += this.vy - normal.y;

            normal.x = (-2 / window.innerWidth) * Math.sin(this.angle);
            normal.y = (-2 / window.innerHeight) * Math.cos(this.angle);
            // normal.y = ((-2 / window.innerHeight) * Math.cos(this.angle)) + this.direction;

            this.r -= 0.3;
            this.vx *= 0.9;
            this.vy *= 0.9;
        }
    }

    function pushBalls(count = 1, x = origin.x, y = origin.y) {
        for (let i = 0; i < count; i++) {
            balls.push(new Ball(x, y));
        }
    }

    function randBetween(min, max) {
        return Math.floor(Math.random() * max) + min;
    }

    loop();

    function loop() {
        // Alpha means "motion blur", yay!
        // ctx.fillStyle = 'rgba(20, 24, 41, 0.0)';
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        // ctx.fillRect(0, 0, canvas.width, canvas.height);

        for (let i = 0; i < balls.length; i++) {
            let b = balls[i];

            if (b.r < 0) continue;

            ctx.fillStyle = b.color;
            ctx.beginPath();
            ctx.arc(b.x, b.y, b.r, 0, Math.PI * 2, false);
            ctx.fill();

            b.update();
        }

        if (longPressed == true) {
            multiplier += multiplier <= 10 ? 0.2 : 0.0;
        } else if (!longPressed && multiplier >= 0) {
            multiplier -= 0.4;
        }

        removeBall();
        requestAnimationFrame(loop);
    }

    function removeBall() {
        for (let i = 0; i < balls.length; i++) {
            let b = balls[i];
            if (
                b.x + b.r < 0 ||
                b.x - b.r > width ||
                b.y + b.r < 0 ||
                b.y - b.r > height ||
                b.r < 0
            ) {
                balls.splice(i, 1);
            }
        }
    }

    window.addEventListener(
        'mousedown',
        function (e) {
            // if (pressed == false) clearInterval(timeOut);

            pressed = true;
            pushBalls(randBetween(10, 20), e.clientX, e.clientY);

            document.body.classList.add('is-pressed');
            longPress = setTimeout(function () {
                document.body.classList.add('is-longpress');
                longPressed = true;
            }, 750);
        },
        false
    );

    window.addEventListener(
        'mouseup',
        function (e) {
            clearInterval(longPress);
            //multiplier = 0;

            // Superblast
            if (longPressed == true) {
                document.body.classList.remove('is-longpress');
                pushBalls(
                    randBetween(50 + Math.ceil(multiplier), 100 + Math.ceil(multiplier)),
                    e.clientX,
                    e.clientY
                );
                longPressed = false;
            }

            document.body.classList.remove('is-pressed');
        },
        false
    );

    // Keep it going
    // let timeOut = setInterval(function() {
    //   pushBalls(
    //     randBetween(10, 20),
    //     origin.x + randBetween(-50, 50),
    //     origin.y + randBetween(-50, 50)
    //   );
    // }, 200);

    // Pointer stuff
    const pointer = document.createElement('span');
    pointer.classList.add('mouse-pointer');
    document.body.appendChild(pointer);

    window.addEventListener(
        'mousemove',
        function (e) {
            let x = e.clientX;
            let y = e.clientY;

            pointer.style.top = y + 'px';
            pointer.style.left = x + 'px';
        },
        false
    );

    document.addEventListener('selectionchange', function () {
        if (pressed) {
            clearInterval(longPress);
            longPressed = false;
            document.body.classList.remove('is-pressed');
            document.body.classList.remove('is-longpress');
        }
    });
}


function openNewPage(url) {
    window.open(url, "_blank");
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
                    jQuery(up_img_label).html('<i class="fa fa-spinner fa-spin" aria-hidden="true"></i> 上传中...');
                },
                success: function (res) {
                    objRef.invokeMethodAsync('UpLoaded', res.image.url + '||' + f.size);
                 //   document.getElementsByClassName("tui-editor-contents")[0].innerHTML='<a href="' + res.image.url + '"><img src="' + res.image.url + '" alt="' + res.image.title + '"></img></a>';
                    jQuery(up_img_label).html('<i class="fa fa-check" aria-hidden="true"></i> 上传成功,继续上传');
                },
                error: function () {
                    jQuery(up_img_label).html('<i class="fa fa-times" aria-hidden="true"></i> 上传失败，重新上传');
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
