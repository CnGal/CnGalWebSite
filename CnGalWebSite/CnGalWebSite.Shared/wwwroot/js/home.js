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
