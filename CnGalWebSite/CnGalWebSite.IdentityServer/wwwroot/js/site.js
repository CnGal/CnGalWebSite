var HttpClient = function () {
    this.get = function (aUrl, aCallback) {
        var anHttpRequest = new XMLHttpRequest();
        anHttpRequest.onreadystatechange = function () {
            if (anHttpRequest.readyState == 4 && anHttpRequest.status == 200)
                aCallback(anHttpRequest.responseText);
        }

        anHttpRequest.open("GET", aUrl, true);
        anHttpRequest.send(null);
    }
}
i = 0;
function callGeetest(buttonId, scenario) {
    buttonId = buttonId || 'geetestCode-submit-button';
    scenario = scenario || 'Login';
    if (i != 0) {
        i = 0;
        return true;
    }
    var client = new HttpClient();
    client.get('/api/tool/GetGeetestCode?scenario=' + encodeURIComponent(scenario), function (response) {
        //获取页面初始化数据
        re= JSON.parse(response);
        gt = re.gt;

        initGeetest4({
            captchaId: gt,
            product: 'bind'
        }, function (captchaObj) {
            captchaObj.onReady(function () {
                //验证码ready之后才能调用验证实例 captchaObj 的实例方法显示验证码
                captchaObj.showCaptcha();
            });
            //成功回调
            captchaObj.onSuccess(function () {
                i = 1;
                var result = captchaObj.getValidate();
                //设置数据
                document.getElementById('geetestResult-lotNumber').value = result.lot_number;
                document.getElementById('geetestResult-captchaOutput').value = result.captcha_output;
                document.getElementById('geetestResult-passToken').value = result.pass_token;
                document.getElementById('geetestResult-genTime').value = result.gen_time;
                //提交表单
                document.getElementById(buttonId).click();
            });
            captchaObj.onError(function () {
            });
        });
    });
    return false;
}
