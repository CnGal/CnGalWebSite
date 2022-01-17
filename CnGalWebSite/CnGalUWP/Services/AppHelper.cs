using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CnGalUWP.Services
{
    public class AppHelper
    {
        public const string ApiBaseUrl = "https://www.cngal.org/";

        public async Task<T> GetDataAsync<T>(string url) where T : class
        {
            using (var client = new HttpClient())
            {
                var str = await client.GetStringAsync(ApiBaseUrl + url);
                T model = null;
                using (TextReader textReader = new StringReader(str))
                {
                    var serializer = new JsonSerializer();
                    model = (T)serializer.Deserialize(textReader, typeof(T));
                }

                return model;
            }

        }
    }
}
