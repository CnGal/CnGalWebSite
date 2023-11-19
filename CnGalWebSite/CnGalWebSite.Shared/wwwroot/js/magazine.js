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
    document.getElementById(id).focus()
}
