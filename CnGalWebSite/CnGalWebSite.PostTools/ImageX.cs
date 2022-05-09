using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Files;
using CnGalWebSite.DataModel.ViewModel.HistoryData;
using CnGalWebSite.Helper.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.PostTools
{
    public class ImageX
    {
        private readonly HttpClient client;
        private readonly SettingX setting;
        public readonly List<OriginalImageToDrawingBedUrl> images = new List<OriginalImageToDrawingBedUrl>();


        public ImageX(HttpClient _client, SettingX _setting)
        {
            client = _client;
            setting = _setting;
        }

        public void Load()
        {
            try
            {
                var path = Path.Combine(setting.TempPath, "images.json");

                using (var file = File.OpenText(path))
                {
                    var serializer = new JsonSerializer();
                    images.AddRange((List<OriginalImageToDrawingBedUrl>)serializer.Deserialize(file, typeof(List<OriginalImageToDrawingBedUrl>)));
                }
            }
            catch
            {
                Save();
            }

        }
        public void Save()
        {
            var path = Path.Combine(setting.TempPath, "images.json");

            using (var file = File.CreateText(path))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, images);
            }
        }

        public async Task<string> UploadImageAsync(string url, double x = 0, double y = 0)
        {
            try
            {
                var result = await client.PostAsJsonAsync(setting.TransferDepositFileAPI + "api/files/TransferDepositFile", new TransferDepositFileModel
                {
                    Url = url,
                    X = x,
                    Y = y
                });

                var jsonContent = result.Content.ReadAsStringAsync().Result;
                var obj = System.Text.Json.JsonSerializer.Deserialize<Result>(jsonContent, ToolHelper.options);

                if (obj.Successful)
                {
                    var temp = obj.Error.Replace("http://local.host/", "https://pic.cngal.top/");
                    Console.WriteLine("上传完成：" + temp);

                    return temp;
                }
                else
                {
                    OutputHelper.Write(OutputLevel.Dager, "上传失败：" + url);
                    return url;
                }
            }
            catch (Exception)
            {
                return url;
            }

        }

        public async Task CutImage(OutlinkArticleModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Image) == false && model.IsCutImage == false)
            {
                model.Image = await UploadImageAsync(model.Image, 460, 215);
                model.IsCutImage = model.Image.Contains("image.cngal");
                if (model.IsCutImage)
                {
                    Console.WriteLine("成功裁剪主图");
                }
                else
                {
                    OutputHelper.Write(OutputLevel.Dager, "裁剪主图失败，请在提交完成后手动修正");
                }
            }
        }

        public async Task<string> ProgressImage(string text, OutlinkArticleType type)
        {
            if (type == OutlinkArticleType.ZhiHu)
            {

                var figures = text.Split("</figure>");
                foreach (var item in figures)
                {
                    var textTemp = item.Split("<figure");
                    if (textTemp.Length > 1)
                    {
                        var image = ToolHelper.MidStrEx(textTemp[1], "data-original=\"", "\">");
                        var newImage = await GetImage(image);

                        text = text.Replace("<figure" + ToolHelper.MidStrEx(text, "<figure", "</figure>") + "</figure>", "\n![image](" + newImage + ")\n");
                    }
                }

            }
            else if (type == OutlinkArticleType.XiaoHeiHe)
            {

                var images = ToolHelper.GetImageLinks(text);
                foreach (var temp in images)
                {
                    var infor = await GetImage(temp.Replace("/thumb", ""));

                    //替换图片
                    text = text.Replace(temp, infor);
                }
            }
            return text;
        }

        public async Task<string> GetImage(string url)
        {
            var image = images.FirstOrDefault(s => s.OldUrl == url);
            if (image == null)
            {
                var newUrl = await UploadImageAsync(url);
                images.Add(new OriginalImageToDrawingBedUrl
                {
                    NewUrl = newUrl,
                    OldUrl = url
                });
                Save();
                //await Task.Delay(1500);
                return newUrl;
            }
            else
            {
                return image.NewUrl;
            }
        }
    }
}
