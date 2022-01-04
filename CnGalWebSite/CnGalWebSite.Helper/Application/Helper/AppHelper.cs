using Microsoft.AspNetCore.Components.Forms;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Files;
using CnGalWebSite.DataModel.ViewModel.Files.Images;
using System.Net.Http.Json;
using System.Text.Json;

namespace CnGalWebSite.DataModel.Application.Helper
{
    public class AppHelper : IAppHelper
    {
        private readonly HttpClient _httpClient;

        public AppHelper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<UploadResult>> UploadFilesAsync(List<IBrowserFile> files, long maxFileSize, int maxAllowedFiles, string url)
        {
            var upload = false;

            using var content = new MultipartFormDataContent();

            foreach (var file in files)
            {
                var fileContent = new StreamContent(file.OpenReadStream(file.Size));

                if (file.Size < maxFileSize)
                {
                    content.Add(
                        content: fileContent,
                        name: "\"files\"",
                        fileName: file.Name);

                    upload = true;
                }
            }

            if (upload)
            {
                var response = await _httpClient.PostAsync(url, content);

                var newUploadResults = await response.Content
                    .ReadFromJsonAsync<List<UploadResult>>();

                return newUploadResults;
            }
            else
            {
                return new List<UploadResult> { new UploadResult { Uploaded = false, Error = "文件没有上传" } };
            }
        }

        public async Task<List<UploadResult>> UploadFilesAsync(string base64, ImageAspectType type)
        {
            //复制数据
            var model = new ImageBase64UploadModel
            {
                Base64Str = base64
            };
            switch (type)
            {
                case ImageAspectType._1_1:
                    model.Height = 1;
                    model.Width = 1;
                    break;
                case ImageAspectType._16_9:
                    model.Width = 460;
                    model.Height = 215;

                    break;
                case ImageAspectType._9_16:
                    model.Width = 9;
                    model.Height = 16;
                    break;
                case ImageAspectType._4_1A2:
                    model.Width = 4;
                    model.Height = 1.2;

                    break;
            }

            var result = await _httpClient.PostAsJsonAsync<ImageBase64UploadModel>(ToolHelper.WebApiPath + "api/files/PostFileBase64", model);
            var jsonContent = result.Content.ReadAsStringAsync().Result;
            var obj = JsonSerializer.Deserialize<List<UploadResult>>(jsonContent, ToolHelper.options);

            return obj;
        }


        /// <summary>
        /// 查IP国家/地区
        /// </summary>
        /// <param name="strIP"></param>
        /// <returns></returns>
        public async Task<string> GetIPCountiy(string strIP = "")
        {
            try
            {
                var Text = await _httpClient.GetStringAsync(ToolHelper.WebApiPath + "api/home/GetIPCitys/" + strIP);
                return Text;
            }
            catch
            {
                return "未知";
            }
        }

    }
}
