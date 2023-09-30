using CnGalWebSite.Components.Models;
using Microsoft.AspNetCore.Components;

namespace CnGalWebSite.Components.Services
{
    public class ImagesLargeViewService
    {
        /// <summary>
        /// 获得 回调委托缓存集合
        /// </summary>
        private List<(IComponent Key, Func<List< ImagesLargeViewModel>,int, ValueTask> Callback)> Cache { get; set; } = new();

        /// <summary>
        /// 设置当前页面 Title 方法
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public async ValueTask ViewLargeImages(List<ImagesLargeViewModel> model, int index)
        {
            var cb = Cache.FirstOrDefault().Callback;
            if (cb != null)
            {
                await cb.Invoke(model,index);
            }
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        public void Register(IComponent key, Func<List<ImagesLargeViewModel>, int, ValueTask> callback)
        {
            Cache.Add((key, callback));
        }

        /// <summary>
        /// 注销事件
        /// </summary>
        public void UnRegister(IComponent key)
        {
            var item = Cache.FirstOrDefault(i => i.Key == key);
            if (item.Key != null)
            {
                Cache.Remove(item);
            }
        }
    }
}
