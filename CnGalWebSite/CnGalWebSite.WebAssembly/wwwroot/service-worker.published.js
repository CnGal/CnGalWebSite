// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations

self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [/\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/];
const offlineAssetsExclude = [ /^service-worker\.js$/ ];

async function onInstall(event) {
    console.info('服务工作进程：注册');

    // Fetch and cache all matching items from the assets manifest
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash }));
    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));
}

async function onActivate(event) {
    console.info('服务工作进程：正在运行');

    // Delete unused caches
   /* const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));*/
}

async function onFetch(event) {
    let cachedResponse = null;
    const cache = await caches.open(cacheName);
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
