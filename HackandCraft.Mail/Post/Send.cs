using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using mandrill.net.Model;

namespace mandrill.net
{
    internal static class Send
    {
        
        private static string mandrillUrl = Config.Instance.apiUrl;

        internal static List<MandrillResponse> send(string template)
        {
            var client = new WebClient();
            client.Headers.Add("Content-Type:application/json");
            byte[] bytedata = Encoding.UTF8.GetBytes(template);
            byte[] responseArray = client.UploadData(mandrillUrl, "POST", bytedata);
            string response = Encoding.UTF8.GetString(responseArray);
            return JsonConvert.DeserializeObject<List<MandrillResponse>>(response);

        }
    }
}
