import '/_content/CnGalWebSite.MainSite.Shared/scripts/geetest-loader.js';
import { own } from '/_content/CnGalWebSite.MainSite.Shared/scripts/component-lifecycle.js';

export function create(root, callback) {
    let widget, version = 0, disposed = false;
    const send = (method, ...args) => callback.invokeMethodAsync(method, ...args)
        .catch(error => console.warn('[CgHumanMachineVerification] callback', error));
    return own(root, {
        start(gt, challenge, success) {
            const generation = ++version;
            widget?.destroy?.(); widget = null;
            window.initGeetest({ gt, challenge, offline: !success, new_captcha: true, product: 'bind' }, captcha => {
                if (disposed || generation !== version) { captcha.destroy?.(); return; }
                widget = captcha;
                const active = () => !disposed && generation === version;
                captcha.onReady(() => { if (active()) captcha.verify(); });
                captcha.onSuccess(() => {
                    if (!active()) return;
                    const result = captcha.getValidate();
                    send('OnChecked', result.geetest_challenge, result.geetest_validate, result.geetest_seccode);
                });
                captcha.onError(() => { if (active()) send('OnCancel'); });
                captcha.onClose(() => { if (active()) send('OnCancel'); });
            });
        },
        dispose() { if (disposed) return; disposed = true; version++; widget?.destroy?.(); widget = null; }
    });
}
