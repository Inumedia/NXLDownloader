using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MapleStoryFullDownloaderNXL
{
    public class Product
    {
        [JsonProperty(PropertyName = "product_id")]
        public string ProductId;
        [JsonProperty(PropertyName = "product_name")]
        public string ProductName;
        [JsonProperty(PropertyName = "client_id")]
        public string ClientId;
        [JsonProperty(PropertyName = "product_no")]
        public string InternalProductNumber;
        [JsonProperty(PropertyName = "display_name")]
        public string FriendlyProductName;
        [JsonProperty(PropertyName = "service_code")]
        public string ServiceCode;
        [JsonProperty(PropertyName = "blocking_eu_user")]
        public bool ShouldBlockEUUsers;
        [JsonProperty(PropertyName = "is_public")]
        public bool IsPublic;
        [JsonProperty(PropertyName = "product_details")]
        public ProductDetails Details;
    }
}
