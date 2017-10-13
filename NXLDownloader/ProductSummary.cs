using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MapleStoryFullDownloaderNXL
{
    public class ProductSummary
    {
        [JsonProperty(PropertyName = "product_id")]
        public string ProductId;
        [JsonProperty(PropertyName = "product_name")]
        public string ProductName;
        [JsonProperty(PropertyName = "game_status")]
        public string Status;
        [JsonProperty(PropertyName = "button_status")]
        public string ButtonStatus;
        [JsonProperty(PropertyName = "button_text")]
        public string ButtonText = "PLAY NOW";
        [JsonProperty(PropertyName = "button_link")]
        public string ButtonLink = "";
        public string[] Locales;
        public string ProductLink { get => $"http://api.nexon.io/products/{ProductId}"; }
    }
}
