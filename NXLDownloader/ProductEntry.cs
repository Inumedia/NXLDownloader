using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NXLDownloader
{
    public class ProductEntry
    {
        [JsonProperty(PropertyName = "product_id")]
        public string ProductId;
        public string ip_block_status;
        public bool IsIpBlock { get => ip_block_status.Equals("Y", StringComparison.CurrentCultureIgnoreCase); }
        public string ProductLink { get => $"http://api.nexon.io/products/{ProductId}"; }
    }
}
