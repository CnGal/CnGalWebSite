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

function initGeetestCaptcha(formId, scenario) {
    var form = document.getElementById(formId);
    if (!form) {
        return;
    }

    var captchaBox = form.querySelector('#captcha-box');
    if (!captchaBox) {
        return;
    }

    if (!scenario) {
        scenario = 'Login';
    }

    var client = new HttpClient();
    client.get('/api/tool/GetGeetestCode?scenario=' + encodeURIComponent(scenario), function (response) {
        var re = JSON.parse(response);
        var gt = re.gt;

        initGeetest4({
            captchaId: gt,
            product: 'float',
            displayMode: 1,
            nativeButton: {
                width: '100%',
                height: '48px'
            }
        }, function (captchaObj) {
            captchaObj.appendTo(captchaBox);

            captchaObj.onSuccess(function () {
                var result = captchaObj.getValidate();

                var lotNumber = form.querySelector('#geetestResult-lotNumber');
                var captchaOutput = form.querySelector('#geetestResult-captchaOutput');
                var passToken = form.querySelector('#geetestResult-passToken');
                var genTime = form.querySelector('#geetestResult-genTime');

                if (lotNumber) {
                    lotNumber.value = result.lot_number;
                }
                if (captchaOutput) {
                    captchaOutput.value = result.captcha_output;
                }
                if (passToken) {
                    passToken.value = result.pass_token;
                }
                if (genTime) {
                    genTime.value = result.gen_time;
                }
            });

            captchaObj.onError(function () {
            });
        });
    });
}
