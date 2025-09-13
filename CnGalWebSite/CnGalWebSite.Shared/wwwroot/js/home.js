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

/**
* 保存div为图片
*
* 该访问为外部方法，如需使用，请解开注解，并且添加js引用
* js引用地址：
* @param {string} id - 要保存为图片的div的id
* @param {string} fileName - 保存的文件名
* @param {boolean} copyToClipboard - 是否复制到剪贴板，true表示复制到剪贴板，false或不传表示下载
*/

function saveDivAsImage(id, fileName, copyToClipboard) {
    var element = document.getElementById(id);

    // 临时修改样式确保元素完全可见
    element.classList.remove("mobile-card");
    var originalWidth = element.style.width;

    // 只有当元素宽度小于1000px时才将其调整为1200px
    if (element.offsetWidth < 1000) {
        element.style.width = "1200px";
    }

    html2canvas(element, {
        scale: 2, // 缩放比例，提高清晰度
        useCORS: true, // 允许加载跨域图片
        allowTaint: true, // 允许图片污染画布
        backgroundColor: null, // 保留背景透明
        logging: false, // 关闭日志记录以提高性能
        imageTimeout: 0, // 不限制图片加载时间
        width: element.offsetWidth, // 明确设置宽度
        height: element.offsetHeight, // 明确设置高度
        x: 0, // 从元素左上角开始捕获
        y: 0, // 从元素左上角开始捕获
        scrollX: 0, // 禁用水平滚动
        scrollY: 0, // 禁用垂直滚动
        windowWidth: document.documentElement.offsetWidth, // 设置窗口宽度
        windowHeight: document.documentElement.offsetHeight // 设置窗口高度
    }).then(canvas => {
        // 使用最高质量导出PNG
        var MIME_TYPE = "image/png";
        var imgURL = canvas.toDataURL(MIME_TYPE, 1.0);

        if (copyToClipboard) {
            // 复制到剪贴板
            // 由于不能直接复制图片到剪贴板，我们需要先创建一个blob
            canvas.toBlob(function (blob) {
                try {
                    // 尝试使用新的Clipboard API (适用于较新的浏览器)
                    const clipboardItem = new ClipboardItem({ 'image/png': blob });
                    navigator.clipboard.write([clipboardItem]).then(() => {
                    }).catch(err => {
                        console.error('剪贴板复制失败:', err);
                        fallbackCopyToClipboard(imgURL, fileName);
                    });
                } catch (e) {
                    // 回退方案
                    fallbackCopyToClipboard(imgURL, fileName);
                }
            }, 'image/png', 1.0);
        } else {
            // 下载图片
            const anchorElement = document.createElement('a');
            anchorElement.href = imgURL;
            anchorElement.download = fileName || '';
            document.body.appendChild(anchorElement);
            anchorElement.click();
            document.body.removeChild(anchorElement);
        }

        // 恢复原始样式
        element.style.width = originalWidth;
        element.classList.add("mobile-card");
    }).catch(error => {
        console.error('截图出错：', error);

        // 发生错误时也要恢复原始样式
        element.style.width = originalWidth;
        element.classList.add("mobile-card");
    });
}

/**
 * 回退方案：将图片URL复制到剪贴板或下载
 * @param {string} imgURL - 图片的DataURL
 * @param {string} fileName - 文件名
 */
function fallbackCopyToClipboard(imgURL, fileName) {
    // 创建一个临时的图片元素
    const img = document.createElement('img');
    img.src = imgURL;
    img.style.position = 'fixed';
    img.style.left = '-9999px';
    document.body.appendChild(img);

    // 尝试复制图片元素
    try {
        // 创建一个range和selection
        const range = document.createRange();
        range.selectNode(img);
        const selection = window.getSelection();
        selection.removeAllRanges();
        selection.addRange(range);

        // 尝试复制
        const success = document.execCommand('copy');
        if (success) {

        } else {
            // 如果复制失败，尝试下载
            const anchorElement = document.createElement('a');
            anchorElement.href = imgURL;
            anchorElement.download = fileName || '';
            document.body.appendChild(anchorElement);
            anchorElement.click();
            document.body.removeChild(anchorElement);
            alert('无法复制到剪贴板，已改为下载图片');
        }
    } catch (e) {
        console.error('回退复制失败:', e);
        // 如果出错，尝试下载
        const anchorElement = document.createElement('a');
        anchorElement.href = imgURL;
        anchorElement.download = fileName || '';
        document.body.appendChild(anchorElement);
        anchorElement.click();
        document.body.removeChild(anchorElement);
        alert('无法复制到剪贴板，已改为下载图片');
    } finally {
        // 清理
        document.body.removeChild(img);
        window.getSelection().removeAllRanges();
    }
}

