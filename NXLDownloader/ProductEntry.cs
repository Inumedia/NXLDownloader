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
        public bool IsIpBlock;
        public string ProductLink;
    }
}
