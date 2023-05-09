// Js代码
var onscrolling = false;
window.onscroll = async function () {
    if (onscrolling == true) {
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
        console.log("===加载更多内容……===");
        if (dotNetHelper != null) {
            await dotNetHelper.invokeMethodAsync('LoadMore');
            console.log(`LoadMore`);
        }
    }

    onscrolling = false;
}
var dotNetHelper;

function initInfiniteScroll(helper) {
    dotNetHelper = helper;
    console.log(`init`);
}
