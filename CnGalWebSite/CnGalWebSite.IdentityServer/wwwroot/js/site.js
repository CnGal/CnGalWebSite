function callGeetest() {
    //获取页面初始化数据
    gt = document.getElementById('geetestCode-gt').innerText;
    challenge = document.getElementById('geetestCode-challenge').innerText;
    success = document.getElementById('geetestCode-success').innerText;

    initGeetest({
        // 以下配置参数来自服务端 SDK
        gt: gt,
        challenge: challenge,
        offline: !success,
        new_captcha: true,
        width: '100%',
        product: 'bind'
    }, function (captchaObj) {
        // 这里可以调用验证实例 captchaObj 的实例方法
        captchaObj.appendTo('#captcha-box');
        captchaObj.onReady(function () {
            //验证码ready之后才能调用verify方法显示验证码
            captchaObj.verify();
        });
        //成功回调
        captchaObj.onSuccess(function () {
            var result = captchaObj.getValidate();
            //设置数据
            document.getElementById('geetestResult-validate').value = result.geetest_validate;
            document.getElementById('geetestResult-challenge').value = result.geetest_challenge;
            document.getElementById('geetestResult-seccode').value = result.geetest_seccode;
            //提交表单
            document.getElementById('geetestCode-submit-button').click();
        });
    });
}
