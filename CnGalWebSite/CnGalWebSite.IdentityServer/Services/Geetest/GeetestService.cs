using CnGalWebSite.IdentityServer.Models.DataModels.Geetest;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace CnGalWebSite.IdentityServer.Services.Geetest
{
    public class GeetestService : IGeetestService
    {
        private readonly GeetestOptions _options;

        public GeetestService(IOptions<GeetestOptions> options)
        {
            _options = options.Value;
        }

        public GeetestCodeModel GetGeetestCode(GeetestScenario scenario)
        {
            var credential = _options.GetCredential(scenario);
            var model = new GeetestCodeModel
            {
                Gt = credential.Id,
                Challenge = "",
                Success = "1"
            };

            return model;
        }

        public bool CheckRecaptcha(HumanMachineVerificationResult model, GeetestScenario scenario)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.LotNumber) || string.IsNullOrWhiteSpace(model.CaptchaOutput) || string.IsNullOrWhiteSpace(model.PassToken) || string.IsNullOrWhiteSpace(model.GenTime))
            {
                return false;
            }

            try
            {
                var credential = _options.GetCredential(scenario);

                // GT4 validation
                string sign_token = HmacSha256HMAC(model.LotNumber, credential.Key);
                
                var values = new Dictionary<string, string>
                {
                    { "lot_number", model.LotNumber },
                    { "captcha_output", model.CaptchaOutput },
                    { "pass_token", model.PassToken },
                    { "gen_time", model.GenTime },
                    { "sign_token", sign_token }
                };

                using var client = new HttpClient();
                var content = new FormUrlEncodedContent(values);
                var url = $"http://gcaptcha4.geetest.com/validate?captcha_id={credential.Id}";
                var response = client.PostAsync(url, content).GetAwaiter().GetResult();
                var responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                var resultObj = JObject.Parse(responseString);
                return resultObj["result"]?.ToString() == "success";
            }
            catch
            {
                return false;
            }
        }

        private string HmacSha256HMAC(string message, string secret)
        {
            var encoding = new UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
            }
        }
    }
}
