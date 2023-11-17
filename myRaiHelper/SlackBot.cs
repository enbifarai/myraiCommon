using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Net;
using System.Text;


namespace myRaiHelper
{
    
    public class SlackBot
    {
       // public  static readonly Uri _uri;
       // private static readonly Encoding _encoding = new UTF8Encoding();

       // public SlackBot()
       // {
      //      _uri = new Uri("https://hooks.slack.com/services/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");
      //  }

        
        public static void PostMessage(string text, string username = "RAIPERME", string channel = "#general")
        {
            Payload payload = new Payload()
            {
                Channel = channel,
                Username = username,
                Text = text
            };

            PostMessage(payload);
        }

        
        public static void PostMessage(Payload payload)
        {
            if (HttpContext.Current.Request.Url.Host.Contains("localhost")) return;

            string payloadJson = JsonConvert.SerializeObject(payload);

            using (WebClient client = new WebClient())
            {
                NameValueCollection data = new NameValueCollection();
                data["payload"] = payloadJson;

                var response = client.UploadValues("https://hooks.slack.com/services/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",
                "POST", data);

                //The response text is usually "ok"
                string responseText = new UTF8Encoding().GetString(response);
            }
        }
    }

    
    public class Payload
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

	
  //string urlWithAccessToken = "https://hooks.slack.com/services/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";
	
  //  SlackBot client = new SlackBot(urlWithAccessToken);
	
  //  client.PostMessage(username: "Mr. Torgue",
  //			 text: "THIS IS A TEST MESSAGE! SQUEEDLYBAMBLYFEEDLYMEEDLYMOWWWWWWWW!",
  //			 channel: "#general");
}