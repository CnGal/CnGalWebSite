using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Accounts;
using CnGalWebSite.DataModel.ViewModel.Space;
using CnGalWebSite.Helper.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.PostTools
{
    public class AccountX
    {
        private readonly SettingX setting;
        private readonly HttpClient client;

        public AccountX(HttpClient _client, SettingX _setting)
        {
            client = _client;
            setting = _setting;
        }

        public async Task LoginAsync()
        {
            while (true)
            {
                if (string.IsNullOrWhiteSpace(setting.Token))
                {
                    Console.WriteLine("请输入登入令牌，可以在个人空间->编辑个人资料->获取登入令牌中获取，输入完毕后请按下回车");


                    var token = Console.ReadLine();
                    try
                    {
                        var result = await client.PostAsJsonAsync<OneTimeCodeModel>(ToolHelper.WebApiPath + "api/account/LoginByOneTimeCode", new OneTimeCodeModel { Code = token });
                        var jsonContent = result.Content.ReadAsStringAsync().Result;
                        var obj = System.Text.Json.JsonSerializer.Deserialize<LoginResult>(jsonContent, ToolHelper.options);
                        //判断结果
                        if (obj.Code == LoginResultCode.OK)
                        {
                            setting.Token = obj.Token;
                        }
                        else
                        {
                            throw new Exception(obj.ErrorInfor);
                        }
                    }
                    catch (Exception ex)
                    {
                        OutputHelper.PressError(ex, "登入失败", "令牌无效");
                        continue;
                    }
                }

                try
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", setting.Token);

                    //调用刷新接口
                    var result = await client.GetFromJsonAsync<LoginResult>(ToolHelper.WebApiPath + "api/account/RefreshJWToken");
                    if (result.Code == LoginResultCode.OK)
                    {
                        setting.Token = result.Token;

                        break;
                    }
                    else
                    {
                        setting.Token = null;


                    }
                }
                catch (Exception ex)
                {
                    setting.Token = null;

                    OutputHelper.PressError(ex, "登入失败", "令牌无效");
                }
            }

            try
            {
                var model = await client.GetFromJsonAsync<PersonalSpaceViewModel>(ToolHelper.WebApiPath + "api/space/GetUserView/");
                setting.UserName = model.BasicInfor.Name;
            }
            catch (Exception ex)
            {
                setting.Token = null;

                OutputHelper.PressError(ex, "获取用户信息失败");
            }

            setting.Save();
        }

        public void Logout()
        {
            setting.Token = null;
            setting.Save();
        }
    }
}
