//var resourcesPath = 'http://localhost:8080/Samples/Resources/';  // 指定资源文件（模型）保存的路径
var backImageName = ''; // 指定背景图片
//var modelDir = '波奇酱2.0,Hiyori,Haru,Mao,Mark,Natori,Rice';  // 指定需要加载的模型
var canvasId = 'live2d';//画布Id

var live2d_dotNetHelper;

function initKanbanLive2D(dotNetHelper, modelDir, modelIndex, resourcesPath) {
    var modelDirs = modelDir.split(',');
    live2d_dotNetHelper = dotNetHelper;

    setLive2dDefine(resourcesPath, backImageName, modelDirs, canvasId, modelIndex);
    initLive2d();
    initKanbanMoveAction(dotNetHelper);
    initButtonGroupMoveAction(dotNetHelper);
    initDialogBoxMoveAction(dotNetHelper);
    listenLive2dCanvasChange();

    console.log("初始化Live2D")
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

//获取此刻时间
function getTimeNow() {
    var now = new Date();
    return now.getTime();
}

var kanban_mousedown = false;
var buttongroup_mousedown = false;
var dialogbox_mousedown = false;

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
            }
        }, 100);
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
        }, 100);
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
    window.addEventListener('mousemove', mousemove_fun )
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
        }, 100);
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
    window.addEventListener('mousemove', mousemove_fun )
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
function initMouseOverEvent(dotNetHelper, data) {
    console.log('初始化鼠标悬停事件', data)
    let lastHoverElement;
    window.addEventListener("mouseover", event => {
        for (let { selector, id } of data) {
            if (!event.target.closest(selector)) continue;
            if (lastHoverElement === selector) return;
            lastHoverElement = selector;
            console.log('触发鼠标悬停', selector)
            dotNetHelper.invokeMethodAsync('TriggerMouseOverEventAsync', id);
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
function startKanbanImageGeneration(dotNetHelper) {
    document.getElementById('live2d').toBlob((res) => {
        let blobUrl = URL.createObjectURL(res);
        console.log(blobUrl)
        dotNetHelper.invokeMethodAsync('OnKanbanImageGenerated', blobUrl);
    });
}
