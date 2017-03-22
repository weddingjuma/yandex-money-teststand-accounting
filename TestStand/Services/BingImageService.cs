using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TestStand.Services
{
    public class BingImageService
    {
        private const string BingApiBase = "http://www.bing.com";

        public async Task<string> GetPictureOfDayAsync()
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync($"{BingApiBase}/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-US");

            var json = JObject.Parse(response);

            var imageToken = json.SelectToken("images[0].url");
            if (imageToken != null)
                return $"{BingApiBase}/{imageToken.ToString()}";

            return null;
        }
    }
}
