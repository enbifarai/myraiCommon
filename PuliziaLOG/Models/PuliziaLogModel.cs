using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuliziaLOG.Models
{
    public class Attachment
    {
        [JsonProperty( "color" )]
        public string Color { get; set; }

        [JsonProperty( "title" )]
        public string Title { get; set; }

        [JsonProperty( "text" )]
        public string Text { get; set; }
    }

    public class PayloadAdvanced
    {
        [JsonProperty( "channel" )]
        public string Channel { get; set; }

        [JsonProperty( "username" )]
        public string Username { get; set; }

        [JsonProperty( "text" )]
        public string Text { get; set; }

        [JsonProperty( "attachments" )]
        public Attachment[] Attachments { get; set; }
    }
}
