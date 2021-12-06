// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('online', event => event.waitUntil(onLine(event)));
self.addEventListener('offline', event => event.waitUntil(offLine(event)));

async function onInstall(event) {
    console.info('服务工作进程：注册');
}

async function onFetch(event) {
    let cachedResponse = null;
    const cache = await caches.open('blazor_pwa');

    //判断是否为 GET
    if (event.request.method === 'GET') {

        //判断是否为离线
        if (navigator.onLine == true) {

            //console.info('Service worker: Online');


        } else {
            //console.info('Service worker: OffLine');
            const request = event.request;
            cachedResponse = await caches.match(request);
            if (cachedResponse) {
                return cachedResponse;
            }

        }

        //在线状态 或本地无缓存
        try {
            var resp = await fetch(event.request);
            //判断回复是否为 404
            if (resp.status != 200) {

                //找到缓存回复
                const request = event.request;
                cachedResponse = await caches.match(request);
                if (cachedResponse) {
                    return cachedResponse;
                }
                else {
                    return resp;
                }
            }
            else {
                cache.put(event.request, resp.clone());
                return resp;
            }
        }
        catch
        {
            //找到缓存回复
            const request = event.request;
            cachedResponse = await caches.match(request);
            if (cachedResponse) {
                return cachedResponse;
            }
            else {
                return resp;
            }
        }

    }

    return fetch(event.request);
}
